using System.Numerics;
using System.Windows;
using Melville.Fonts.SfntParsers.TableDeclarations.CffGlyphs;
using Melville.Fonts.Type1TextParsers;
using Melville.MVVM.Wpf.ViewFrames;
using Melville.Pdf.LowLevelViewerParts.FontViewers.CFFGlyphViewers;

namespace Melville.Pdf.LowLevelViewerParts.FontViewers.Type1Views;

public class Type1CharstringViewModel :CharStringViewModel, ICreateView
{
    private readonly Type1GenericFont font;

    public Type1CharstringViewModel(Type1GenericFont font)
    {
        this.font = font;
        PageSelector.MaxPage = font.GlyphCount-1;
        LoadNewGlyph();
    }

    protected override ValueTask RenderGlyphAsync(ICffGlyphTarget renderTemp)
    {
        if (font == null) return ValueTask.CompletedTask;
        return font.RenderToCffGlyphTargetAsync(
            (uint)PageSelector.Page, renderTemp, Matrix3x2.Identity);
    }

    public UIElement View() => 
        new CffGlyphView() { DataContext = this };
}