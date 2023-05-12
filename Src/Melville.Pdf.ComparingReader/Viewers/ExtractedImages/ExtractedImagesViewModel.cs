using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Melville.Hacks;
using Melville.INPC;
using Melville.Lists;
using Melville.Pdf.ComparingReader.Renderers;
using Melville.Pdf.ImageExtractor;
using Melville.Pdf.Model;
using Melville.Pdf.Model.Renderers.DocumentRenderers;
using Melville.Pdf.Wpf.Rendering;

namespace Melville.Pdf.ComparingReader.Viewers.ExtractedImages;

internal partial class ExtractedImagesViewModel: IRenderer
{
    public string DisplayName => "Extracted Images";
    public object RenderTarget => this;
    private DocumentRenderer? renderer;
    [AutoNotify] private IList<ImageSource> images  = Array.Empty<ImageSource>();

    public async void SetTarget(Stream pdfBits)
    {
        renderer = await new PdfReader().ReadFrom(pdfBits);
        SetPage(1);
    }

    public async void SetPage(int page)
    {
        if (renderer == null) return;
        var images = new List<ImageSource>();
        foreach (var image in await renderer.ImagesFrom(page))
        {
            images.Add(await image.ToWpfBitmap());
        }
        Images = images;
    }
}