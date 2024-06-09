using System.Numerics;
using Melville.Fonts.SfntParsers.TableDeclarations.TrueTypeGlyphs;
using Melville.INPC;
using Melville.Pdf.Wpf.Controls;

namespace Melville.Pdf.LowLevelViewerParts.FontViewers.GlyphViewer;

public partial class GlyphsViewModel
{
    [FromConstructor] public TrueTypeGlyphSource GlyphSource { get; }
    [AutoNotify] private GlyphRecorder glyph = GlyphRecorderFactory.GetRecorder();
    [AutoNotify] private bool unitSquare = true;
    [AutoNotify] private bool boundingBox = true;
    [AutoNotify] private bool points = true;
    [AutoNotify] private bool controlPoints = true;
    [AutoNotify] private bool phantomPoints = true;
    [AutoNotify] private bool outline = true;
    [AutoNotify] private bool fill = false;
    public PageSelectorViewModel PageSelector { get; } = new();

    partial void OnConstructed()
    {
        PageSelector.MinPage = 0;
        PageSelector.MaxPage = GlyphSource.GlyphCount;
        PageSelector.WhenMemberChanges(nameof(PageSelector.Page), LoadNewGlyph);
        LoadNewGlyph();
    }


    private async void LoadNewGlyph()
    {           //something is wrong with the glyphrecorder factory 
                // makes the lengths inconsistent
            var newGlyph = GlyphRecorderFactory.GetRecorder();
            await GlyphSource.RenderGlyphInEmUnitsAsync(
                (uint)PageSelector.Page, newGlyph, Matrix3x2.Identity);
           var oldGlyph = Glyph;
            Glyph = newGlyph;
//            GlyphRecorderFactory.ReturnRecorder(oldGlyph);
    }
}