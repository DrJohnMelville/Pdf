using Melville.INPC;

namespace Melville.Parsing.ParserMapping;

/// <summary>
/// Represents a node of the parse map tree with children.
/// </summary>
public partial class ParseMapTitle : ParseMapEntryBase
{
    /// <summary>
    /// The parent of this node, or null if this is the root.
    /// </summary>
    [FromConstructor] public ParseMapTitle? Parent { get; }
    private readonly List<ParseMapEntryBase> items = new();

    /// <summary>
    /// Children of this item
    /// </summary>
    public IReadOnlyList<ParseMapEntryBase> Items => items;

    /// <inheritdoc />
    public override int StartPos => items.FirstOrDefault()?.StartPos ?? 0;

    /// <inheritdoc />
    public override int NextPos => items.LastOrDefault()?.NextPos ?? 0;
    
    /// <summary>
    /// Add a child to this item.
    /// </summary>
    /// <param name="item">The child to add.</param>
    public void Add(ParseMapEntryBase item) => items.Add(item);
}