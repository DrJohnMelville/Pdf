using Melville.INPC;

namespace Melville.Parsing.ParserMapping
{
    public partial class ParseMapEntry: ParseMapEntryBase
    {
        [FromConstructor] public override int StartPos { get; }
        [FromConstructor] public override int NextPos { get; }
    }
}