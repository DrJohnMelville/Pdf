using System.Buffers.Binary;
using System.IO.Pipelines;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using Melville.Fonts.SfntParsers.TableParserParts;

namespace Melville.Fonts.SfntParsers.TableDeclarations.CMaps;


internal readonly struct CmapFormat2Parser
{
    public static async Task<ICmapImplementation> ParseAsync(PipeReader input)
    {
        var record = await FieldParser.ReadFromAsync<CmapFormat2RootTable>(input);
        return record.CreateImplementation();
    }
}

internal readonly partial struct CmapFormat2RootTable
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
    public readonly ushort First;
    public readonly ushort EntryCount;
    public readonly short IdDelta;
    public readonly ushort IdRangeOffset;

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

// this comment was saved from
// // it explains how to implement a TTF type 2 cmap

/*
         # How this gets processed.
   # Charcodes may be one or two bytes.
   # The first byte of a charcode is mapped through the subHeaderKeys, to select
   # a subHeader. For any subheader but 0, the next byte is then mapped through the
   # selected subheader. If subheader Index 0 is selected, then the byte itself is
   # mapped through the subheader, and there is no second byte.
   # Then assume that the subsequent byte is the first byte of the next charcode,and repeat.
   #
   # Each subheader references a range in the glyphIndexArray whose length is entryCount.
   # The range in glyphIndexArray referenced by a sunheader may overlap with the range in glyphIndexArray
   # referenced by another subheader.
   # The only subheader that will be referenced by more than one first-byte value is the subheader
   # that maps the entire range of glyphID values to glyphIndex 0, e.g notdef:
   # 	 {firstChar 0, EntryCount 0,idDelta 0,idRangeOffset xx}
   # A byte being mapped though a subheader is treated as in index into a mapping of array index to font glyphIndex.
   # A subheader specifies a subrange within (0...256) by the
   # firstChar and EntryCount values. If the byte value is outside the subrange, then the glyphIndex is zero
   # (e.g. glyph not in font).
   # If the byte index is in the subrange, then an offset index is calculated as (byteIndex - firstChar).
   # The index to glyphIndex mapping is a subrange of the glyphIndexArray. You find the start of the subrange by
   # counting idRangeOffset bytes from the idRangeOffset word. The first value in this subrange is the
   # glyphIndex for the index firstChar. The offset index should then be used in this array to get the glyphIndex.
   # Example for Logocut-Medium
   # first byte of charcode = 129; selects subheader 1.
   # subheader 1 = {firstChar 64, EntryCount 108,idDelta 42,idRangeOffset 0252}
   # second byte of charCode = 66
   # the index offset = 66-64 = 2.
   # The subrange of the glyphIndexArray starting at 0x0252 bytes from the idRangeOffset word is:
   # [glyphIndexArray index], [subrange array index] = glyphIndex
   # [256], [0]=1 	from charcode [129, 64]
   # [257], [1]=2  	from charcode [129, 65]
   # [258], [2]=3  	from charcode [129, 66]
   # [259], [3]=4  	from charcode [129, 67]
   # So, the glyphIndex = 3 from the array. Then if idDelta is not zero and the glyph ID is not zero,
   # add it to the glyphID to get the final glyphIndex
   # value. In this case the final glyph index = 3+ 42 -> 45 for the final glyphIndex. Whew!
 */
