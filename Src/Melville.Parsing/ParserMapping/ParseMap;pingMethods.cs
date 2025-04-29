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
        ret.AddEntry("Parse mapping is only supported in debug builds");
        return ret;
    }
    public static ParseMap MonitorParsing(this Stream parseSource) => flyweight;
    public static ParseMap MonitorParsing(this IByteSource parseSource) => flyweight;
    
#endif

    [MacroItem("Stream")]
    [MacroItem("IByteSource")]
    [MacroCode("""
     [Conditional("DEBUG")]
         public static void LogParsePosition(this ~0~ stream, string label, int delta = 0) => 
             ParseMapRegistry.LogToMap(stream, label, (int)stream.Position + delta);
     [Conditional("DEBUG")]
         public static void IndentParseMap(this ~0~ alias, string title) =>
             ParseMapRegistry.Indent(alias, title);
     [Conditional("DEBUG")]
         public static void OutdentParseMap(this ~0~ alias) =>
             ParseMapRegistry.Outdent(alias);
     [Conditional("DEBUG")]
         public static void PeerIndentParseMap(this ~0~ alias, string title) =>
             ParseMapRegistry.PeerIndent(alias, title);
     [Conditional("DEBUG")]
     public static void AddParseMapAlias(this ~0~ s, object alias) =>
         ParseMapRegistry.AddAlias(s, alias);
               
     """)]
    [Conditional("DEBUG")]
    public static void AddParseMapAlias(this IMultiplexSource s, object alias) =>
        ParseMapRegistry.AddAlias(s, alias);

    public static IByteSource ReadLoggedPipeFrom(this IMultiplexSource src, int position)
    {
        var ret = src.ReadPipeFrom(position);
        src.AddParseMapAlias(ret);
        return ret;
    }
}