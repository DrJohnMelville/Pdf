using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Threading;
using Melville.INPC;
using Melville.Pdf.ComparingReader.Renderers;
using Melville.Pdf.LowLevel.Parsing.ParserContext;

namespace Melville.Pdf.ComparingReader.Viewers.GenericImageViewers;

public interface IImageRenderer
{
    ValueTask<ImageSource> LoadFirstPage(Stream pdfBits, string password);
}

public partial class ImageViewerViewModel : IRenderer
{
    public string DisplayName { get; }
    public object RenderTarget => this;
    [AutoNotify] private ImageSource? image;
    private readonly IPasswordSource passwords;
    private readonly IImageRenderer renderer;

    public ImageViewerViewModel(IPasswordSource passwords, IImageRenderer renderer, string displayName)
    {
        this.passwords = passwords;
        this.renderer = renderer;
        DisplayName = displayName;
    }

    public async void SetTarget(Stream pdfBits)
    {
        try
        {
            var (password, _) = await passwords.GetPassword();
            pdfBits.Seek(0, SeekOrigin.Begin);
            Image = await renderer.LoadFirstPage(pdfBits, password ??"");
        }
        catch (Exception)
        {
            Image = null;
        }
    }

}
