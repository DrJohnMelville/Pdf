using System;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Pdf.Model.Renderers.DocumentRenderers;
using Melville.Pdf.TextExtractor;

namespace Melville.Pdf.ComparingReader.Viewers.ExtractedTexts;

public partial class ExtractedTextViewModel
{
    [FromConstructor]private readonly DocumentRenderer renderer;
    [AutoNotify] private string text = "No Page Loaded";

    public async Task LoadPageAsync(int page)
    {
        Text = await PageText(page);
    }

    private async Task<string> PageText(int page)
    {
        try
        {
            return await renderer.PageTextAsync(page);
        }
        catch (Exception e)
        {
            return $"{e.Message}\r\n\r\n{e.StackTrace}";
        }
    }
}

