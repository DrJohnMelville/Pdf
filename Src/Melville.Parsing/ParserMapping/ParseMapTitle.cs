using Melville.INPC;

namespace Melville.Parsing.ParserMapping
{
    public partial class ParseMapTitle : ParseMapEntryBase
    {
        [FromConstructor] public ParseMapTitle? Parent { get; }
        private readonly List<ParseMapEntryBase> items = new();
        public IReadOnlyList<ParseMapEntryBase> Items => items;
        public override int StartPos => items.FirstOrDefault()?.StartPos ?? 0;
        public override int NextPos => items.LastOrDefault()?.NextPos ?? 0;
        public void Add(ParseMapEntryBase item) => items.Add(item);
    }
}