using System.Net.Sockets;
using System.Numerics;
using System.Text;
using Melville.Fonts.SfntParsers.TableDeclarations.CffGlyphs;
using Melville.INPC;

namespace Melville.Pdf.LowLevelViewerParts.FontViewers.CFFGlyphViewers;

public partial class CffGlyphViewModel: CharStringViewModel
{
    [FromConstructor] private readonly CffGlyphSource GlpyhSource;
    
    partial void OnConstructed()
    {
        PageSelector.MaxPage = GlpyhSource.GlyphCount-1;
        LoadNewGlyph();
    }

    protected override ValueTask RenderGlyph(ICffGlyphTarget renderTemp) => 
        GlpyhSource?.RenderCffGlyphAsync((uint)PageSelector.Page, renderTemp, 
            Matrix3x2.Identity) ?? ValueTask.CompletedTask;
}

