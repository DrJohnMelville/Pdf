using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Pdf.ComparingReader.Renderers;
using Melville.Pdf.LowLevel.Parsing.ParserContext;
using Melville.Pdf.Model;
using Melville.Pdf.Model.Renderers.DocumentRenderers;
using Melville.Pdf.TextExtractor;

namespace Melville.Pdf.ComparingReader.Viewers.ExtractedTexts;

public partial class ExtractedTextRenderer: IRenderer
{
    public string DisplayName => "Extracted Text";
    [AutoNotify] private object renderTarget = "No Document Loaded";
    public async void SetTarget(Stream pdfBits, IPasswordSource passwordSource)
    {
        var model = new ExtractedTextViewModel(
            await new PdfReader(passwordSource).ReadFromAsync(pdfBits));
        model.LoadPage(1);
        RenderTarget = model;
    }

    public void SetPage(int page) => (RenderTarget as ExtractedTextViewModel)?.LoadPage(page);
}

public partial class ExtractedTextViewModel
{
    [FromConstructor]private readonly DocumentRenderer renderer;
    [AutoNotify] private string text = "No Page Loaded";


    public async Task LoadPage(int page)
    {
        Text = await renderer.PageTextAsync(page);
    }
}