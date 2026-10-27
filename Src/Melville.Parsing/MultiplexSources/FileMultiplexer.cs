using Melville.StreamInterfaces;
using Melville.StreamInterfaces.Files;
using Microsoft.Win32.SafeHandles;
using System.Collections.Concurrent;

namespace Melville.Parsing.MultiplexSources;

internal class FileMultiplexer : CountedMultiplexSource, IIndexedReader
{
    private SafeFileHandle handle;
   
    public FileMultiplexer(FileStream stream)
    {
        handle = stream.SafeFileHandle;        
        bool succeeded = false;
        handle.DangerousAddRef(ref succeeded);
        if (!succeeded)
            throw new IOException("Failed to attach to file handle");
    }

    protected override void CleanUp() => handle.DangerousRelease();

    protected override Stream ReadFromOverride(long position) => 
        new IndexedReaderStream(this, position);

    protected override IAsyncReader ReaderFromOverride(long position)
    {
        return new IndexedReader(this, position);
    }


    public override long Length => RandomAccess.GetLength(handle);

    int IIndexedReader.Read(long position, in Span<byte> buffer) =>
        RandomAccess.Read(handle, buffer, position);

    ValueTask<int> IIndexedReader.ReadAsync(long position, Memory<byte> buffer, CancellationToken cancellationToken) =>
        RandomAccess.ReadAsync(handle, buffer, position, cancellationToken);
}
