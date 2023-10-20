using System;
using System.IO;
using System.Windows.Forms;
using Melville.INPC;
using Melville.Pdf.ComparingReader.Renderers;
using Melville.Pdf.FormReader;
using Melville.Pdf.LowLevel.Parsing.ParserContext;

namespace Melville.Pdf.ComparingReader.Viewers.FormViewer
{
    [AutoNotify]
    public partial class FormViewerViewModel: IRenderer
    {
        public string DisplayName => "Form Viewer";
        public object RenderTarget => this;
        public IPdfForm? Form { get; private set; } 
        public async void SetTarget(Stream pdfBits, IPasswordSource passwordSource)
        {
            try
            {
                Form = await FormReaderFacade.ReadFormAsync(pdfBits, passwordSource);
            }
            catch (Exception )
            {
            }
        }

        public void SetPage(int page)
        {
            // do nothing
        }
    }
}