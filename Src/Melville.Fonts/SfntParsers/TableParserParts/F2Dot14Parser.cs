using Melville.StreamInterfaces.ParsingBuffers;

namespace Melville.Fonts.SfntParsers.TableParserParts;

internal static class F2Dot14Parser
{
    private const float Factor = (float)( 1.0 / 16384);
    public static float Convert(short value)
    {
        return value * Factor;
    }
    public static float ReadF2Dot14(this ref SpanParsingBuffer reader) =>
        Convert(reader.GetInt16BigEndian());

}