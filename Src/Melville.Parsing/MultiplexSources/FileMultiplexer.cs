using Microsoft.Win32.SafeHandles;

namespace Melville.Parsing.MultiplexSources;

internal class FileMultiplexer(FileStream stream) : IMultiplexSource, IIndexedReader
{
    private readonly SafeFileHandle handle = stream.SafeFileHandle;

    public void Dispose()
    {
        handle.Dispose();
        stream.Dispose();
    }

    public Stream ReadFrom(long position) => 
        new IndexedReaderStream<FileMultiplexer>(this, position);

    public long Length => stream.Length;

    public int Read(long position, in Span<byte> buffer) =>
        RandomAccess.Read(handle, buffer, position);

    public ValueTask<int> ReadAsync(long position, Memory<byte> buffer, CancellationToken cancellationToken) =>
        RandomAccess.ReadAsync(handle, buffer, position, cancellationToken);
}