﻿using System.Buffers;
using System.IO.Pipelines;
using Melville.Fonts.SfntParsers.TableDeclarations.CMaps;
using Melville.Fonts.SfntParsers.TableParserParts;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.CountingReaders;
using Melville.Parsing.SequenceReaders;

namespace Melville.Fonts.SfntParsers.TableDeclarations.Maximums;

internal readonly struct MaxpParser(IByteSource source)
{
    public async ValueTask<ParsedMaximums> ParseAsync() =>
        TryReadShortForm(source, await source.ReadAtLeastAsync(6).CA()) 
        ?? await ReadLongFormAsync(source).CA();

    private ParsedMaximums? TryReadShortForm(IByteSource source, ReadResult readResult)
    {
        var reader = new SequenceReader<byte>(readResult.Buffer);
        if (reader.ReadBigEndianUint32() == 0x00005000)
        {
            var ret = new ParsedMaximums(reader.ReadBigEndianUint16());
            source.AdvanceTo(reader.Position);
            return ret;
        }
        source.AdvanceTo(readResult.Buffer.Start, reader.Position);
        return null;
    }

    private ValueTask<ParsedMaximums> ReadLongFormAsync(IByteSource source) => 
        FieldParser.ReadFromAsync<ParsedMaximums>(source);
}