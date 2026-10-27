using Melville.Fonts.SfntParsers.TableParserParts;
using Melville.StreamInterfaces.ParsingBuffers;

namespace Melville.Fonts.SfntParsers.TableDeclarations.Maximums;

internal readonly struct MaxpParser(ParsingBuffer source)
{
    public async ValueTask<ParsedMaximums> ParseAsync()
    {
        await source.EnsureBytesAsync(6);
        var spanReader = new SpanParsingBuffer(source.Peek(6));
        if (spanReader.GetUInt32BigEndian() is 0x00005000)
        {
            var ret = new ParsedMaximums(spanReader.GetUInt16BigEndian());
            source.Advance((int)spanReader.RelativePosition);
            return ret;
        }
        return await FieldParser.ReadFromAsync<ParsedMaximums>(source);
    }
}