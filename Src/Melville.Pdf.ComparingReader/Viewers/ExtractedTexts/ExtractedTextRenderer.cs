using System.IO;
using Melville.INPC;
using Melville.Pdf.ComparingReader.Renderers;
using Melville.Pdf.LowLevel.Parsing.ParserContext;
using Melville.Pdf.Model;

namespace Melville.Pdf.ComparingReader.Viewers.ExtractedTexts
{
    public partial class ExtractedTextRenderer: IRenderer
    {
        public string DisplayName => "Extracted Text";
        [AutoNotify] private object renderTarget = "No Document Loaded";
        public async void SetTarget(Stream pdfBits, IPasswordSource passwordSource)
        {
            var model = new ExtractedTextViewModel(
                await new PdfReader(passwordSource).ReadFromAsync(pdfBits));
            await model.LoadPageAsync(1);
            RenderTarget = model;
        }

        public void SetPage(int page) => (RenderTarget as ExtractedTextViewModel)?.LoadPageAsync(page);
    }
}