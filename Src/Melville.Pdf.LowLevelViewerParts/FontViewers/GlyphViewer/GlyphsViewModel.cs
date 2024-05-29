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
    [AutoNotify] private RecordedGlyph glyph = new();
    [AutoNotify] private IList<GlyphPointViewModel>? renderedPoints;
    [AutoNotify] private bool unitSquare = true;
    [AutoNotify] private bool boundingBox = true;
    [AutoNotify] private bool points = true;
    [AutoNotify] private bool vectors = true;
    [AutoNotify] private bool outline = true;
    [AutoNotify] private bool fill = true;

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
        var target = new GlyphPointRecorder();
        if (GlyphSource is TrueTypeGlyphSource ttgs)
        {
            await ttgs.ParsePointsAsync((uint)PageSelector.Page,target, Matrix3x2.Identity);
            var newGlyph = new RecordedGlyph();
            await ttgs.ParsePointsAsync((uint)PageSelector.Page, newGlyph, Matrix3x2.Identity);
            Glyph = newGlyph;
        }

        RenderedPoints = target.Points;
    }
}

public record GlyphPointViewModel(double X, double Y, string Type);