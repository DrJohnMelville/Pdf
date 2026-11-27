using Melville.StreamInterfaces;
using Melville.StreamInterfaces.Files;
using Microsoft.Win32.SafeHandles;
using System.Buffers;
using System.Collections.Concurrent;
using System.Reflection.Metadata;

namespace Melville.Parsing.MultiplexSources;

internal class FileMultiplexer : IndexedMultiplexSource
{
    private SafeFileHandle handle;
   
    public FileMultiplexer(FileStream stream): this(stream.SafeFileHandle) {
        stream.Dispose();
    }
    public FileMultiplexer(SafeFileHandle streamHandle)
    {
        handle = streamHandle;        
        bool succeeded = false;
        handle.DangerousAddRef(ref succeeded);
        if (!succeeded)
            throw new IOException("Failed to attach to file handle");
    }

    protected override void CleanUp() => handle.DangerousRelease();

    public override long Length => RandomAccess.GetLength(handle);

    public override int Read(long position, in Span<byte> buffer) =>
        RandomAccess.Read(handle, buffer, position);

    public override ValueTask<int> ReadAsync(long position, Memory<byte> buffer, CancellationToken cancellationToken) =>
        RandomAccess.ReadAsync(handle, buffer, position, cancellationToken);
}
