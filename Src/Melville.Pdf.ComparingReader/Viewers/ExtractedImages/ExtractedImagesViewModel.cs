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
using Melville.Pdf.LowLevel.Parsing.ParserContext;
using Melville.Pdf.Model;
using Melville.Pdf.Model.Renderers.DocumentRenderers;
using Melville.Pdf.Wpf.Rendering;

namespace Melville.Pdf.ComparingReader.Viewers.ExtractedImages;

internal partial class ExtractedImagesViewModel: IRenderer
{
    public string DisplayName => "Extracted Images";
    public object RenderTarget => this;
    private DocumentRenderer? renderer;
    private int lastPage;
    [AutoNotify] private IList<ImageSource> images  = Array.Empty<ImageSource>();
    [FromConstructor] private readonly IPasswordSource passwordSource;
    [AutoNotify] private bool collapseImages;

    private void OnCollapseImagesChanged() => SetPage(lastPage);

    public async void SetTarget(Stream pdfBits, IPasswordSource passwordSource)
    {
        renderer = await new PdfReader(passwordSource).ReadFromAsync(pdfBits);
        SetPage(1);
    }

    public async void SetPage(int page)
    {
        lastPage = page;
        if (renderer == null) return;
        var images = new List<ImageSource>();
        try
        {

            var filteredImages = (await renderer.ImagesFromAsync(page));
            if (CollapseImages)
                filteredImages = filteredImages.CollapseAdjacentImages();
            foreach (var image in filteredImages)
            {
                images.Add(await image.ToWpfBitmap());
            }
        }
        catch (Exception )
        {
        }
        Images = images;
    }
}