using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using Melville.Fonts;
using Melville.Fonts.SfntParsers.TableDeclarations.TrueTypeGlyphs;
using Melville.INPC;
using Melville.Pdf.Wpf.Controls;

namespace Melville.Pdf.LowLevelViewerParts.FontViewers.GlyphViewer;

public partial class GlyphsViewModel
{
    [FromConstructor] public IGlyphSource GlyphSource { get; }
    [AutoNotify] private GlyphRecorder glyph = GlyphRecorderFactory.GetRecorder();
    [AutoNotify] private bool unitSquare = true;
    [AutoNotify] private bool boundingBox = true;
    [AutoNotify] private bool points = true;
    [AutoNotify] private bool controlPoints = true;
    [AutoNotify] private bool phantomPoints = true;
    [AutoNotify] private bool outline = true;
    [AutoNotify] private bool fill = false;

    partial void OnConstructed()
    {
        PageSelector.MinPage = 0;
        PageSelector.MaxPage = GlyphSource.GlyphCount;
        PageSelector.WhenMemberChanges(nameof(PageSelector.Page), LoadNewGlyph);
        LoadNewGlyph();
    }

    public PageSelectorViewModel PageSelector { get; } = new();

    private async void LoadNewGlyph()
    {
        if (GlyphSource is TrueTypeGlyphSource ttgs)
        {
            var newGlyph = GlyphRecorderFactory.GetRecorder();
            await ttgs.RenderGlyphInEmUnitsAsync((uint)PageSelector.Page, newGlyph, Matrix3x2.Identity);
            GlyphRecorderFactory.ReturnRecorder(Glyph);
            Glyph = newGlyph;
        }
    }
}

public record GlyphPointViewModel(double X, double Y, string Type);