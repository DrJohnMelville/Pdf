using Melville.INPC;

namespace Melville.Parsing.ParserMapping;

/// <summary>
/// This class is used to represent a Sequence of bytes in the parsed content
/// </summary>
public partial class ParseMapEntry: ParseMapEntryBase
{
    /// <inheritdoc />
    [FromConstructor] public override int StartPos { get; }

    /// <inheritdoc />
    [FromConstructor] public override int NextPos { get; }
}