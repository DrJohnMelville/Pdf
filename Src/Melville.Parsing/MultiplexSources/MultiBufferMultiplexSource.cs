using Melville.StreamInterfaces;
using System.Buffers;

namespace Melville.Parsing.MultiplexSources;

internal class MultiBufferMultiplexSource(IAsyncReader reader) : IndexedMultiplexSource
{
    private const int rowWidth = 4096;
    int lastEndPoint = rowWidth;
    private readonly List<byte[]> buffers = new();

    public override long Length
    {
        get
        {
            EnsurePosition(long.MaxValue);
            return CurrentLength;
        }
    }

    private int CurrentLength => (rowWidth * (buffers.Count - 1)) + rowWidth;

    (int row, int col) ComputePosition(long position) => Math.DivRem((int)position, rowWidth);

    public void EnsurePosition(long position)
    {
        while (CurrentLength < position)
        {
            TryExpandPool(); 
            var read = Read(NextReadBuffer());
            lastEndPoint += read;
            if (read is 0) return;
        }
    }
    public async ValueTask EnsurePositionAsync(long position)
    {
        while (CurrentLength < position)
        {
            TryExpandPool();
            var read = await reader.ReadAsync(NextReadBuffer()).CA();
            lastEndPoint += read;
            if (read is 0) return;
        }
    }

    private Memory<byte> NextReadBuffer()
    {
        return buffers[buffers.Count - 1].AsMemory(0, rowWidth)[lastEndPoint..];
    }

    private void TryExpandPool()
    {
        if (lastEndPoint >= rowWidth)
        {
            buffers.Add(ArrayPool<byte>.Shared.Rent(rowWidth));
            lastEndPoint = 0;
        }
    }

    int Read(Memory<byte> memory)
    {
        if (reader is ISyncReader syncRead) return syncRead.Read(memory.Span);
        return  reader.ReadAsync(memory).GetAwaiter().GetResult();
    }

    public override int Read(long position, in Span<byte> buffer)
    {
        EnsurePosition(position + buffer.Length);
        return DoRead(position, buffer);
    }

    private int DoRead(long position, Span<byte> buffer)
    {
        var (line, col) = ComputePosition(position);
        var length = (line + 1) == buffers.Count ? lastEndPoint : rowWidth;
        var source = buffers[line].AsSpan(0, length).TakeMax(buffer.Length, out var copyLength);
        source.CopyTo(buffer);
        return copyLength;
    }

    public override async ValueTask<int> ReadAsync(long position, Memory<byte> buffer, CancellationToken cancellationToken)
    {
        await EnsurePositionAsync(position + buffer.Length);
        return DoRead(position, buffer.Span);
    }

    protected override void CleanUp()
    {
        foreach (var buf in buffers)
        {
            ArrayPool<byte>.Shared.Return(buf);
        }
        (reader as IDisposable)?.Dispose();
    }
}
