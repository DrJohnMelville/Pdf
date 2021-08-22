using System;
using System.IO;
using System.IO.Compression;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Filters.StreamFilters;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Filters.FlateFilters
{
    public class FlateCodecDefinition: ICodecDefinition
    {
        public ValueTask<Stream> EncodeOnReadStream(Stream data, PdfObject? parameters) => 
            new(new MinimumReadSizeFilter(new FlateEncodeWrapper(data), 4));

        public ValueTask<Stream> EncodeOnWriteStream(Stream data, PdfObject? parameters) => 
            new(new FlateEncodeWriteWrapper(data));

        public async ValueTask<Stream> DecodeOnReadStream(Stream input, PdfObject parameters)
        {
            var buffer = new byte[2];
            int totalRead = 0;
            do
            {
                var localRead = await input.ReadAsync(buffer, 0, 2 - totalRead);
                totalRead += localRead;
            } while (totalRead < 2);
            return new DeflateStream(input, CompressionMode.Decompress);
        }
    }
        public sealed class FlateEncodeWrapper: SequentialReadFilterStream
        {

            private enum State
            {
                WritePrefix,
                CopyBytes,
                WriteTrailer,
                Done
            }            
            private State state;
            private readonly Stream source;
            private readonly ReadAdlerStream adler;
            private readonly DeflateStream deflator;
            private readonly Pipe reverser;
            private readonly Stream compressedSource;
            
            public FlateEncodeWrapper(Stream source)
            {
                this.source = source;
                state = State.WritePrefix;
                adler = new ReadAdlerStream(source);
                reverser = new Pipe();
                deflator = new DeflateStream(reverser.Writer.AsStream(), CompressionLevel.Optimal);
                InitiateCopyProcess();
                compressedSource = reverser.Reader.AsStream();
            }

            private async void InitiateCopyProcess()
            {
                await adler.CopyToAsync(deflator);
                await deflator.DisposeAsync();
                await reverser.Writer.CompleteAsync();
            }

            protected override void Dispose(bool disposing)
            {
                source.Dispose();
                base.Dispose(disposing);
            }

            public override ValueTask DisposeAsync()
            {
                source.Dispose();
                return base.DisposeAsync();
            }

            public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = new CancellationToken()) =>
                state switch
                {
                    State.WritePrefix => await TryWritePrefix(buffer, cancellationToken),
                    State.CopyBytes => await CopyBytes(buffer, cancellationToken),
                    State.WriteTrailer => await WriteTrailer(buffer),
                    State.Done => 0,
                    _ => throw new ArgumentOutOfRangeException()
                };

            private ValueTask<int> WriteTrailer(Memory<byte> buffer)
            {
                adler.Computer.CopyHashToBigEndianSpan(buffer.Span);
                state = State.Done;
                return new ValueTask<int>(4);
            }

            private async ValueTask<int> CopyBytes(Memory<byte> buffer, CancellationToken cancellationToken)
            {
                var ret = await compressedSource.ReadAsync(buffer);
                if (ret == 0)
                {
                    state = State.WriteTrailer;
                    // we need the "tail call" here so we do not return 0 before we write out the trailer.
                    return await ReadAsync(buffer, cancellationToken);
                }
                return ret;
            }

            private async ValueTask<int> TryWritePrefix(Memory<byte> destination, CancellationToken cancellationToken)
            {
                CopyPrefixToMemory(destination);
                state = State.CopyBytes;
                // this is an optimization -- we cold just always return 2 bytes from the first read.
                // we choose to try and fill the buffer as much as we can.
                return 2 + await ReadAsync(destination[2..], cancellationToken);
            }

            private static void CopyPrefixToMemory(in Memory<byte> destination)
            {
                var span = destination.Span;
                span[0] = 0x78;
                span[1] = 0xDA;
            }
        }
}