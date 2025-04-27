using System.Diagnostics;
using Melville.Parsing.CountingReaders;

namespace Melville.Parsing.ParserMapping;

public static class ParseMappingMethods
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

    [Conditional("DEBUG")]
    public static void LogParsePosition(this Stream stream, string label, int delta = 0) => 
        ParseMapRegistry.LogToMap(stream, label, (int)stream.Position + delta);
    [Conditional("DEBUG")]
    public static void LogParsePosition(this IByteSource stream, string label, int delta = 0) => 
        ParseMapRegistry.LogToMap(stream, label, (int)stream.Position + delta);
}