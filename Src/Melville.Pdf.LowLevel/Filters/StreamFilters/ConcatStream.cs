using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Melville.Pdf.LowLevel.Filters.StreamFilters;

public class ConcatStream : ConcatStreamBase
{
    private IEnumerator<Stream> items;

    public ConcatStream(params Stream[] items) : this(items.AsEnumerable())
    {
    }

    public ConcatStream(IEnumerable<Stream> items)
    {
        this.items = items.GetEnumerator();
    }
    protected override ValueTask<Stream?> GetNextStream() => new(items.MoveNext() ? items.Current : null);
}
public abstract class ConcatStreamBase : SequentialReadFilterStream
{
    private Stream? current = null;

    protected ConcatStreamBase()
    {
        current = this;
    }

    public override async ValueTask<int> ReadAsync(
        Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        await TryLoadFirstSource();
        var bytesWritten = 0;
        while (!AtEndOfStream() && bytesWritten < buffer.Length)
        {
            var localWritten = await current.ReadAsync(buffer[bytesWritten..], cancellationToken);
            bytesWritten += localWritten;
            await PrepareForNextRead(localWritten);
        }

        return bytesWritten;
    }

    private async ValueTask TryLoadFirstSource()
    {
        // we use this as a sentinel to mean we have not gotten the first source yet
        current = (current == this) ? await GetNextStream(): current;
    }
    
    [MemberNotNullWhen(false, nameof(current))]
    private bool AtEndOfStream() => current is null;

    private async ValueTask PrepareForNextRead(int localWritten)
    {
        if (PriorReadSucceeded(localWritten)) return;
        if (current != null) await current.DisposeAsync();
        current = await GetNextStream();;
    }

    private static bool PriorReadSucceeded(int localWritten) => localWritten > 0;

    protected override void Dispose(bool disposing)
    {
        current?.Dispose();
        base.Dispose(disposing);
    }

    protected abstract ValueTask<Stream?> GetNextStream();
}