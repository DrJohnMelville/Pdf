using System.ComponentModel;
using System.Numerics;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Pdf.Model.Renderers.DocumentRenderers;
using Melville.Pdf.Model.Renderers.FontRenderings;
using Melville.Pdf.TextExtractor;

namespace Melville.Pdf.ComparingReader.Viewers.ExtractedTexts;

public partial class ExtractedTextViewModel
{
    [FromConstructor]private readonly DocumentRenderer renderer;
    [AutoNotify] private string text = "No Page Loaded";

    public async Task LoadPageAsync(int page)
    {
        Text = await renderer.PageTextAsync(page);
    }
}

