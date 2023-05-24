using System;
using System.Collections.Generic;
using System.Linq;
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
[MacroItem("T", "StrokeBrush", "Brush used to draw outlines on figures and glyphs")]
[MacroItem("T", "NonstrokeBrush", "Brush used to fill figures and glyphs")]
[MacroCode("""
        /// <summary>
        /// ~2~
        /// </summary>
        public ~0~ ~1~ {get; private set;} = default!;
        """)]
[MacroCode("    ~1~ = ((GraphicsState<T>)other).~1~;",
    Prefix = """
       /// <inheritdoc />
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

/// <summary>
/// This represents the current state of the Pdf Graphics Context
/// </summary>
public abstract partial class GraphicsState: IGraphicsState, IDisposable
{
    /// <inheritdoc />
    [MacroItem("Matrix3x2", "TransformMatrix", "Matrix3x2.Identity", "Transforms local coordinates to device coordinates")]
    [MacroItem("Matrix3x2", "InitialTransformMatrix", "Matrix3x2.Identity", "Transforms the visible box to device coordinates")]
    [MacroItem("Matrix3x2", "TextMatrix", "Matrix3x2.Identity", "Transform 0,0 to the location of the next character to be drawn")]
    [MacroItem("Matrix3x2", "TextLineMatrix", "Matrix3x2.Identity", "Transform for the beginning of the current text line.")]
    [MacroItem("double", "LineWidth", "1.0", "Width of stroked lines, in local coordinates.")]
    [MacroItem("double", "MiterLimit", "10.0", "Controls how long to point of a miter line joint can be.")]
    [MacroItem("LineJoinStyle", "LineJoinStyle", "LineJoinStyle.Miter", "Controls how the joint between two line segmets is drawn")]
    [MacroItem("LineCap", "LineCap", "LineCap.Butt", "Controls how the ends of of paths are drawn")]
    [MacroItem("double", "DashPhase", "0.0", "The initial phase of a dashed line.")]
    [MacroItem("double", "FlatnessTolerance", "1.0", "Not currently used, but could control the flatness precision in bezier curve rendering")]
    [MacroItem("double[]", "DashArray", "Array.Empty<double>()", "dash or dot pattern for dotted lines")]
    [MacroItem("RenderIntentName", "RenderIntent", "RenderIntentName.RelativeColoriMetric", "The desired rendering intent for various color transformations")]
    [MacroItem("IColorSpace", "StrokeColorSpace", "DeviceGray.Instance", "Color space for stroking brushes.")]
    [MacroItem("IColorSpace", "NonstrokeColorSpace", "DeviceGray.Instance", "Color space for nonstroking brushes.")]
    [MacroItem("DeviceColor", "StrokeColor", "DeviceColor.Black", "Color for stroking brushes.")]
    [MacroItem("DeviceColor", "NonstrokeColor", "DeviceColor.Black", "Color for nonstroking brushes")]

    // Text Properties
    [MacroItem("double", "CharacterSpacing", "0.0", "Space to add between characters")]
    [MacroItem("double", "WordSpacing", "0.0", "Additional space to add to a ' ' (space or 0x20) character")]
    [MacroItem("double", "TextLeading", "0.0", "The space between two lines of text in local coordinate units.")]
    [MacroItem("double", "TextRise", "0.0", "The vertical offset of the next character above the text baseline.")]
    [MacroItem("double", "HorizontalTextScale", "1.0", "Factor by which text should be stretched or compressed horizontally.")]
    [MacroItem("TextRendering", "TextRender", "TextRendering.Fill", "Describes whether text should be stroked, filled, and or added to the current clipping region.")]
    [MacroItem("IRealizedFont", "Typeface", "NullRealizedFont.Instance", "The font to write characters with.")]
    [MacroItem("double", "FontSize", "0.0", "The font size to write characters.")]

    //PageSizes
    [MacroItem("double", "PageWidth", "1", "Width of the page in device pixels")]
    [MacroItem("double", "PageHeight", "1", "Height of the page in device pixels")]

    // code
    [MacroCode("""
          /// <summary>
          /// ~3~
          /// </summary>
          public ~0~ ~1~ {get; private set;} = ~2~;

          """)]
    [MacroCode("    ~1~ = other.~1~;", Prefix = """
         /// <summary>
         /// Duplicate a GraphicsState by shallow copying all of its values.
         /// </summary>
         public virtual void CopyFrom(GraphicsState other)
         {
         """, Postfix = "}")]
    public void SaveGraphicsState() => throw new NotSupportedException("Needs to be intercepted");

    /// <inheritdoc />
    public void RestoreGraphicsState()=> throw new NotSupportedException("Needs to be intercepted");

    /// <inheritdoc />
    public void ModifyTransformMatrix(in Matrix3x2 newTransform) => 
        TransformMatrix = newTransform * TransformMatrix;

    /// <summary>
    /// Sets the transform matrix to the identity matrix.
    /// </summary>
    public void ResetTransformMatrix() => 
        TransformMatrix = Matrix3x2.Identity;

    /// <summary>
    /// Transform a point using the current transform matrix.
    /// </summary>
    /// <param name="point">The point to transform, in local coordinates.</param>
    /// <returns>The input point, expressed in device coordinates.</returns>
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
    
    /// <summary>
    /// Content stream operator i.
    ///
    /// As of 3/1/2023 both renderers (WPF and Skia) ignore this parameter.  I preserve this
    /// value because it is  part of the PDF standard and it may be useful to a renderer some day.
    /// </summary>
    /// <param name="flatness"></param>
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

    /// <summary>
    /// The current writing mode, although as of 3/1/2023 only left to right is supported.
    /// </summary>
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
    public ValueTask SetFontAsync(PdfName font, double size)
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

    /// <inheritdoc />
    public void Dispose() => pendingDispose.Dispose();
    /// <summary>
    /// Adds an item to the list of items to be disposed of when the graphic state is disposed.
    /// </summary>
    /// <param name="item">The item to be disposed</param>
    /// <typeparam name="T">The type of the argument passed</typeparam>
    /// <returns>The parameter passed in</returns>
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