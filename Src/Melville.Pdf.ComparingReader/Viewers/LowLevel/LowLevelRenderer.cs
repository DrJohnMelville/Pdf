using System.IO;
using Melville.Pdf.ComparingReader.Renderers;
using Melville.Pdf.WpfViewerParts.LowLevelViewer;

namespace Melville.Pdf.ComparingReader.Viewers.LowLevel;

public class LowLevelRenderer : IRenderer
{
    private LowLevelViewModel model;

    public LowLevelRenderer(LowLevelViewModel model)
    {
        this.model = model;
    }

    public string DisplayName => "Low Level";

    public object RenderTarget => model;

    public void SetTarget(Stream pdfBits)
    {
        model.SetStream(pdfBits);
    }
}

