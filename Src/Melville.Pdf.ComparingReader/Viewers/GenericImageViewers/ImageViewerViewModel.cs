using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media;
using Melville.INPC;
using Melville.Pdf.ComparingReader.Renderers;
using Melville.Pdf.LowLevel.Parsing.ParserContext;
using Melville.Pdf.Model.DocumentRenderers;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.Renderers.FontRenderings.DefaultFonts;
using Melville.Pdf.Wpf.Controls;

namespace Melville.Pdf.ComparingReader.Viewers.GenericImageViewers;

public interface IImageRenderer
{
    ValueTask  SetSource(Stream pdfBits, string password);
    ValueTask<ImageSource> LoadPage(int page);
}

public abstract class MelvillePdfRenderer : IImageRenderer
{
    private DocumentRenderer? source;
    public async ValueTask SetSource(Stream pdfBits, string password)
    {
        source = null;
        source = await DocumentRendererFactory.CreateRendererAsync(
            await PdfDocument.ReadAsync(pdfBits, new SinglePasswordSource(password)),
            new WindowsDefaultFonts()
        );
    }

    public async ValueTask<ImageSource> LoadPage(int page)
    {
        var ret = await TryRenderPage(page);
        ret.Freeze();
        return ret;
    }

    private ValueTask<ImageSource> TryRenderPage(int page) =>
        IsValidPageRender(page)
            ?  Render(source, page - 1)
            : new(new DrawingImage());

    [MemberNotNullWhen(true, nameof(source))]
    private bool IsValidPageRender(int page) => 
        source != null && page >= 1 && page <= source.TotalPages;

    protected abstract ValueTask<ImageSource> Render(DocumentRenderer source, int page);
}

public class SinglePasswordSource : IPasswordSource
{
    private readonly string password;

    public SinglePasswordSource(string password)
    {
        this.password = password;
    }

    public ValueTask<(string?, PasswordType)> GetPassword() => new((password, PasswordType.User));
}

public partial class ImageViewerViewModel : IRenderer
{
    public string DisplayName { get; }
    public object RenderTarget => this;
    [AutoNotify] private ImageSource? image;
    [AutoNotify] private string? exception;
    private readonly IPasswordSource passwords;
    private readonly IImageRenderer renderer;
    private readonly IPageSelector pageSelector;
    private bool fileParseSucceeded = false;

    public ImageViewerViewModel(IPasswordSource passwords, IImageRenderer renderer, string displayName, IPageSelector pageSelector)
    {
        this.passwords = passwords;
        this.renderer = renderer;
        DisplayName = displayName;
        this.pageSelector = pageSelector;
    }
    
    public async void SetTarget(Stream pdfBits)
    {
        fileParseSucceeded = await LoadStream(pdfBits);
        SetPage(pageSelector.Page);
    }

    private async ValueTask<bool> LoadStream(Stream pdfBits)
    {
        try
        {
            var (password, _) = await passwords.GetPassword();
            await renderer.SetSource(pdfBits, password ?? "");
            return true;
        }
        catch (Exception e)
        {
            ReportException(e);
            return false;
        }
    }

    public async void SetPage(int page)
    {
        if  (!fileParseSucceeded) return;
        try
        {
            Image = await renderer.LoadPage(page);
            Exception = null;
        }
        catch (Exception e)
        {
            ReportException(e);
        }
    }

    private void ReportException(Exception e)
    {
        Image = null;
        Exception = $"{e.Message}\r\n\r\n{e.StackTrace}";
    }
}
