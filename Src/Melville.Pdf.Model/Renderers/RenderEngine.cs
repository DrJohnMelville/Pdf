using System;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.Model.Documents;

namespace Melville.Pdf.Model.Renderers;

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

    #region Graphics State
    [DelegateTo] private IGraphiscState StateOps => target.GrapicsStateChange;

    public async ValueTask LoadGraphicStateDictionary(PdfName dictionaryName) =>
        await StateOps.LoadGraphicStateDictionary(
            await page.GetResourceObject(ResourceTypeName.ExtGState, dictionaryName) as 
                PdfDictionary ?? throw new PdfParseException($"Cannot find GraphicsState {dictionaryName}"));
    #endregion
    
    #region Drawing Operations

    public void MoveTo(double x, double y) => target.MoveTo(x,y);
    public void LineTo(double x, double y) => target.LineTo(x,y);
    public void EndPathWithNoOp() => target.ClearPath();
    public void StrokePath() => target.StrokePath();

    #endregion
}