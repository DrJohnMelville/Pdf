using System.Diagnostics;
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

    [Conditional("DEBUG")]
    public void UnRegister()
    {
        
        ParseMapRegistry.Remove(this);
    }
}

internal static class ParseMapRegistry
{
    private static readonly Lock mutex = new();
    private static List<ParseMap> maps = new();
    public static ParseMap NewMap(object source)
    {
        var ret = new ParseMap();
        ret.AddAlias(source);
        lock (mutex)
        {
            maps.Add(ret);
        }
        return ret;
    }

    public static void LogToMap(object key, string label, int streamPosition)
    {
        lock (mutex)
        {
            FindMap(key)?.AddEntry(label, streamPosition);
        }
    }

    private static ParseMap? FindMap(object key)
    {
        lock (mutex)
        {
            return maps.FirstOrDefault(i => i.MonitoringKey(key));
        }
    }

    public static void Remove(ParseMap parseMap)
    {
        lock (mutex)
        {
            maps.Remove(parseMap);
        }
    }
}

