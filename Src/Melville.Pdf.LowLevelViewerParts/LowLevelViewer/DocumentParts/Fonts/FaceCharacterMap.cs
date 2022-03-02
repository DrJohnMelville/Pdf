using System.Windows.Data;
using Melville.MVVM.Wpf.Bindings;
using SharpFont;

namespace Melville.Pdf.LowLevelViewerParts.LowLevelViewer.DocumentParts.Fonts;

public record class FaceCharacterMap(uint CharIndex, uint GlyphIndex)
{
    public override string ToString() => $"0x{CharIndex:X} => {GlyphIndex}  ";
}

public static class FaceCharacterConverter
{
    public static readonly IValueConverter Instance = 
        LambdaConverter.Create<CharMap,IEnumerable<FaceCharacterMap>>(ExtractCharMap);

    private static IEnumerable<FaceCharacterMap> ExtractCharMap(CharMap map)
    {
        map.Face.SelectCharmap(map.Encoding);
        var charIndex = map.Face.GetFirstChar(out var glyphIndex);
        while (charIndex != 0)
        {
            yield return new FaceCharacterMap(charIndex, glyphIndex);
            charIndex = map.Face.GetNextChar(charIndex, out glyphIndex);
        }
    }
}
