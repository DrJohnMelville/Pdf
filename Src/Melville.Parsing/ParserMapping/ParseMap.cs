using System.ComponentModel.Design.Serialization;
using System.Diagnostics;
using Melville.INPC;
using Melville.Parsing.CountingReaders;

namespace Melville.Parsing.ParserMapping;

public abstract partial class ParseMapEntryBase
{
    [FromConstructor] public string Title { get; }
    public abstract int StartPos { get; }
    public abstract int NextPos { get; }
}

public partial class ParseMapEntry: ParseMapEntryBase
{
    [FromConstructor] public override int StartPos { get; }
    [FromConstructor] public override int NextPos { get; }
}

public partial class ParseMapTitle : ParseMapEntryBase
{
    [FromConstructor] public ParseMapTitle? Parent { get; }
    private readonly List<ParseMapEntryBase> items = new();
    public IReadOnlyList<ParseMapEntryBase> Items => items;
    public override int StartPos => items.FirstOrDefault()?.StartPos ?? 0;
    public override int NextPos => items.LastOrDefault()?.NextPos ?? 0;
    public void Add(ParseMapEntryBase item) => items.Add(item);
}

public class ParseMap
{
    private readonly HashSet<object> aliases = new();
    public ParseMapTitle Root { get; } = new ParseMapTitle("Pasing Map Root", null);
    private ParseMapTitle currentNode;

    public ParseMap()
    {
        currentNode = Root;
    }

    [Conditional("DEBUG")]
    public void AddAlias(object? source)
    {
        if (source == null) return;
        aliases.Add(source);
    }

    public bool MonitoringKey(object key) => aliases.Contains(key);

    private int priorEndPoint = 0;
    public void AddEntry(string label, int position)
    {
        currentNode.Add(new ParseMapEntry(label, priorEndPoint, position));
        priorEndPoint = position;
    }

    [Conditional("DEBUG")]
    public void UnRegister() => ParseMapRegistry.Remove(this);

    public static ParseMap CreateNew() => ParseMapRegistry.NewMap(null);

    public void Indent(string title)
    {
        var newNode = new ParseMapTitle(title, currentNode);
        currentNode.Add(newNode);
        currentNode = newNode;
    }

    public void Outdent() => currentNode = currentNode.Parent ?? currentNode;
}

internal static class ParseMapRegistry
{
    private static readonly Lock mutex = new();
    private static List<ParseMap> maps = new();
    public static ParseMap NewMap(object? source)
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

    private static ParseMap? FindMap(object key) => maps.FirstOrDefault(i => i.MonitoringKey(key));

    public static void Remove(ParseMap parseMap)
    {
        lock (mutex)
        {
            maps.Remove(parseMap);
        }
    }

    public static void AddAlias(object key, object alias)
    {
        lock (mutex)
        {
            FindMap(key)?.AddAlias(alias);
        }
    }

    public static void Indent(object alias, string title)
    {
        lock (mutex)
        {
            FindMap(alias)?.Indent(title);
        }
    }

    public static void Outdent(object alias)
    {
        lock (mutex)
        {
            FindMap(alias)?.Outdent();
        }
    }

    public static void PeerIndent(object alias, string title)
    {
        lock (mutex)
        {
            if (FindMap(alias) is not {} map) return;
            map.Outdent();
            map.Indent(title);
        }
    }
}

