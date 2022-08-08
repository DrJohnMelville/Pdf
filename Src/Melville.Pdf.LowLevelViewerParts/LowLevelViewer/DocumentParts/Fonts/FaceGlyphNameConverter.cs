using System.Windows.Data;
using Melville.MVVM.Wpf.Bindings;
using Melville.Pdf.Model.Renderers.FontRenderings.FreeType;
using SharpFont;

namespace Melville.Pdf.LowLevelViewerParts.LowLevelViewer.DocumentParts.Fonts;

public record GlyphNameRecord(uint Glyph, string Name)
{
    public override string ToString() => $"0x{Glyph:X} => {Name}  ";
}

public static class FaceGlyphNameConverter
{
    public static readonly IValueConverter Instance = 
        LambdaConverter.Create<Face,IEnumerable<GlyphNameRecord>>(ExtractGlyphNames);

    private static IEnumerable<GlyphNameRecord> ExtractGlyphNames(Face face) => 
        face.AllGlyphNames().Select(i => new GlyphNameRecord(i.Glyph, i.Name));
}