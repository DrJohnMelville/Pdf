using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using Melville.INPC;
using Melville.MVVM.Wpf.DiParameterSources;
using Melville.MVVM.Wpf.MvvmDialogs;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.ComparingReader.Renderers;
using Melville.Pdf.FormReader;
using Melville.Pdf.FormReader.Interface;
using Melville.Pdf.LowLevel.Parsing.ParserContext;
using Melville.Pdf.LowLevel.Writers.DocumentWriters;

namespace Melville.Pdf.ComparingReader.Viewers.FormViewer;

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

    public async ValueTask WriteNewDocumentAsync([FromServices] IOpenSaveFile dlg)
    {
        var output = dlg.GetSaveFile(null, "pdf", "Pdf File (*.pdf)|*.pdf", "Select a File to write too");
        if (output == null || Form == null) return;

        await using var outputStream = await output.CreateWrite();
        await (await Form.CreateModifiedDocumentAsync().CA()).WriteToWithXrefStreamAsync(outputStream);
    }
}