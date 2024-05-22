namespace Melville.Fonts.SfntParsers.TableDeclarations.CMaps;

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

internal readonly struct CmapFormat2Parser
{
}