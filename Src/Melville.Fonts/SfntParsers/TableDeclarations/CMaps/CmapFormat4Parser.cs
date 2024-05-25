using System.IO.Pipelines;
using Melville.Fonts.SfntParsers.TableParserParts;
using Melville.Parsing.AwaitConfiguration;

namespace Melville.Fonts.SfntParsers.TableDeclarations.CMaps;

internal readonly partial struct CmapFormat4Parser
{
    [SFntField] private readonly ushort format;
    [SFntField] private readonly ushort length;
    [SFntField] private readonly ushort language;
    [SFntField] private readonly ushort segCountX2;
    [SFntField] private readonly ushort searchRange;
    [SFntField] private readonly ushort entrySelector;
    [SFntField] private readonly ushort rangeShift;
    [SFntField("SegCount + 1")] private readonly ushort[] endCode;
    // The spec has a 2 byte padding field in here.  I just read it onto the end of the
    // endCode array, where it does not cause any problems.
    [SFntField("SegCount")] private readonly ushort[] startCode;
    [SFntField("SegCount")] private readonly short[] idDelta;
    [SFntField("SegCount")] private readonly ushort[] idRangeOffset;
    [SFntField("GlyphIdArrayLength")] private readonly ushort[] glyphIdArray;

    private int SegCount => segCountX2 / 2;
    private int GlyphIdArrayLength => (length - SizePriorToGlyphArray())/2;

    private int SizePriorToGlyphArray() => (this.GetStaticSize() + 2 + (8*SegCount));

    public static async ValueTask<ICmapImplementation> ParseAsync(PipeReader input) => 
            (await FieldParser.ReadFromAsync<CmapFormat4Parser>(input).CA())
            .CreateImplementation();

    private ICmapImplementation CreateImplementation()
    {
        return new CmapFormat4Implementation(
            endCode, startCode, idDelta, idRangeOffset, glyphIdArray);
    }
}

internal class CmapFormat4Implementation (
    ushort[] endCode, 
    ushort[] startCode, 
    short[] idDelta, 
    ushort[] idRangeOffset, 
    ushort[] glyphIdArray) : ICmapImplementation
{
    public IEnumerable<(int Bytes, uint Character, uint Glyph)> AllMappings()
    {
        return startCode.Zip<ushort, ushort, (ushort, ushort)>(endCode, 
                (s, e) => (s, e))
            .SelectMany(MapSingleRange);
    }

    private IEnumerable<(int Bytes, uint Character, uint Glyph)> MapSingleRange(
        (ushort, ushort) pair)
    {
        for (uint i = pair.Item1; i <= pair.Item2; i++)
        {
            if (TryMap(2, i, out var glyph))
            {
                yield return (2, i, glyph);
            }
        }
    }

    public bool TryMap(int bytes, uint character, out uint glyph)
    {
        if (bytes < 2) {glyph = 0; return false; }

        var index = TryFlipBits(endCode.AsSpan()[..^1].BinarySearch((ushort)character));
        glyph = MapCharacter(index, character);
        return true;
    }

    private int TryFlipBits(int value)
    {
        return value <0 ? ~value : value;
    }

    private uint MapCharacter(int index, uint character)
    {
        if (index > startCode.Length) return 0;
        if (startCode[index] > character) return 0;
        var glyphDelta = idRangeOffset[index] /2;
        if (glyphDelta == 0) return MapByDelta(index, character);
        return MapUsingGlyphArray(index, character, glyphDelta);
    }

    private uint MapUsingGlyphArray(int index, uint character, int rangeOffset) =>
        MapNonZerosByDelta(index, 
            glyphIdArray[GlyphArrayIndex(index, character, rangeOffset)]);


    private long GlyphArrayIndex(int index, uint character, int gylphDelta)
    {
        var rangeOffsetsAfterMe = (idRangeOffset.Length - (index));
        return (gylphDelta - rangeOffsetsAfterMe) + (character - startCode[index]);
    }

    private uint MapNonZerosByDelta(int index, ushort character) => 
        character == 0 ? 0 : MapByDelta(index, character);

    private uint MapByDelta(int index, uint character) => 
        (uint)((character + idDelta[index]) % 65536);
}