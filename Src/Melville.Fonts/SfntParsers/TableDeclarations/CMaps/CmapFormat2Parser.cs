using System.Buffers.Binary;
using System.IO.Pipelines;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using Melville.Fonts.SfntParsers.TableParserParts;
using Melville.Parsing.AwaitConfiguration;

namespace Melville.Fonts.SfntParsers.TableDeclarations.CMaps;

internal readonly partial struct CmapFormat2Parser
{
    [SFntField] private readonly ushort format;
    [SFntField] private readonly ushort length;
    [SFntField] private readonly ushort language;
    [SFntField("256")] private readonly ushort[] subheadderKeys;
    [SFntField("SubHeaderLength()")] private readonly byte[] glyphArray;

    private int SubHeaderLength() => 
        length - (512 + this.GetStaticSize());

    public ICmapImplementation CreateImplementation()
    {
        var glyphsAsUint = MemoryMarshal.Cast<byte, ushort>(glyphArray);
        BinaryPrimitives.ReverseEndianness(glyphsAsUint, glyphsAsUint);
        return new Type2Cmap(subheadderKeys, glyphArray);
    }

    public static async Task<ICmapImplementation> ParseAsync(PipeReader input)
    {
        return (await FieldParser.ReadFromAsync<CmapFormat2Parser>(input).CA())
            .CreateImplementation();
    }
}

internal class Type2Cmap(ushort[] headers, byte[] glyphs) : ICmapImplementation
{
    public IEnumerable<(int Bytes, uint Character, uint Glyph)> AllMappings() =>
        CastSpanToHeaderRec(glyphs).AllMappings(glyphs).Select(
                i => (1, (uint)i.Character, (uint)i.Glyph))
            .Concat(
                headers
                    .Select((i,j)=>(Offset: i, Index: j))
                    .Where(i=>i.Offset > 0)
                    .SelectMany(i=>SubkeysForHeader(i.Offset,i.Index)));

    private IEnumerable<(int Bytes, uint Character, uint Glyph)> SubkeysForHeader(
        ushort offset, int index)
    {
        var firstByte = index << 8;
        var span = glyphs[offset..];
        return CastSpanToHeaderRec(span).AllMappings(span).Select(
            i => (2, (uint)(firstByte | i.Character), (uint)i.Glyph));
    }

    public uint Map(uint character)
    {
        TryMap(character, out var glyph);
        return glyph;
    }

    public bool TryMap(uint character, out uint glyph) =>
        TryMap(character > 0xFF ? 2 : 1, character, out glyph);

    public bool TryMap(int bytes, uint character, out uint glyph)
    {
        var headerOffset = headers[FirstByte(bytes, character)];
        if (IsValidCharacter(bytes, headerOffset))
            return ProcessHeader(headerOffset, LastByte(character), out glyph);

        glyph = 0;
        return false;
    }

    private static uint LastByte(uint character) => character & 0xFF;

    private static bool IsValidCharacter(int bytes, ushort header) => 
        ((header is 0 && bytes is 1) | bytes is 2);

    private static uint FirstByte(int bytes, uint character) => 
        (character >> (8 * (bytes - 1))) &  0xFF;

    private bool ProcessHeader(ushort offset, uint character, out uint glyph)
    {
        ReadOnlySpan<byte> headerSpan = glyphs[offset..];
        glyph = CastSpanToHeaderRec(headerSpan).LookupGlyphFor(character, headerSpan);
        return true;
    }

    private static Type2Subheader CastSpanToHeaderRec(ReadOnlySpan<byte> headerSpan) => 
        MemoryMarshal.Cast<byte, Type2Subheader>(headerSpan)[0];
}

[StructLayout(LayoutKind.Sequential)]
internal readonly struct Type2Subheader
{
    private readonly ushort First;
    private readonly ushort EntryCount;
    private readonly short IdDelta;
    private readonly ushort IdRangeOffset;

    public ushort LookupGlyphFor(uint character, ReadOnlySpan<byte> glyphs)
    {
        var characterAsRangePosition = character - First;
        if (characterAsRangePosition >= EntryCount) return 0;
        return AdjustByIdDelta(
            CastToCharacter(glyphs, CharacterArryaOffset(characterAsRangePosition)));
    }

    private static ushort CastToCharacter(ReadOnlySpan<byte> glyphs, int finalIndex) => 
        MemoryMarshal.Cast<byte, ushort>(glyphs[finalIndex..])[0];

    private const int positionOfIdRangeOffsetField = 6;
    private int CharacterArryaOffset(uint characterAsRangePosition) => (int)(
        positionOfIdRangeOffsetField + IdRangeOffset + (characterAsRangePosition*2));

    private ushort AdjustByIdDelta(ushort glyph) =>
        glyph == 0 ? glyph : (ushort) (glyph + IdDelta);

    public IEnumerable<(int Character, ushort Glyph)> AllMappings(ReadOnlySpan<byte> glyphs)
    {
        var ret = new (int,ushort)[EntryCount];
        for (ushort i = First; i < First+EntryCount; i++)
        {
            ret[i-First] = (i, LookupGlyphFor(i, glyphs));
        }

        return ret;
    }
}