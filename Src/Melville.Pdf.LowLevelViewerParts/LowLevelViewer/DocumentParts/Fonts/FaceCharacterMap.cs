﻿using System.Windows.Data;
using Melville.MVVM.Wpf.Bindings;
using Melville.SharpFont;

namespace Melville.Pdf.LowLevelViewerParts.LowLevelViewer.DocumentParts.Fonts;

public record class FaceCharacterMap(uint CharIndex, uint GlyphIndex)
{
    public override string ToString() => $"0x{CharIndex:X} => {GlyphIndex}  ";
}

public static class FaceCharacterConverter
{
    public static readonly IValueConverter Instance = 
        LambdaConverter.Create<CharMap,IEnumerable<FaceCharacterMap>>(ExtractCharMap);

    private static IEnumerable<FaceCharacterMap> ExtractCharMap(CharMap map) => 
        map.AllMappings().Select(i => new FaceCharacterMap(i.Char, i.Glyph));

    public static IEnumerable<(uint Char, uint Glyph)> AllMappings(this CharMap cMap)
    {
        try
        {
            cMap.Face.SetCharmap(cMap);
        }
        catch (Exception )
        {
            yield break;
        }
        uint character = cMap.Face.GetFirstChar(out var glyph);
        do
        {
            yield return (character, glyph);
            character = cMap.Face.GetNextChar(character, out glyph);
        } while (character != 0);
    }
}