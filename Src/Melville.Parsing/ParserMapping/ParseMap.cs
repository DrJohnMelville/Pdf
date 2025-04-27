using Melville.INPC;

namespace Melville.Parsing.ParserMapping;

public interface IParsMap
{
    public void StopCollecting();
}

public partial class ParseMapEntry
{
    [FromConstructor] public string Title { get; }
    [FromConstructor] public int StartPos { get; }
    [FromConstructor] public int NextPos { get; }
}

public class ParseMap: IParsMap
{
    private readonly HashSet<object> aliases = new();
    private readonly List<ParseMapEntry> entries = new();
    public IList<ParseMapEntry> Entries => entries;

    public void StopCollecting()
    {
    }

    public void AddAlias(object source) => aliases.Add(source);

    public bool MonitoringKey(object key) => aliases.Contains(key);

    public void AddEntry(string label, int position)
    {
        entries.Add(new(label, StartPoint(), position));
    }

    private int StartPoint() =>
        entries.Count == 0 ? 0 : entries[^1].NextPos;
}

internal static class ParseMapRegistry
{
    private static List<ParseMap> maps = new();
    public static ParseMap NewMap(object source)
    {
        var ret = new ParseMap();
        ret.AddAlias(source);
        maps.Add(ret);
        return ret;
    }

    public static void LogToMap(object key, string label, int streamPosition) => 
        FindMap(key)?.AddEntry(label, streamPosition);

    private static ParseMap? FindMap(object key) => 
        maps.FirstOrDefault(i => i.MonitoringKey(key));
}

