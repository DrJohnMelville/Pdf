using System;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.Renderers.Colors;
using Melville.Pdf.Model.Renderers.GraphicsStates;

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

    private double firstX, firstY;
    private double lastX, lasty;

    private void SetLast(double x, double y) => (lastX, lasty) = (x, y);
    private void SetFirst(double x, double y) => (firstX, firstY) = (x, y);

    public void MoveTo(double x, double y)
    {
        target.MoveTo(x, y);
        SetLast(x,y);
        SetFirst(x,y);
    }

    public void LineTo(double x, double y)
    {
        target.LineTo(x, y);
        SetLast(x,y);
    }

    public void CurveTo(double control1X, double control1Y, double control2X, double control2Y, double finalX, double finalY)
    {
        target.CurveTo(control1X, control1Y, control2X, control2Y, finalX, finalY);
        SetLast(finalX, finalY);
    }

    public void CurveToWithoutInitialControl(double control2X, double control2Y, double finalX, double finalY)
    {
        target.CurveTo(lastX, lasty, control2X, control2Y, finalX, finalY);
        SetLast(finalX, finalY);
    }

    public void CurveToWithoutFinalControl(double control1X, double control1Y, double finalX, double finalY)
    {
        target.CurveTo(control1X, control1Y, finalX, finalY, finalX, finalY);
        SetLast(finalX, finalY);
    }

    public void ClosePath()
    {
        target.ClosePath();
        SetLast(firstX, firstY);
    }

    public void Rectangle(double x, double y, double width, double height)
    {
        target.MoveTo(x,y);
        target.LineTo(x+width,y);
        target.LineTo(x+width,y+height);
        target.LineTo(x,y+height);
        target.ClosePath();
    }

    public void StrokePath() => target.StrokePath();
    public void EndPathWithNoOp() => target.EndPathWithNoOp();
    
    public void CloseAndStrokePath()
    {
        throw new NotImplementedException();
    }

    public void FillPath()
    {
        throw new NotImplementedException();
    }

    public void FillPathEvenOdd()
    {
        throw new NotImplementedException();
    }

    public void FillAndStrokePath()
    {
        throw new NotImplementedException();
    }

    public void FillAndStrokePathEvenOdd()
    {
        throw new NotImplementedException();
    }

    public void CloseFillAndStrokePath()
    {
        throw new NotImplementedException();
    }

    public void CloseFillAndStrokePathEvenOdd()
    {
        throw new NotImplementedException();
    }

    public void ClipToPath()
    {
        throw new NotImplementedException();
    }

    public void ClipToPathEvenOdd()
    {
        throw new NotImplementedException();
    }

    public ValueTask DoAsync(PdfName name)
    {
        throw new NotImplementedException();
    }

    public ValueTask DoAsync(PdfStream inlineImage)
    {
        throw new NotImplementedException();
    }

    #endregion

    #region Color Implementation
    
    public async ValueTask SetStrokingColorSpace(PdfName colorSpace) =>
        target.GrapicsStateChange.SetStrokeColorSpace(
            await ColorSpaceFactory.ParseColorSpace(colorSpace, page));

    public async ValueTask SetNonstrokingColorSpace(PdfName colorSpace) =>
        target.GrapicsStateChange.SetNonstrokeColorSpace(
            await ColorSpaceFactory.ParseColorSpace(colorSpace, page));
    
//    private ValueTask<(IColorSpace, DeviceColor)> SetNamespace(PdfName ns)

    public ValueTask SetStrokeColorExtended(PdfName? patternName, in ReadOnlySpan<double> colors)
    {
        throw new NotImplementedException();
    }
    
    public ValueTask SetNonstrokingColorExtended(PdfName? patternName, in ReadOnlySpan<double> colors)
    {
        throw new NotImplementedException();
    }

    public void SetStrokeGray(double grayLevel)
    {
        throw new NotImplementedException();
    }

    public void SetStrokeRGB(double red, double green, double blue)
    {
        throw new NotImplementedException();
    }

    public void SetStrokeCMYK(double cyan, double magenta, double yellow, double black)
    {
        throw new NotImplementedException();
    }

    public void SetNonstrokingGray(double grayLevel)
    {
        throw new NotImplementedException();
    }

    public void SetNonstrokingRGB(double red, double green, double blue)
    {
        throw new NotImplementedException();
    }

    public void SetNonstrokingCMYK(double cyan, double magenta, double yellow, double black)
    {
        throw new NotImplementedException();
    }

    #endregion
}