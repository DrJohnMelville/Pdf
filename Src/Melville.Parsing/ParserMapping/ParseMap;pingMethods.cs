using System.Diagnostics;
using Melville.INPC;
using Melville.Parsing.CountingReaders;
using Melville.Parsing.MultiplexSources;

namespace Melville.Parsing.ParserMapping;

public static partial class ParseMappingMethods
{
#if DEBUG
    public static ParseMap MonitorParsing(this Stream source) => ParseMapRegistry.NewMap(source);
    public static ParseMap MonitorParsing(this IByteSource source) => ParseMapRegistry.NewMap(source);
#else
    private static ParseMap flyweightMap = CreateFlyweight();
    private static ParseMap CreateFlyweight ()
    {
        var ret = new ParseMap();
        ret.AddEntry("Parse mapping is only supported in debug builds", 0);
        return ret;
    }
    public static ParseMap MonitorParsing(this Stream parseSource) => flyweightMap;
    public static ParseMap MonitorParsing(this IByteSource parseSource) => flyweightMap;

#endif

    [MacroItem("Stream")]
    [MacroItem("IByteSource")]
    [MacroItem("ParseMapBookmark?")]
    [MacroCode("""
               [Conditional("DEBUG")]
                   public static void LogParsePosition(this ~0~ stream, string label, int delta = 0) => 
                      ParseMapRegistry.FindMap(stream)?.AddEntry(label, (int)stream.Position + delta);
               [Conditional("DEBUG")]
                   public static void IndentParseMap(this ~0~ alias, string title) =>
                       ParseMapRegistry.FindMap(alias)?.Indent(title);
               [Conditional("DEBUG")]
                   public static void OutdentParseMap(this ~0~ alias) =>
                       ParseMapRegistry.FindMap(alias)?.Outdent();
               [Conditional("DEBUG")]
                   public static void PeerIndentParseMap(this ~0~ alias, string title) =>
                       ParseMapRegistry.FindMap(alias)?.PeerIndent(title);
               [Conditional("DEBUG")]
               public static void AddParseMapAlias(this ~0~ s, object alias) =>
                       ParseMapRegistry.FindMap(s)?.AddAlias(alias);

               public static ParseMapBookmark? CreateParseMapBookmark(this ~0~ s, int offset = 0)
               {
                   #if DEBUG
                   if (ParseMapRegistry.FindMap(s) is not { } map) return null;
                   var ret = new ParseMapBookmark(s.Position + offset);
                   map.AddAlias(ret);
                   return ret;
                   #else
                       return null;
                   #endif
               }
               [Conditional("DEBUG")]
               public static void JumpToParseMap(this ~0~ s, int position) =>
                   ParseMapRegistry.FindMap(s)?.JumpTo((int)s.Position + position);
                          
               """)]
    [Conditional("DEBUG")]
    public static void AddParseMapAlias(this IMultiplexSource s, object alias) =>
        ParseMapRegistry.FindMap(s)?.AddAlias(alias);

    public static bool IsLoggingParseMap(this IMultiplexSource s) =>
        ParseMapRegistry.FindMap(s) is not null;

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

    public static IByteSource ReadLoggedPipeFrom(this IMultiplexSource src, int position, int internalPosition = 0)
    {
        var ret = src.ReadPipeFrom(position, internalPosition);
        src.AddParseMapAlias(ret);
        return ret;
    }
}