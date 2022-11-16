using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.CharacterEncoding;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.Model.Renderers.Colors;
using Melville.Pdf.Model.Renderers.FontRenderings;

namespace Melville.Pdf.Model.Renderers.GraphicsStates;

public interface IGraphicsState : IStateChangingOperations
{
    ValueTask LoadGraphicStateDictionary(PdfDictionary dictionary);
    void SetStrokeColorSpace(IColorSpace colorSpace);
    void SetStrokeColor(in ReadOnlySpan<double> components);
    void SetNonstrokeColorSpace(IColorSpace colorSpace);
    void SetNonstrokingColor(in ReadOnlySpan<double> components);
    GraphicsState CurrentState();
    void SetTextMatrix(in Matrix3x2 value);
    void SetTextLineMatrix(in Matrix3x2 value);
    void SetBothTextMatrices(in Matrix3x2 value);
    void StoreInitialTransform();
}


[MacroItem("T", "StrokeBrush", "default!")]
[MacroItem("T", "NonstrokeBrush", "default!")]
// code
[MacroCode("public ~0~ ~1~ {get; private set;} = ~2~;")]
[MacroCode("    ~1~ = ((GraphicsState<T>)other).~1~;",
    Prefix = "public override void CopyFrom(GraphicsState other){ base.CopyFrom(other);", Postfix = "}")]
public abstract partial class GraphicsState<T> : GraphicsState
{
    protected GraphicsState()
    {
        StrokeColorChanged();
        NonstrokeColorChanged();
    }

    protected sealed override void StrokeColorChanged() =>
        StrokeBrush = TryRegisterDispose(CreateSolidBrush(StrokeColor));

    protected sealed override void NonstrokeColorChanged() => NonstrokeBrush =
        TryRegisterDispose(CreateSolidBrush(NonstrokeColor));

    protected abstract T CreateSolidBrush(DeviceColor color);

    public override async ValueTask SetStrokePattern(PdfDictionary pattern, DocumentRenderer parentRenderer) => 
        StrokeBrush = await CreatePatternBrush(pattern, parentRenderer).CA();
    public override async ValueTask SetNonstrokePattern(PdfDictionary pattern, DocumentRenderer parentRenderer) =>
        NonstrokeBrush = await CreatePatternBrush(pattern, parentRenderer).CA();
    protected abstract ValueTask<T> CreatePatternBrush(PdfDictionary pattern, DocumentRenderer parentRenderer);
}

public abstract partial  class GraphicsState: IGraphicsState, IDisposable
{
    [MacroItem("Matrix3x2", "TransformMatrix", "Matrix3x2.Identity")]
    [MacroItem("Matrix3x2", "InitialTransformMatrix", "Matrix3x2.Identity")]
    [MacroItem("Matrix3x2", "TextMatrix", "Matrix3x2.Identity")]
    [MacroItem("Matrix3x2", "TextLineMatrix", "Matrix3x2.Identity")]
    [MacroItem("double", "LineWidth", "1.0")]
    [MacroItem("double", "MiterLimit", "10.0")]
    [MacroItem("LineJoinStyle", "LineJoinStyle", "LineJoinStyle.Miter")]
    [MacroItem("LineCap", "LineCap", "LineCap.Butt")]
    [MacroItem("double", "DashPhase", "0.0")]
    [MacroItem("double", "FlatnessTolerance", "1.0")]
    [MacroItem("double[]", "DashArray", "Array.Empty<double>()")]
    [MacroItem("RenderIntentName", "RenderIntent", "RenderIntentName.RelativeColoriMetric")]
    [MacroItem("IColorSpace", "StrokeColorSpace", "DeviceGray.Instance")]
    [MacroItem("IColorSpace", "NonstrokeColorSpace", "DeviceGray.Instance")]
    [MacroItem("DeviceColor", "StrokeColor", "DeviceColor.Black")]
    [MacroItem("DeviceColor", "NonstrokeColor", "DeviceColor.Black")]

    // Text Properties
    [MacroItem("double", "CharacterSpacing", "0.0")]
    [MacroItem("double", "WordSpacing", "0.0")]
    [MacroItem("double", "TextLeading", "0.0")]
    [MacroItem("double", "TextRise", "0.0")]
    [MacroItem("double", "HorizontalTextScale", "100.0")]
    [MacroItem("TextRendering", "TextRender", "TextRendering.Fill")]
    [MacroItem("IRealizedFont", "Typeface", "NullRealizedFont.Instance")]
    [MacroItem("double", "FontSize", "0.0")]

    //PageSizes
    [MacroItem("double", "PageWidth", "1")]
    [MacroItem("double", "PageHeight", "1")]

    // code
    [MacroCode("public ~0~ ~1~ {get; private set;} = ~2~;")]
    [MacroCode("    ~1~ = other.~1~;", Prefix = "public virtual void CopyFrom(GraphicsState other){", Postfix = "}")]
    public void SaveGraphicsState() => throw new NotSupportedException("Needs to be intercepted");
    public void RestoreGraphicsState()=> throw new NotSupportedException("Needs to be intercepted");

    public void ModifyTransformMatrix(in Matrix3x2 newTransform) => 
        TransformMatrix = newTransform * TransformMatrix;

    public void ResetTransformMatrix() => 
        TransformMatrix = Matrix3x2.Identity;

    public Vector2 ApplyCurrentTransform(in Vector2 point) => Vector2.Transform(point, TransformMatrix);

    #region Drawing

    public void SetLineWidth(double width) => LineWidth = width;
    public void SetLineCap(LineCap cap) => LineCap = cap;
    public void SetLineJoinStyle(LineJoinStyle lineJoinStyle) => LineJoinStyle = lineJoinStyle;
    public void SetMiterLimit(double miter) => MiterLimit = miter;

    public void SetLineDashPattern(double dashPhase, in ReadOnlySpan<double> dashArray)
    {
        DashPhase = dashPhase;
        // this copies the dasharray on write, which is unavoidable, but then it reuses the array object when
        // we duplicate the graphicsstate, which we expect to happen frequently.
        DashArray = dashArray.ToArray(); 
    }
    
    private async ValueTask SetLineDashPattern(PdfArray entryValue)
    {
        DashPhase = (await entryValue.GetAsync<PdfNumber>(1).CA()).DoubleValue;
        var pattern = await entryValue.GetAsync<PdfArray>(0).CA();
        DashArray = await pattern.AsDoublesAsync().CA();
    }
    
    // as of 11/18/2021 this parameter is ignored by both the Pdf and Skia renderers, but
    // we will preserve the property.
    public void SetFlatnessTolerance(double flatness) => FlatnessTolerance = flatness;
    

    #endregion    
    
    public async ValueTask LoadGraphicStateDictionary(PdfDictionary dictionary)
    {
        foreach (var entry in dictionary.RawItems)
        {
            var hashCode = entry.Key.GetHashCode();
            switch (hashCode)
            {
                case KnownNameKeys.LW:
                    SetLineWidth((await EntryValue<PdfNumber>(entry).CA()).DoubleValue);
                    break;
                case KnownNameKeys.FL:
                    SetFlatnessTolerance((await EntryValue<PdfNumber>(entry).CA()).DoubleValue);
                    break;
                case KnownNameKeys.LC:
                    SetLineCap((LineCap)(await EntryValue<PdfNumber>(entry).CA()).IntValue);
                    break;
                case KnownNameKeys.LJ:
                    SetLineJoinStyle((LineJoinStyle)(await EntryValue<PdfNumber>(entry).CA()).IntValue);
                    break;
                case KnownNameKeys.ML:
                    SetMiterLimit((await EntryValue<PdfNumber>(entry).CA()).DoubleValue);
                    break;
                case KnownNameKeys.D:
                    await SetLineDashPattern(await EntryValue<PdfArray>(entry).CA()).CA();
                    break;
                case KnownNameKeys.RI:
                    SetRenderIntent(new RenderIntentName(await EntryValue<PdfName>(entry).CA()));
                    break;
            }
        }
    }
    
    private static async ValueTask<T> EntryValue<T>(KeyValuePair<PdfName, PdfObject> entry)
        where T:PdfObject
    {
        return (T) await entry.Value.DirectValueAsync().CA();
    }

    #region Text State

    public WritingMode WritingMode => WritingMode.LeftToRight;
    
    public void SetCharSpace(double value) => CharacterSpacing = value;
    public void SetWordSpace(double value) => WordSpacing = value;
    public void SetHorizontalTextScaling(double value) => HorizontalTextScale = value;
    public void SetTextLeading(double value) => TextLeading = value;
    public ValueTask SetFont(PdfName font, double size)
    {
        FontSize = size;
        return ValueTask.CompletedTask;
    }

    public void SetTextRender(TextRendering rendering) => TextRender = rendering;
    public void SetTextRise(double value) => TextRise = value;

    #endregion

    #region Text Position

    public void SetTextMatrix(in Matrix3x2 value) => TextMatrix = value;
    public void SetTextLineMatrix(in Matrix3x2 value) => TextLineMatrix = value;
    public void SetBothTextMatrices(in Matrix3x2 value)
    {
        SetTextMatrix(value);
        SetTextLineMatrix(value);
    }

    #endregion

    #region Color

    protected abstract void StrokeColorChanged();
    protected abstract void NonstrokeColorChanged();
    public void SetRenderIntent(RenderIntentName intent) => RenderIntent = intent;
    public void SetStrokeColorSpace(IColorSpace colorSpace)
    {
        SetStrokeColor(colorSpace.DefaultColor());
        StrokeColorSpace = colorSpace;
    }
    
    public void SetNonstrokeColorSpace(IColorSpace colorSpace)
    {
        SetNonstrokeColor(colorSpace.DefaultColor());
        NonstrokeColorSpace = colorSpace;
    }

    public GraphicsState CurrentState() => this;

    public void SetStrokeColor(in ReadOnlySpan<double> color) => 
        SetStrokeColor(StrokeColorSpace.SetColor(color));
    private void SetStrokeColor(DeviceColor color)
    {
        StrokeColor = color;
        StrokeColorChanged();
    }
    public void SetNonstrokingColor(in ReadOnlySpan<double> color) => 
       SetNonstrokeColor(NonstrokeColorSpace.SetColor(color));
    private void SetNonstrokeColor(DeviceColor color)
    {
        NonstrokeColor = color;
        NonstrokeColorChanged();
    }
    #endregion

    public void SetTypeface(IRealizedFont realizedFont)
    {
        Typeface = TryRegisterDispose(realizedFont);
    }

    public void SetPageSize(double width, double height)
    {
        PageWidth = width;
        PageHeight = height;
    }

    #region Disposal

    private readonly DisposeList pendingDispose = new();
    public void Dispose() => pendingDispose.Dispose();
    protected T TryRegisterDispose<T>(T item) => pendingDispose.TryRegister(item);

    #endregion

    public abstract ValueTask SetStrokePattern(PdfDictionary pattern, DocumentRenderer parentRenderer);
    public abstract ValueTask SetNonstrokePattern(PdfDictionary pattern, DocumentRenderer parentRenderer);

    public void StoreInitialTransform()
    {
        InitialTransformMatrix = TransformMatrix;
    }
}

public static class GraphicsStateOperations
{
    public static Matrix3x2 RevertToPixelsMatrix(this GraphicsState gs)
    {
        Matrix3x2.Invert(gs.InitialTransformMatrix, out var invInitial);
        Matrix3x2.Invert(gs.TransformMatrix * invInitial, out var ret);
        return ret;
    }
}