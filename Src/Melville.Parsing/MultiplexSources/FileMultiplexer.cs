using Melville.StreamInterfaces;
using Melville.StreamInterfaces.Files;
using Microsoft.Win32.SafeHandles;

namespace Melville.Parsing.MultiplexSources;

internal class FileMultiplexer(FileStream stream) : CountedMultiplexSource, IIndexedReader
{
    private readonly FileStream stream = stream; // hold onto the stream so it does not get prematurely disposed
    private SafeFileHandle handle => stream.SafeFileHandle;

    protected override void CleanUp()
    {
        stream.Dispose();
    }

    protected override Stream ReadFromOverride(long position, CountedSourceTicket ticket) => 
        new IndexedReaderStream(this, position, ticket);

    protected override IAsyncReader ReaderFromOverride(long position, CountedSourceTicket ticket)
    {
        return new IndexedReader(this, position, ticket);
    }


    public override long Length => stream.Length;

    int IIndexedReader.Read(long position, in Span<byte> buffer) =>
        RandomAccess.Read(handle, buffer, position);

    ValueTask<int> IIndexedReader.ReadAsync(long position, Memory<byte> buffer, CancellationToken cancellationToken)
    {
        return RandomAccess.ReadAsync(handle, buffer, position, cancellationToken);
    }
}
