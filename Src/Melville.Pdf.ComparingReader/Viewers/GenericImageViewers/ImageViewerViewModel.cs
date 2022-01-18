using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Threading;
using Melville.INPC;
using Melville.Pdf.ComparingReader.Renderers;
using Melville.Pdf.LowLevel.Parsing.ParserContext;
using Melville.Pdf.Model.Documents;

namespace Melville.Pdf.ComparingReader.Viewers.GenericImageViewers;

public interface IImageRenderer
{
    ValueTask<ImageSource> LoadFirstPage(Stream pdfBits, string password, int page);
}

public abstract class MelvillePdfRenderer : IImageRenderer
{
    public async ValueTask<ImageSource> LoadFirstPage(Stream pdfBits, string password, int page)
    {
        var doc = await PdfDocument.ReadAsync(pdfBits, new SinglePasswordSource(password));
        var pages = await doc.PagesAsync();
        var ret = (await pages.CountAsync()) > 0
            ? await Render(await pages.GetPageAsync(page -1))
            : new DrawingImage();
        ret.Freeze();
        return ret;
    }

    protected abstract ValueTask<ImageSource> Render(PdfPage page);
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

    public ImageViewerViewModel(IPasswordSource passwords, IImageRenderer renderer, string displayName)
    {
        this.passwords = passwords;
        this.renderer = renderer;
        DisplayName = displayName;
    }

    public async void SetTarget(Stream pdfBits, int page)
    {
        try
        {
            var (password, _) = await passwords.GetPassword();
            pdfBits.Seek(0, SeekOrigin.Begin);
            Image = await renderer.LoadFirstPage(pdfBits, password ??"", page);
            Exception = null;
        }
        catch (Exception e)
        {
            Image = null;
            Exception = $"{e.Message}\r\n\r\n{e.StackTrace}";
        }
    }
}
