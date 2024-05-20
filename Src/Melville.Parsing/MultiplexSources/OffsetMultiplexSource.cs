using Melville.Parsing.MultiplexSources;

public class OffsetMultiplexSource(IMultiplexSource inner, long offset) : IMultiplexSource
{

    /// <inheritdoc />
    public void Dispose() => inner.Dispose();

    /// <inheritdoc />
    public Stream ReadFrom(long position) => inner.ReadFrom(position + offset);

    /// <inheritdoc />
    public long Length => inner.Length - offset;
}