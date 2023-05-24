using System;
using System.IO;
using System.IO.Compression;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.Streams.Bases;

namespace Melville.Pdf.LowLevel.Filters.FlateFilters;

internal sealed class FlateEncodeWrapper: DefaultBaseStream
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
            
    public FlateEncodeWrapper(Stream source): base(true, false,false)
    {
        this.source = source;
        state = State.WritePrefix;
        adler = new ReadAdlerStream(source);
        reverser = new Pipe();
        deflator = new DeflateStream(reverser.Writer.AsStream(), CompressionLevel.Optimal);
        InitiateCopyProcessAsync();
        compressedSource = reverser.Reader.AsStream();
    }

    private async void InitiateCopyProcessAsync()
    {
        await adler.CopyToAsync(deflator).CA();
        await deflator.DisposeAsync().CA();
        await reverser.Writer.CompleteAsync().CA();
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
            State.WritePrefix => await TryWritePrefixAsync(buffer, cancellationToken).CA(),
            State.CopyBytes => await CopyBytesAsync(buffer, cancellationToken).CA(),
            State.WriteTrailer => await WriteTrailerAsync(buffer).CA(),
            State.Done => 0,
            _ => throw new ArgumentOutOfRangeException()
        };

    private ValueTask<int> WriteTrailerAsync(Memory<byte> buffer)
    {
        adler.Computer.CopyHashToBigEndianSpan(buffer.Span);
        state = State.Done;
        return new ValueTask<int>(4);
    }

    private async ValueTask<int> CopyBytesAsync(Memory<byte> buffer, CancellationToken cancellationToken)
    {
        var ret = await compressedSource.ReadAsync(buffer).CA();
        if (ret == 0)
        {
            state = State.WriteTrailer;
            // we need the "tail call" here so we do not return 0 before we write out the trailer.
            return await ReadAsync(buffer, cancellationToken).CA();
        }
        return ret;
    }

    private async ValueTask<int> TryWritePrefixAsync(Memory<byte> destination, CancellationToken cancellationToken)
    {
        CopyPrefixToMemory(destination);
        state = State.CopyBytes;
        // this is an optimization -- we cold just always return 2 bytes from the first read.
        // we choose to try and fill the buffer as much as we can.
        return 2 + await ReadAsync(destination[2..], cancellationToken).CA();
    }

    private static void CopyPrefixToMemory(in Memory<byte> destination)
    {
        var span = destination.Span;
        span[0] = 0x78;
        span[1] = 0xDA;
    }
}