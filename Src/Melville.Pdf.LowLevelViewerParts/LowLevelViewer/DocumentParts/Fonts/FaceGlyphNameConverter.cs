using System.Buffers;
using System.Windows.Data;
using Melville.MVVM.Wpf.Bindings;
using Melville.Pdf.Model.Renderers.FontRenderings.FreeType;
using Melville.SharpFont;

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

    public static IEnumerable<(uint Glyph, string Name)> AllGlyphNames(this Face face)
    {
        if (!face.HasGlyphNames) yield break;
        var buffer = ArrayPool<byte>.Shared.Rent(100);
        for (uint i = 0; i < face.GlyphCount; i++)
        {
            yield return (i, face.GetGlyphName(i, buffer));
        }
        ArrayPool<byte>.Shared.Return(buffer);
    }

}