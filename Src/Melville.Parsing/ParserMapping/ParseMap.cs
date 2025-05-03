using System.ComponentModel.Design.Serialization;
using System.Diagnostics;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.CountingReaders;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Melville.Parsing.ParserMapping;

public interface IParseMap
{
    public void AddAlias(object? source);
    public bool MonitoringKey(object key);
    public void AddEntry(string label, int position);
    public void Indent(string title);
    public void Outdent();
    public void SetData(byte[] newData);
    public void JumpTo(int position);

    public void PeerIndent(string title)
    {
        Outdent();
        Indent(title);
    }
}

public record ParseMapBookmark(long Position);

public class ParseMap: IParseMap
{
    private readonly HashSet<object> aliases = new();
    public ParseMapTitle Root { get; } = new ParseMapTitle("Pasing Map Root", null);
    public byte[] Data { get; internal set; } = [];
  
    private ParseMapTitle currentNode;

    public ParseMap() => currentNode = Root;

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
        JumpTo(position);
    }

    public void JumpTo(int position) => priorEndPoint = position;

    public void UnRegister() => ParseMapRegistry.Remove(this);

    public static ParseMap CreateNew() => ParseMapRegistry.NewMap(null);

    public void Indent(string title)
    {
        var newNode = new ParseMapTitle(title, currentNode);
        currentNode.Add(newNode);
        currentNode = newNode;
    }

    public void Outdent() => currentNode = currentNode.Parent ?? currentNode;

    public void SetData(byte[] newData) => Data = newData;
}

public static class ParseMapSetDataOperations
{
    [Conditional("DEBUG")]
    public static void SetData(this ParseMap? map, byte[] source)
    {
        if (map == null) return;
        map.SetData(source);
    }

#if DEBUG
    public static async ValueTask SetDataAsync(this ParseMap? map, Stream s)
    {
        if (map == null) return;
        var memory = new MemoryStream();
        await s.CopyToAsync(memory).CA();
        map.SetData(memory.ToArray());
        await memory.DisposeAsync().CA();
        await s.DisposeAsync().CA();
    }
#else
    public static ValueTask SetDataAsync(this ParseMap? map, Stream s) =>
         s.DisposeAsync();
#endif
}