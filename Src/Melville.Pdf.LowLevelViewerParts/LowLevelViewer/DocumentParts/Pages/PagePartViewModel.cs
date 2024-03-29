﻿using System.Windows.Media;
using Melville.INPC;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.Renderers.DocumentRenderers;
using Melville.Pdf.Model.Renderers.FontRenderings.DefaultFonts;
using Melville.Pdf.Wpf.Rendering;

namespace Melville.Pdf.LowLevelViewerParts.LowLevelViewer.DocumentParts.Pages;

public partial class PagePartViewModel: DocumentPart
{
    [AutoNotify]private ImageSource? renderedPage;
    private readonly PdfPage page;
    
    private ImageSource? RenderedPageGetFilter(ImageSource? value)
    {
        if (value == null) RenderPage();
        return value;
    }

    private async void RenderPage()
    {
        try
        {
            var image = await new RenderToDrawingGroup(
                DocumentRendererFactory.CreateRenderer(page, WindowsDefaultFonts.Instance), 0)
                .RenderToDrawingImageAsync();
            image.Freeze();
            RenderedPage =image;
        }
        catch (Exception )
        {
        }
    }


    public override object? DetailView => this;

    public PagePartViewModel(string title, IReadOnlyList<DocumentPart> children, PdfPage page) : base(title, children)
    {
        this.page = page;
    }
}