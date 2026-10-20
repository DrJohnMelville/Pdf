using System.Buffers;
using System.IO.Pipelines;
using Melville.Fonts.SfntParsers.TableDeclarations.CMaps;
using Melville.Fonts.SfntParsers.TableParserParts;

using Melville.Parsing.CountingReaders;
using Melville.Parsing.SequenceReaders;
using Melville.StreamInterfaces;
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

/*internal readonly struct MaxpParser(IByteSource source)
{
    public async ValueTask<ParsedMaximums> ParseAsync() =>
        TryReadShortForm(source, await source.ReadAtLeastAsync(6).CA()) 
        ?? await ReadLongFormAsync(source).CA();

    private ParsedMaximums? TryReadShortForm(IByteSource source, ReadResult readResult)
    {
        var reader = new SequenceReader<byte>(readResult.Buffer);
        if (reader.GetUInt32BigEndian() == 0x00005000)
        {
            var ret = new ParsedMaximums(reader.GetUInt16BigEndian());
            source.AdvanceTo(reader.Position);
            return ret;
        }
        source.AdvanceTo(readResult.Buffer.Start, reader.Position);
        return null;
    }

    private ValueTask<ParsedMaximums> ReadLongFormAsync(IByteSource source) => 
        FieldParser.ReadFromAsync<ParsedMaximums>(source);
}*/