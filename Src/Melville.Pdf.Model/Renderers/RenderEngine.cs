using System;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.Model.Documents;

namespace Melville.Pdf.Model.Renderers;

public interface IRenderTarget
{
}
public partial class RenderEngine: IContentStreamOperations
{
    private readonly PdfPage page;
    private readonly IRenderTarget target;

    public RenderEngine(PdfPage page, IRenderTarget target)
    {
        this.page = page;
        this.target = target;
    }

    [DelegateTo]
    private IStateChangingOperations StateoPS => throw new NotImplementedException("State change not implemented");
    [DelegateTo]
    private IDrawingOperations Drawing => throw new NotImplementedException("Drawing not implemented");
    [DelegateTo]
    private IColorOperations Color => throw new NotImplementedException("Color not implemented");
    [DelegateTo]
    private ITextObjectOperations TextObject => throw new NotImplementedException("Text Object not implemented");
    [DelegateTo]
    private ITextBlockOperations TextBlock => throw new NotImplementedException("Text Block not implemented");
    [DelegateTo]
    private IMarkedContentCSOperations Marked => throw new NotImplementedException("Marked Operations not implemented");
    [DelegateTo]
    private ICompatibilityOperations Compat => 
        throw new NotImplementedException("Compatibility Operations not implemented");
    [DelegateTo]
    private IFontMetricsOperations FontMetrics => 
        throw new NotImplementedException("Compatibility Operations not implemented");
}