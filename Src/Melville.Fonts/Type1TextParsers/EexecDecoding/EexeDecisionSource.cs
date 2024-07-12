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
using Melville.Parsing.Streams.Bases;
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

        public async ValueTask<ReadResult> ReadAsync(CancellationToken cancellationToken = default)
        {
            if (setReader is not null)
            {
                await CreateDecodingReader().ConfigureAwait(false);
            }

            return await inner.ReadAsync(cancellationToken).CA();
        }

        private async Task CreateDecodingReader()
        {
            await SkipWhitespaceAsync().CA();

            var result = await inner.ReadAtLeastAsync(4).CA();
            var newInner = IsHex(result.Buffer.Slice(0, 4))
                ? ConstructNewByteSource(
                    new DecodeHexStream(multiplexSource.ReadFrom(inner.Position)))
                : ConstructNewByteSource(multiplexSource.ReadFrom(inner.Position));

            result = await newInner.ReadAtLeastAsync(4).CA();
            newInner.AdvanceTo(result.Buffer.GetPosition(4));
            setReader?.Invoke(newInner);
            inner = newInner;
            setReader = null;
        }

        private IByteSource ConstructNewByteSource(Stream input)
        {
            return 
                ReusableStreamPipeReader.Create(new EexecDecodeStream(
                        input, key), false);
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

internal class DecodeHexStream(Stream readFrom) : DefaultBaseStream(true, false, false)
{
    PipeReader reader = PipeReader.Create(readFrom);

    public override int Read(Span<byte> buffer)
    {
        throw new NotImplementedException("Must read async");
    }

    public override async ValueTask<int> ReadAsync(
        Memory<byte> buffer, CancellationToken cancellationToken = new CancellationToken())
    {
        int pos = 0;
        while (pos < buffer.Length)
        {
            var result = await reader.ReadAsync(cancellationToken).CA();
//            if (result.Buffer.Length == 0) break;
            var (consumed, examined, written) = CopyToBuffer(buffer.Span[pos..], result.Buffer);
            pos += written;
            if (result.IsCompleted) break;
            reader.AdvanceTo(
                result.Buffer.GetPosition(consumed),
                result.Buffer.GetPosition(examined));
        }

        return pos;
    }

    private (int BytesConsumed, int BytesExamined, int BytesWritten)
        CopyToBuffer(Span<byte> span, ReadOnlySequence<byte> resultBuffer)
    {
        var bytesWritten = 0;
        var bytesExamined = 0;
        var bytesConsumed = 0;
        int partial = -1;
        foreach (var inputMemory in resultBuffer)
        {
            foreach (var singleByte in inputMemory.Span)
            {
                switch (partial, ValueFromDigit(singleByte))
                {
                    case (_, 255): break;
                    case (-1, var high):
                        partial = high << 4;
                        break;
                    case (_, var low):
                        span[bytesWritten++] = (byte)(partial | low);
                        partial = -1;
                        bytesConsumed = bytesExamined + 1;
                        if (bytesWritten == span.Length)
                            return (bytesConsumed, bytesExamined, bytesWritten);
                        break;
                }

                bytesExamined++;
            }
        }

        return (bytesConsumed, bytesExamined, bytesWritten);
    }

    byte ValueFromDigit(byte digitChar) => digitChar switch
    {
        >= (byte)'0' and <= (byte)'9' => (byte)(digitChar - '0'),
        >= (byte)'A' and <= (byte)'F' => (byte)(digitChar - 'A' + 10),
        >= (byte)'a' and <= (byte)'f' => (byte)(digitChar - 'a' + 10),
        _ => byte.MaxValue
    };
}