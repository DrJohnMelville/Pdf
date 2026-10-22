using System.Buffers;
using System.Runtime.InteropServices.ComTypes;
using Melville.Parsing;
using Melville.Parsing.CountingReaders;
using Melville.Parsing.MultiplexSources;
using Melville.Parsing.ParserMapping;
using Melville.Parsing.SequenceReaders;
using Melville.StreamInterfaces;
using Melville.StreamInterfaces.ParsingBuffers;

namespace Melville.Fonts.SfntParsers.TableDeclarations.CffGlyphs;

internal readonly struct CffEncodingReader(ParsingBuffer source, GlyphFromSid sidDecoder)
{
    private readonly byte[] result = new byte[256];
    public async ValueTask<byte[]> ParseAsync()
    {
        var type = await source.GetUInt8Async().CA();
        source.LogParsePosition($"Encoding type {type}");
        switch (type & 0x7F)
        {
            case 0:
                await ParseType0Async().CA();
                break;
            case 1:
                await ParseType1Async().CA();
                break;
            default:
                throw new InvalidDataException("Invalid encoding type");
        }

        if ((type & 0x80) == 0x80)
        {
            await ReadSupplementalMappingsAsync().CA();
        }

        return result;
    }

    private async ValueTask ParseType0Async()
    {
        var count = await source.GetUInt8Async().CA();
        source.LogParsePosition($"Encoding Count {count}");
        var buffer = await source.TakeBytesAsync(count).CA();
        ParseType0(buffer.Span, count);
    }

    private void ParseType0(Span<byte> bufferBuffer, byte count)
    {
        var reader = new SpanParsingBuffer(bufferBuffer);
        for (byte i = 1; i <= count; i++)
        {
            var encoding = reader.GetUInt8();
            source.LogParsePosition($"{encoding} => {i}", (int)reader.RelativePosition);
            result[encoding] = i;
        }
    }

    
    private async ValueTask ParseType1Async()
    {
        var count = await source.GetUInt8Async().CA();
        source.LogParsePosition($"Encoding count {count}");
        var length = count * 2;
        var buffer = await source.TakeBytesAsync(length).CA();
        ParseType1(buffer.Span, count);
    }

    private void ParseType1(Span<byte> buffer, byte count)
    {
        var src = new SpanParsingBuffer(buffer);
        byte glyph = 1;
        for (int i = 0; i < count; i++)
        {
            var first = src.GetUInt8();
            var innerCount = src.GetUInt8();
            source.IndentParseMap($"Code Range Begin {first} Length {innerCount}");
            source.LogParsePosition($"{first} => {glyph}", (int)src.RelativePosition);
            for (int j = 0; j < innerCount; j++)
            {
                source.LogParsePosition($"{first + j} => {glyph}", (int)src.RelativePosition);
                result[first + j] = glyph++;
            }
            source.OutdentParseMap();
        }
    }

    private async Task ReadSupplementalMappingsAsync()
    {
        var count = await source.GetUInt8Async().CA();
        source.LogParsePosition($"Supplemental Mappings Count {count}");
        var supplementLength = (int)count*3;
        var result= await source.TakeBytesAsync(supplementLength).CA();
        ReadSupplementalMappings(result.Span);
    }

    private void ReadSupplementalMappings(Span<byte> slice)
    {
        var reader = new SpanParsingBuffer(slice);
        while (reader.TryGetUInt8(out var code) &&
               reader.TryGetUInt16BigEndian(out var sid))
        {
            source.LogParsePosition($"Supplemental Mapping {code} => {sid}", (int)reader.RelativePosition);
// I think the SID Decoder may be a mistake here.  see note 4 on page 21 of the CFF spec
            result[code] = (byte)sidDecoder.Search(sid);
        }
    }
}