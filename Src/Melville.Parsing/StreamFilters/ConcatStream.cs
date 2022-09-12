using System.Diagnostics.CodeAnalysis;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.Streams.Bases;

namespace Melville.Parsing.StreamFilters;

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
public abstract class ConcatStreamBase : DefaultBaseStream
{
    private Stream? current = null;

    protected ConcatStreamBase(): base(true, false, false)
    {
        current = this;
    }

    public override async ValueTask<int> ReadAsync(
        Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        await TryLoadFirstSource().CA();
        var bytesWritten = 0;
        while (!AtEndOfStream() && bytesWritten < buffer.Length)
        {
            var localWritten = await current.ReadAsync(buffer[bytesWritten..], cancellationToken).CA();
            bytesWritten += localWritten;
            await PrepareForNextRead(localWritten).CA();
        }

        return bytesWritten;
    }

    private async ValueTask TryLoadFirstSource()
    {
        // we use this as a sentinel to mean we have not gotten the first source yet
        current = (current == this) ? await GetNextStream().CA(): current;
    }
    
    [MemberNotNullWhen(false, nameof(current))]
    private bool AtEndOfStream() => current is null;

    private async ValueTask PrepareForNextRead(int localWritten)
    {
        if (PriorReadSucceeded(localWritten)) return;
        if (current != null) await current.DisposeAsync().CA();
        current = await GetNextStream().CA();;
    }

    private static bool PriorReadSucceeded(int localWritten) => localWritten > 0;

    protected override void Dispose(bool disposing)
    {
        current?.Dispose();
        base.Dispose(disposing);
    }

    protected abstract ValueTask<Stream?> GetNextStream();
}