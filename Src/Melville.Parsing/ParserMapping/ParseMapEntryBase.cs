using Melville.INPC;

namespace Melville.Parsing.ParserMapping
{
    public abstract partial class ParseMapEntryBase
    {
        [FromConstructor] public string Title { get; }
        public abstract int StartPos { get; }
        public abstract int NextPos { get; }
    }
}