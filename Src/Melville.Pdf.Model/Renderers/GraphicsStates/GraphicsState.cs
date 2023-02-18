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
using Melville.Pdf.Model.Renderers.DocumentRenderers;
using Melville.Pdf.Model.Renderers.FontRenderings;

namespace Melville.Pdf.Model.Renderers.GraphicsStates;

/// <summary>
/// This is an interface of all the methods for the state stack which is used a lot for method forwarding.
/// </summary>
public interface IGraphicsState : IStateChangingOperations
{
    /// <summary>
    /// Get the current Graphics State object
    /// </summary>
    GraphicsState CurrentState();
}


/// <summary>
/// This is a base class for thie renderer specific graphics state.  The generic type
/// is the native brush types in the target renderer.
/// </summary>
/// <typeparam name="T">Type that the render target uses to represent a brush.</typeparam>
[MacroItem("T", "StrokeBrush", "default!")]
[MacroItem("T", "NonstrokeBrush", "default!")]
[MacroCode("public ~0~ ~1~ {get; private set;} = ~2~;")]
[MacroCode("    ~1~ = ((GraphicsState<T>)other).~1~;",
    Prefix = """
       public override void CopyFrom(GraphicsState other)
       {
           base.CopyFrom(other);
       """, Postfix = "}")]
public abstract partial class GraphicsState<T> : GraphicsState
{
    /// <summary>
    /// Create a graphics state.
    /// </summary>
    protected GraphicsState()
    {
        StrokeColorChanged();
        NonstrokeColorChanged();
    }

    /// <inheritdoc />
    protected sealed override void StrokeColorChanged() =>
        StrokeBrush = TryRegisterDispose(CreateSolidBrush(StrokeColor));

    /// <inheritdoc />
    protected sealed override void NonstrokeColorChanged() => NonstrokeBrush =
        TryRegisterDispose(CreateSolidBrush(NonstrokeColor));

    /// <summary>
    /// Create a solid color brush from a device color.
    /// </summary>
    /// <param name="color">The color for the brush</param>
    protected abstract T CreateSolidBrush(DeviceColor color);

    /// <inheritdoc />
    public override async ValueTask SetStrokePattern(PdfDictionary pattern, DocumentRenderer parentRenderer) => 
        StrokeBrush = await CreatePatternBrush(pattern, parentRenderer).CA();

    /// <inheritdoc />
    public override async ValueTask SetNonstrokePattern(PdfDictionary pattern, DocumentRenderer parentRenderer) =>
        NonstrokeBrush = await CreatePatternBrush(pattern, parentRenderer).CA();
    /// <summary>
    /// Create a pattern brush specific to the target renderer.
    /// </summary>
    /// <param name="pattern">The pattern to put in the brush.</param>
    /// <param name="parentRenderer">DocumentRenderer with resources to render the pattern.</param>
    /// <returns>Valuetask governing completion of the task.</returns>
    protected abstract ValueTask<T> CreatePatternBrush(PdfDictionary pattern, DocumentRenderer parentRenderer);
}

public abstract partial class GraphicsState: IGraphicsState, IDisposable
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
    [MacroItem("double", "HorizontalTextScale", "1.0")]
    [MacroItem("TextRendering", "TextRender", "TextRendering.Fill")]
    [MacroItem("IRealizedFont", "Typeface", "NullRealizedFont.Instance")]
    [MacroItem("double", "FontSize", "0.0")]

    //PageSizes
    [MacroItem("double", "PageWidth", "1")]
    [MacroItem("double", "PageHeight", "1")]

    // code
    [MacroCode("public ~0~ ~1~ {get; private set;} = ~2~;")]
    [MacroCode("    ~1~ = other.~1~;", Prefix = """
         public virtual void CopyFrom(GraphicsState other)
         {
         """, Postfix = "}")]
    public void SaveGraphicsState() => throw new NotSupportedException("Needs to be intercepted");
    public void RestoreGraphicsState()=> throw new NotSupportedException("Needs to be intercepted");

    public void ModifyTransformMatrix(in Matrix3x2 newTransform) => 
        TransformMatrix = newTransform * TransformMatrix;

    public void ResetTransformMatrix() => 
        TransformMatrix = Matrix3x2.Identity;

    public Vector2 ApplyCurrentTransform(in Vector2 point) => Vector2.Transform(point, TransformMatrix);

    #region Drawing

    /// <inheritdoc />
    public void SetLineWidth(double width) => LineWidth = width;
    /// <inheritdoc />
    public void SetLineCap(LineCap cap) => LineCap = cap;
    /// <inheritdoc />
    public void SetLineJoinStyle(LineJoinStyle lineJoinStyle) => LineJoinStyle = lineJoinStyle;
    /// <inheritdoc />
    public void SetMiterLimit(double miter) => MiterLimit = miter;

    /// <inheritdoc />
    public void SetLineDashPattern(double dashPhase, in ReadOnlySpan<double> dashArray)
    {
        DashPhase = dashPhase;
        // this copies the dasharray on write, which is unavoidable, but then it reuses the array object when
        // we duplicate the graphicsstate, which we expect to happen frequently.
        DashArray = dashArray.ToArray(); 
    }

    /// <inheritdoc />
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
    /// <summary>
    /// Load Graphics State from a Graphic State Dictionary
    /// </summary>
    /// <param name="dictionary"></param>
    /// <returns></returns>
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

    /// <inheritdoc />
    public void SetCharSpace(double value) => CharacterSpacing = value;
    /// <inheritdoc />
    public void SetWordSpace(double value) => WordSpacing = value;
    /// <inheritdoc />
    public void SetHorizontalTextScaling(double value) => HorizontalTextScale = value/100.0;
    /// <inheritdoc />
    public void SetTextLeading(double value) => TextLeading = value;
    /// <inheritdoc />
    public ValueTask SetFont(PdfName font, double size)
    {
        FontSize = size;
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public void SetTextRender(TextRendering rendering) => TextRender = rendering;
    /// <inheritdoc />
    public void SetTextRise(double value) => TextRise = value;

    #endregion

    #region Text Position

    /// <summary>
    /// Set the text matrix
    /// </summary>
    /// <param name="value">The new text matrix.</param>
    public void SetTextMatrix(in Matrix3x2 value) => TextMatrix = value;

    /// <summary>
    /// Set the text line matrix
    /// </summary>
    /// <param name="value"></param>
    public void SetTextLineMatrix(in Matrix3x2 value) => TextLineMatrix = value;

    /// <summary>
    /// Set both the text and text line matrices.
    /// </summary>
    /// <param name="value">The new value for each matrices.</param>
    public void SetBothTextMatrices(in Matrix3x2 value)
    {
        SetTextMatrix(value);
        SetTextLineMatrix(value);
    }

    #endregion

    #region Color

    /// <summary>
    /// Called when the stroke color changes, and allows the child to update the native brush.
    /// </summary>
    protected abstract void StrokeColorChanged();
    /// <summary>
    /// Called when the nonstroke color changes, and allows the child to update the native brush.
    /// </summary>
    protected abstract void NonstrokeColorChanged();

    /// <summary>
    /// Set the rendering intent
    /// </summary>
    /// <param name="intent">The new rendering intent.</param>
    public void SetRenderIntent(RenderIntentName intent) => RenderIntent = intent;

    /// <summary>
    /// Set the stroke color space.
    /// </summary>
    /// <param name="colorSpace">The new colorspace</param>
    public void SetStrokeColorSpace(IColorSpace colorSpace)
    {
        SetStrokeColor(colorSpace.DefaultColor());
        StrokeColorSpace = colorSpace;
    }
    
    /// <summary>
    /// Sets the nonstroking colorspace.
    /// </summary>
    /// <param name="colorSpace">The new colorspace.</param>
    public void SetNonstrokeColorSpace(IColorSpace colorSpace)
    {
        SetNonstrokeColor(colorSpace.DefaultColor());
        NonstrokeColorSpace = colorSpace;
    }

    /// <inheritdoc />
    public GraphicsState CurrentState() => this;

    /// <summary>
    /// Set the stroking color.
    /// </summary>
    /// <param name="color">A span of doubles representing the color in
    /// the current colorspace</param>
    public void SetStrokeColor(in ReadOnlySpan<double> color) => 
        SetStrokeColor(StrokeColorSpace.SetColor(color));
    
    private void SetStrokeColor(DeviceColor color)
    {
        StrokeColor = color;
        StrokeColorChanged();
    }
    /// <summary>
    /// Set the stroking color.
    /// </summary>
    /// <param name="color">A span of doubles representing the color in
    /// the current colorspace</param>
    public void SetNonstrokingColor(in ReadOnlySpan<double> color) => 
       SetNonstrokeColor(NonstrokeColorSpace.SetColor(color));

    private void SetNonstrokeColor(DeviceColor color)
    {
        NonstrokeColor = color;
        NonstrokeColorChanged();
    }
    #endregion

    /// <summary>
    /// Set the current font.
    /// </summary>
    /// <param name="realizedFont">The font to use.</param>
    public void SetTypeface(IRealizedFont realizedFont) => 
        Typeface = TryRegisterDispose(realizedFont);

    /// <summary>
    /// Set the size of the printed page, in Pdf units
    /// </summary>
    /// <param name="width">The desired width.</param>
    /// <param name="height">The desired height.</param>
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

    /// <summary>
    /// Set a pattern brush to the current stroke brush
    /// </summary>
    /// <param name="pattern">The pattern dictionary for the new brush.</param>
    /// <param name="parentRenderer">The document renderer with resources to paint the brush.</param>
    /// <returns>A valuetask noting the completion of the action.</returns>
    public abstract ValueTask SetStrokePattern(PdfDictionary pattern, DocumentRenderer parentRenderer);
    /// <summary>
    /// Set a pattern brush to the current nonstroking brush
    /// </summary>
    /// <param name="pattern">The pattern dictionary for the new brush.</param>
    /// <param name="parentRenderer">The document renderer with resources to paint the brush.</param>
    /// <returns>A valuetask noting the completion of the action.</returns>
    public abstract ValueTask SetNonstrokePattern(PdfDictionary pattern, DocumentRenderer parentRenderer);

    /// <summary>
    /// Designate the current transform matrix as the initial matrix for the page.
    /// </summary>
    public void StoreInitialTransform()
    {
        InitialTransformMatrix = TransformMatrix;
    }
}

public static class GraphicsStateOperations
{
    /// <summary>
    /// Compute a transform that will revert the current matrix bac; to the current matrix.
    /// </summary>
    /// <param name="gs"></param>
    /// <returns></returns>
    public static Matrix3x2 RevertToPixelsMatrix(this GraphicsState gs)
    {
        Matrix3x2.Invert(gs.InitialTransformMatrix, out var invInitial);
        Matrix3x2.Invert(gs.TransformMatrix * invInitial, out var ret);
        return ret;
    }
}