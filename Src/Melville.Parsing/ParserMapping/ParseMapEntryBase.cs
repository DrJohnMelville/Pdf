using Melville.INPC;

namespace Melville.Parsing.ParserMapping;

/// <summary>
/// Represents a node in the parse map tree.
/// </summary>
public abstract partial class ParseMapEntryBase
{
    /// <summary>
    /// The title of this item
    /// </summary>
    [FromConstructor] public string Title { get; }

    /// <summary>
    /// Index of the first byte of the content.
    /// </summary>
    public abstract int StartPos { get; }

    /// <summary>
    /// index of the byte directly following the content.
    /// </summary>
    public abstract int NextPos { get; }
}