using Melville.Fonts.SfntParsers.TableDeclarations.CffGlyphs;
using Melville.INPC;
using Melville.Pdf.Wpf.Controls;

namespace Melville.Pdf.LowLevelViewerParts.FontViewers.CFFGlyphViewers;

public abstract partial class CharStringViewModel
{
    [AutoNotify] private CffGlyphBuffer? renderedGlyph;
    [AutoNotify] private bool unitSquare = true;
    [AutoNotify] private bool boundingBox = true;
    [AutoNotify] private bool points = true;
    [AutoNotify] private bool controlPoints = true;
    [AutoNotify] private bool phantomPoints = true;
    [AutoNotify] private bool outline = true;
    [AutoNotify] private bool fill = false;
    public PageSelectorViewModel PageSelector { get; } = new();

    public CharStringViewModel()
    {
        PageSelector.MinPage = 0;
        PageSelector.MaxPage = 0;
        PageSelector.WhenMemberChanges(nameof(PageSelector.Page), LoadNewGlyph);
        LoadNewGlyph();
    }
    protected async void LoadNewGlyph()
    {
        var renderTemp = new CffGlyphBuffer();
        await RenderGlyphAsync(renderTemp);
        RenderedGlyph = renderTemp;
    }

    protected abstract ValueTask RenderGlyphAsync(ICffGlyphTarget renderTemp);

}