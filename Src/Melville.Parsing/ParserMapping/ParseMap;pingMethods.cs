using System.Diagnostics;
using Melville.INPC;
using Melville.Parsing.CountingReaders;
using Melville.Parsing.MultiplexSources;

namespace Melville.Parsing.ParserMapping;

/// <summary>
/// This class is extension methods used to declare parsemaps.  Most of these calls fall
/// away in release builds.
/// </summary>
public static partial class ParseMappingMethods
{
#if DEBUG
    /// <summary>
    /// Creates a new parse map for the given source.
    /// </summary>
    /// <param name="source">The item to monitor for parsing</param>
    public static ParseMap MonitorParsing(this Stream source) => ParseMapRegistry.NewMap(source);
    /// <summary>
    /// Creates a new parse map for the given source.
    /// </summary>
    /// <param name="source">The item to monitor for parsing</param>
    public static ParseMap MonitorParsing(this IByteSource source) => ParseMapRegistry.NewMap(source);
#else
    private static ParseMap flyweightMap = CreateFlyweight();
    private static ParseMap CreateFlyweight ()
    {
        var ret = new ParseMap();
        ret.AddEntry("Parse mapping is only supported in debug builds", 0);
        return ret;
    }

    /// <summary>
    /// Creates a new parse map for the given source.
    /// </summary>
    /// <param name="source">The item to monitor for parsing</param>
    public static ParseMap MonitorParsing(this Stream parseSource) => flyweightMap;
    
    /// <summary>
    /// Creates a new parse map for the given source.
    /// </summary>
    /// <param name="source">The item to monitor for parsing</param>
    public static ParseMap MonitorParsing(this IByteSource parseSource) => flyweightMap;

#endif

    /// <summary>
    /// Add another alias to the parsemap.
    /// </summary>
    /// <param name="s">An existing alias to the parsemap.</param>
    /// <param name="alias">The new alias to be added</param>
    [MacroItem("Stream")]
    [MacroItem("IByteSource")]
    [MacroItem("ParseMapBookmark?")]
    [MacroCode("""
               /// <summary>
               ///  Adds a bookmark to the current parse map.  The interval from the last
               /// position to the current position is given the indicated label.
               /// </summary>
               /// <param name="stream">The alias to the parsemap</param>
               /// <param name="label">The label to assign to the bytes</param>
               /// <param name="delta">Offset from the label's position</param>
               [Conditional("DEBUG")]
                   public static void LogParsePosition(this ~0~ stream, string label, int delta = 0) => 
                      ParseMapRegistry.FindMap(stream)?.AddEntry(label, (int)(stream?.Position??0) + delta);

               /// <summary>
               /// Start a new indented section of the parsemap
               /// </summary>
               /// <param name="alias">The parsemap alias</param>
               /// <param name="title">Title for the section</param>
               [Conditional("DEBUG")]
                   public static void IndentParseMap(this ~0~ alias, string title) =>
                       ParseMapRegistry.FindMap(alias)?.Indent(title);
                       
               /// <summary>
               /// End and indented section of the parsemap
               /// </summary>
               /// <param name="alias">Alias to the parsemap</param>
               [Conditional("DEBUG")]
                   public static void OutdentParseMap(this ~0~ alias) =>
                       ParseMapRegistry.FindMap(alias)?.Outdent();

               /// <summary>
               /// Convenience operator to do an outdent and then a new indent in one operation
               /// </summary>
               /// <param name="alias">Alias for the parsemap</param>
               /// <param name="title">Title of the new parse map node.</param>
               [Conditional("DEBUG")]
                   public static void PeerIndentParseMap(this ~0~ alias, string title) =>
                       ParseMapRegistry.FindMap(alias)?.PeerIndent(title);

               /// <summary>
               /// Add another alias to the parsemap.
               /// </summary>
               /// <param name="s">An existing alias to the parsemap.</param>
               /// <param name="alias">The new alias to be added</param>
               [Conditional("DEBUG")]
               public static void AddParseMapAlias(this ~0~ s, object alias) =>
                       ParseMapRegistry.FindMap(s)?.AddAlias(alias);

               /// <summary>
               /// Create a bookmark for a given position in the parsemap.
               /// </summary>
               /// <param name="s">Alias for the parsemap</param>
               /// <param name="offset">Offset from the current position.</param>
               public static ParseMapBookmark? CreateParseMapBookmark(this ~0~ s, int offset = 0)
               {
                   #if DEBUG
                   if (ParseMapRegistry.FindMap(s) is not { } map) return null;
                   var ret = new ParseMapBookmark((s?.Position ?? 0) + offset);
                   map.AddAlias(ret);
                   return ret;
                   #else
                       return null;
                   #endif
               }

               /// <summary>
               /// Set the current position of a parsemap without creating any nodes.
               /// </summary>
               /// <param name="s">Alias for a parsemap.</param>
               /// <param name="position">Position relative to the alias</param>
               [Conditional("DEBUG")]
               public static void JumpToParseMap(this ~0~ s, int position) =>
                   ParseMapRegistry.FindMap(s)?.JumpTo((int)(s?.Position ?? 0) + position);
                          
               """)]
    [Conditional("DEBUG")]
    public static void AddParseMapAlias(this IMultiplexSource s, object alias) =>
        ParseMapRegistry.FindMap(s)?.AddAlias(alias);

    /// <summary>
    /// Returns true if the given source is being monitored by a parse map.
    /// </summary>
    /// <param name="s">Alias for the parse map</param>
    public static bool IsLoggingParseMap(this IMultiplexSource s) =>
        ParseMapRegistry.FindMap(s) is not null;

    /// <summary>
    /// Create a bookmark for a given position in the parsemap.
    /// </summary>
    /// <param name="s">Alias for the parsemap</param>
    /// <param name="offset">Offset from the current position.</param>
    public static ParseMapBookmark? CreateParseMapBookmark(this IMultiplexSource s, int offset = 0)
    {
#if DEBUG
        if (ParseMapRegistry.FindMap(s) is not
            {
            }
            map) return null;
        var ret = new ParseMapBookmark(offset);
        map.AddAlias(ret);
        return ret;
#else
             return null;
#endif
    }

    /// <summary>
    /// Create a pipe from a multiplex source and add it to the parsemap
    /// </summary>
    /// <param name="src">The source to get a pipe from</param>
    /// <param name="position">Desired starting position of the pipe</param>
    /// <param name="internalPosition">Position the pipe should report as the position of the first byte.</param>
    /// <returns></returns>
    public static IByteSource ReadLoggedPipeFrom(this IMultiplexSource src, int position, int internalPosition = 0)
    {
        var ret = src.ReadPipeFrom(position, internalPosition);
        src.AddParseMapAlias(ret);
        return ret;
    }
}