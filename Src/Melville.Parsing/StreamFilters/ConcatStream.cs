namespace Melville.Parsing.StreamFilters;

/// <summary>
/// A stream that takes an IEnumerable of stream and concatenates them into a single stream.
/// </summary>
public class ConcatStream : ConcatStreamBase
{
    private IEnumerator<Stream> items;

    /// <summary>
    /// Create a ConcatStream from a sequence of streams.
    /// </summary>
    /// <param name="items">Streams to be concatenated</param>
    public ConcatStream(params Stream[] items) : this(items.AsEnumerable())
    {
    }

    /// <summary>
    /// Create a ConcatStream from a sequence of streams.
    /// </summary>
    /// <param name="items">Streams to be concatenated</param>
    public ConcatStream(IEnumerable<Stream> items)
    {
        this.items = items.GetEnumerator();
    }

    /// <inheritdoc />
    protected override ValueTask<Stream?> GetNextStreamAsync() => new(items.MoveNext() ? items.Current : null);
}