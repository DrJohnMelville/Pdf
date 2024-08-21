using System.Buffers;
using System.IO.Pipelines;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.InteropServices.Marshalling;
using Melville.Fonts.SfntParsers.TableDeclarations.CFF2Glyphs;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.CountingReaders;
using Melville.Parsing.MultiplexSources;
using Melville.Parsing.ObjectRentals;
using Melville.Parsing.PipeReaders;
using Melville.Postscript.Interpreter.Tokenizers;

namespace Melville.Fonts.Type1TextParsers.EexecDecoding
{
    internal sealed partial class EexeDecisionSource : IByteSource
    {
        [FromConstructor] [DelegateTo] private IByteSource inner;
        [FromConstructor] private readonly IMultiplexSource multiplexSource;
        [FromConstructor] private Action<IByteSource>? setReader;
        [FromConstructor] private readonly ushort key;

        public bool TryRead(out ReadResult result) =>
            throw new NotSupportedException("Must read asynchronously");

        public ReadResult Read() =>
            throw new NotSupportedException("Must read asynchronously");

        public async ValueTask<ReadResult> ReadAsync()
        {
            if (setReader is not null)
            {
                await CreateDecodingReaderAsync().CA();
            }

            return await inner.ReadAsync().CA();
        }

        private async Task CreateDecodingReaderAsync()
        {
            await SkipWhitespaceAsync().CA();

            // Determine if the encoded stream is hex encoded
            var result = await inner.ReadAtLeastAsync(4).CA();
            var isHex = IsHex(result.Buffer.Slice(0, 4));

            // construct the decrypting stream
            var newInner = ConstructDecryptingStream(isHex);

            // skip the first four (random) bytes
            result = await newInner.ReadAtLeastAsync(4).CA();
            newInner.AdvanceTo(result.Buffer.GetPosition(4));

            // replace the existing with the new reader
            setReader?.Invoke(newInner);
            setReader = null;
            inner.Dispose();
            inner = newInner;
        }

        private IByteSource ConstructDecryptingStream(bool isHex)
        {
            var encodedStream = multiplexSource.ReadFrom(inner.Position);
            return ConstructNewByteSource(isHex ? new DecodeHexStream(encodedStream) : encodedStream,
                (int)inner.Position);
        }

        private IByteSource ConstructNewByteSource(Stream input, int startpos)
        {
            return 
                MultiplexSourceFactory.SingleReaderForStream(new EexecDecodeStream(
                    input, key), false).WithCurrentPosition(startpos);
        }

        private bool IsHex(ReadOnlySequence<byte> buffer)
        {
            foreach (var item in buffer)
            {
                if (item.Span.ContainsAnyExcept(CharacterClassifier.HexDigits)) return false;
            }

            return true;
        }

        private async Task SkipWhitespaceAsync()
        {
            ReadResult result;
            do
            {
                result = await inner.ReadAsync().CA();
            } while (AdvancePastWhiteSpace(result.Buffer));
        }

        private bool AdvancePastWhiteSpace(ReadOnlySequence<byte> buffer)
        {
            var reader = new SequenceReader<byte>(buffer);
            var ret = reader.AdvancePastAny(CharacterClassifier.WhiteSpaceChars());
            inner.AdvanceTo(reader.Position);
            return ret >= buffer.Length;
        }
    }
}