using System.Buffers;
using System.Reflection.Metadata;
using Melville.Parsing.SequenceReaders;
using Melville.StreamInterfaces.ParsingBuffers;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Melville.Fonts.SfntParsers.TableParserParts;

internal static class F2Dot14Parser
{
    private const float Factor = (float)( 1.0 / 16384);
    public static float Convert(short value)
    {
        return value * Factor;
    }
#warning eliminate this SequenceReader

    public static float ReadF2Dot14(this ref SpanParsingBuffer reader) =>
        Convert(reader.GetInt16BigEndian());

}