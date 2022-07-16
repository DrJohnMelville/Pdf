using System.IO;
using Melville.Pdf.ComparingReader.Renderers;
using Melville.Pdf.LowLevelViewerParts.LowLevelViewer;

namespace Melville.Pdf.ComparingReader.Viewers.LowLevel;

public class LowLevelRenderer : IRenderer
{
    private LowLevelRenderViewModel model;

    public LowLevelRenderer(LowLevelViewModel model)
    {
        this.model = new LowLevelRenderViewModel(model);
    }

    public string DisplayName => "Low Level";

    public object RenderTarget => model;

    public void SetTarget(Stream pdfBits) => model.InnerModel.SetStream(pdfBits);

    public void SetPage(int page) { model.InnerModel.JumpTOPage(page-1); }
}