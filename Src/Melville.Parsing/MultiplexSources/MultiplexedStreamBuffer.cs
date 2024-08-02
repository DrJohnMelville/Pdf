using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.Streams;
using Melville.Parsing.Streams.Bases;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Melville.Parsing.MultiplexSources;

internal class MultiplexedStreamBuffer(Stream innerStream, int blockLength = 4096) :
    IMultiplexSource
{
    private readonly MultiBuffer multiBuffer = new(blockLength);
    private readonly SemaphoreSlim mutex = new(1);

    public void Dispose() => innerStream.Dispose();

    public long Length => innerStream.Length;

    public Stream ReadFrom(long position) => 
        new Reader(this){Position = position};


    private bool TryExtendTo(long position)
    {
        mutex.Wait();

        try
        {
            while (multiBuffer.Length < position)
            {
                if (!TryExtendSync()) return false;
            }
            return true;
        }
        finally
        {
            mutex.Release();
        }
    }
    private async ValueTask<bool> TryExtendToAsync(long position, CancellationToken ct)
    {
        await mutex.WaitAsync().CA();
        try
        {
            while (multiBuffer.Length < position)
            {
                if (!await TryExtendAsync(ct).CA()) return false;
            }
            return true;
        }
        finally
        {
            mutex.Release();
        }
    }
    

    private bool TryExtendSync() => multiBuffer.ExtendStreamFrom(innerStream);

    private ValueTask<bool> TryExtendAsync(CancellationToken cancellationToken) =>
        multiBuffer.ExtendFromAsync(innerStream, cancellationToken);


    private class Reader(MultiplexedStreamBuffer parent) : DefaultBaseStream(true, false, true)
    {
        private readonly MultiBufferStream2 source = new (parent.multiBuffer);

        public override long Length => parent.Length;

        public override int Read(Span<byte> buffer)
        {
            parent.TryExtendTo(Position + buffer.Length);
            source.Position = Position;
            return UpdatePosition(source.Read(buffer));
        }
        public override async ValueTask<int> ReadAsync(
            Memory<byte> buffer, CancellationToken cancellationToken = new())
        {
            await parent.TryExtendToAsync(Position + buffer.Length, cancellationToken).CA();
            source.Position = Position;
            return UpdatePosition(await source.ReadAsync(buffer, cancellationToken).CA());
        }

        private int UpdatePosition(int bytesRead)
        {
            Position += bytesRead;
            return bytesRead;
        }

        public override long Seek(long offset, SeekOrigin origin) => 
            Position = offset + this.SeekOriginLocation(origin);
    }

}