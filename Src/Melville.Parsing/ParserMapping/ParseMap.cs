using System.ComponentModel.Design.Serialization;
using System.Diagnostics;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.CountingReaders;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Melville.Parsing.ParserMapping;

internal interface IParseMap
{
    /// <summary>
    /// Add an alias to this parsemap.
    /// </summary>
    /// <param name="source">A new alias that can refer to this parsemap</param>
    public void AddAlias(object? source);

    /// <summary>
    /// True if this parsemap is monitoring the given alais.
    /// </summary>
    /// <param name="alias">The alias queried about.</param>
    /// <returns>Thue if alias should map to this parsemap, false otherwise.</returns>
    public bool MonitoringKey(object alias);

    /// <summary>
    /// Add an entry to this parsemap with the given label and next position.
    /// </summary>
    /// <param name="label">Text of the label</param>
    /// <param name="position">ofset of the first byte after the span</param>
    public void AddEntry(string label, int position);

    /// <summary>
    /// Create a new indented node
    /// </summary>
    /// <param name="title">Name of the new node</param>
    public void Indent(string title);

    /// <summary>
    /// End an indended node.
    /// </summary>
    public void Outdent();

    /// <summary>
    /// Set the prior position to a new location without
    /// creating a node
    /// </summary>
    /// <param name="position">The new prior position</param>
    public void JumpTo(int position);

    /// <summary>
    /// Outdent then indent in one operation
    /// </summary>
    /// <param name="title">The title of the new indented node.</param>
    public void PeerIndent(string title)
    {
        Outdent();
        Indent(title);
    }
}

/// <summary>
/// An alias to a parse map with a fixed position.
/// </summary>
/// <param name="Position">The position of the bookmark
/// relative to the start of the input.</param>
public record ParseMapBookmark(long Position);

/// <summary>
/// Represents a trace of a parser through some data.
/// </summary>
public class ParseMap: IParseMap
{
    private readonly HashSet<object> aliases = new();

    /// <summary>
    /// The root of the parsing map.
    /// </summary>
    public ParseMapTitle Root { get; } = new ParseMapTitle("Parsing Map Root", null);

    /// <summary>
    /// Reference copy of the data being parsed.
    /// </summary>
    public byte[] Data { get; internal set; } = [];
  
    private ParseMapTitle currentNode;

    /// <summary>
    /// Create a new parse map.
    /// </summary>
    public ParseMap() => currentNode = Root;

    /// <inheritdoc />
    public void AddAlias(object? source)
    {
        if (source == null) return;
        aliases.Add(source);
    }

    /// <inheritdoc />
    public bool MonitoringKey(object alias) => aliases.Contains(alias);

    private int priorEndPoint = 0;

    /// <inheritdoc />
    public void AddEntry(string label, int position)
    {
        currentNode.Add(new ParseMapEntry(label, priorEndPoint, position));
        JumpTo(position);
    }

    /// <inheritdoc />
    public void JumpTo(int position) => priorEndPoint = position;

    /// <summary>
    /// Stop receiving parse map indications by removing myself from the registry.
    /// </summary>
    public void UnRegister() => ParseMapRegistry.Remove(this);

    /// <summary>
    /// Create a new parsemap and add it to the registry
    /// </summary>

    public static ParseMap CreateNew() => ParseMapRegistry.NewMap(null);

    /// <inheritdoc />
    public void Indent(string title)
    {
        var newNode = new ParseMapTitle(title, currentNode);
        currentNode.Add(newNode);
        currentNode = newNode;
    }

    /// <inheritdoc />
    public void Outdent() => currentNode = currentNode.Parent ?? currentNode;

    internal void SetDataInternal(byte[] newData) => Data = newData;
}

/// <summary>
/// Operations to add data to a parsemap in debug mode only
/// </summary>
public static class ParseMapSetDataOperations
{
    /// <summary>
    /// Set the data for a parsemap.
    /// </summary>
    /// <param name="map">The map to set the data for</param>
    /// <param name="source">the data to set.</param>
    [Conditional("DEBUG")]
    public static void SetData(this ParseMap? map, byte[] source)
    {
        if (map == null) return;
        map.SetDataInternal(source);
    }

    /// <summary>
    /// Set a stream as the parsemap data.
    /// </summary>
    /// <param name="map">The map to set</param>
    /// <param name="s">the stream to use as the parsemap source</param>
#if DEBUG
    public static async ValueTask SetDataAsync(this ParseMap? map, Stream s)
    {
        if (map == null) return;
        var memory = new MemoryStream();
        await s.CopyToAsync(memory).CA();
        map.SetDataInternal(memory.ToArray());
        await memory.DisposeAsync().CA();
        await s.DisposeAsync().CA();
    }
#else
    public static ValueTask SetDataAsync(this ParseMap? map, Stream s) =>
         s.DisposeAsync();
#endif
}