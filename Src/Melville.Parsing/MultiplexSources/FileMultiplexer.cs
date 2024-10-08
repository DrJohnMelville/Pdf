﻿using Microsoft.Win32.SafeHandles;

namespace Melville.Parsing.MultiplexSources;

internal class FileMultiplexer(FileStream stream) : CountedMultiplexSource, IIndexedReader
{
    private readonly SafeFileHandle handle = stream.SafeFileHandle;

    protected override void CleanUp()
    {
        handle.Dispose();
        stream.Dispose();
    }

    protected override Stream ReadFromOverride(long position, CountedSourceTicket ticket) => 
        new IndexedReaderStream(this, position, ticket);

    public override long Length => stream.Length;

    int IIndexedReader.Read(long position, in Span<byte> buffer) =>
        RandomAccess.Read(handle, buffer, position);

    ValueTask<int> IIndexedReader.ReadAsync(long position, Memory<byte> buffer, CancellationToken cancellationToken) =>
        RandomAccess.ReadAsync(handle, buffer, position, cancellationToken);
}