using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
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
/// This represents the current state of the Pdf Graphics Context
/// </summary>
public abstract partial class GraphicsState: IGraphicsState, IDisposable
{
    /// <inheritdoc />
    [MacroItem("Matrix3x2", "TransformMatrix", "Matrix3x2.Identity",
        "Transforms local coordinates to device coordinates")]
    [MacroItem("Matrix3x2", "InitialTransformMatrix", "Matrix3x2.Identity",
        "Transforms the visible box to device coordinates")]
    [MacroItem("Matrix3x2", "TextMatrix", "Matrix3x2.Identity",
        "Transform 0,0 to the location of the next character to be drawn")]
    [MacroItem("Matrix3x2", "TextLineMatrix", "Matrix3x2.Identity",
        "Transform for the beginning of the current text line.")]
    [MacroItem("double", "LineWidth", "1.0", "Width of stroked lines, in local coordinates.")]
    [MacroItem("double", "MiterLimit", "10.0", "Controls how long to point of a miter line joint can be.")]
    [MacroItem("LineJoinStyle", "LineJoinStyle", "LineJoinStyle.Miter",
        "Controls how the joint between two line segmets is drawn")]
    [MacroItem("LineCap", "LineCap", "LineCap.Butt", "Controls how the ends of of paths are drawn")]
    [MacroItem("double", "DashPhase", "0.0", "The initial phase of a dashed line.")]
    [MacroItem("double", "FlatnessTolerance", "1.0",
        "Not currently used, but could control the flatness precision in bezier curve rendering")]
    [MacroItem("IReadOnlyList<double>", "DashArray", "Array.Empty<double>()", "dash or dot pattern for dotted lines")]
    [MacroItem("RenderIntentName", "RenderIntent", "RenderIntentName.RelativeColoriMetric",
        "The desired rendering intent for various color transformations")]
    [MacroItem("IColorSpace", "StrokeColorSpace", "DeviceGray.Instance", "Color space for stroking brushes.")]
    [MacroItem("IColorSpace", "NonstrokeColorSpace", "DeviceGray.Instance", "Color space for nonstroking brushes.")]
    [MacroItem("DeviceColor", "RawStrokeColor", "DeviceColor.Black", "Color for stroking brushes.")]
    [MacroItem("DeviceColor", "RawNonstrokeColor", "DeviceColor.Black", "Color for nonstroking brushes")]

    // Text Properties
    [MacroItem("double", "CharacterSpacing", "0.0", "Space to add between characters")]
    [MacroItem("double", "WordSpacing", "0.0", "Additional space to add to a ' ' (space or 0x20) character")]
    [MacroItem("double", "TextLeading", "0.0", "The space between two lines of text in local coordinate units.")]
    [MacroItem("double", "TextRise", "0.0", "The vertical offset of the next character above the text baseline.")]
    [MacroItem("double", "HorizontalTextScale", "1.0",
        "Factor by which text should be stretched or compressed horizontally.")]
    [MacroItem("TextRendering", "TextRender", "TextRendering.Fill",
        "Describes whether text should be stroked, filled, and or added to the current clipping region.")]
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
        private void InnerCopyFrom(GraphicsState other)
        {
        """, Postfix = "}")]


#warning copyFrom needs to clone the native brushes.
    [FromConstructor]public INativeBrush StrokeBrush { get; }
    [FromConstructor]public INativeBrush NonstrokeBrush { get; }

    public void CopyFrom(GraphicsState other)
    {
        InnerCopyFrom(other);
        StrokeBrush.Clone(other.StrokeBrush);
        NonstrokeBrush.Clone(other.NonstrokeBrush);
    }


    /// <summary>
    /// Color for lines and other strokes
    /// </summary>
    public DeviceColor StrokeColor => RawStrokeColor;
    /// <summary>
    /// Fill color
    /// </summary>
    public DeviceColor NonstrokeColor => RawNonstrokeColor;

    /// <inheritdoc />
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
    private async ValueTask SetLineDashPatternAsync(PdfArray entryValue)
    {
        DashPhase = await entryValue.GetAsync<double>(1).CA();
        var pattern = await entryValue.GetAsync<PdfArray>(0).CA();
        DashArray = await pattern.CastAsync<double>().CA();
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
    public async ValueTask LoadGraphicStateDictionaryAsync(PdfDictionary dictionary)
    {
        foreach (var entry in dictionary.RawItems) await ApplySingleKeyAsync(entry).CA();
    }

    private async ValueTask ApplySingleKeyAsync(KeyValuePair<PdfDirectObject, PdfIndirectObject> entry)
    {
        switch (entry.Key)
        {
            case var x when x.Equals(KnownNames.LW):
                SetLineWidth(await EntryValueAsync<double>(entry).CA());
                break;
            case var x when x.Equals(KnownNames.FL):
                SetFlatnessTolerance(await EntryValueAsync<double>(entry).CA());
                break;
            case var x when x.Equals(KnownNames.LC):
                SetLineCap((LineCap)await EntryValueAsync<long>(entry).CA());
                break;
            case var x when x.Equals(KnownNames.LJ):
                SetLineJoinStyle((LineJoinStyle)await EntryValueAsync<long>(entry).CA());
                break;
            case var x when x.Equals(KnownNames.ML):
                SetMiterLimit(await EntryValueAsync<double>(entry).CA());
                break;
            case var x when x.Equals(KnownNames.D):
                await SetLineDashPatternAsync(await EntryValueAsync<PdfArray>(entry).CA()).CA();
                break;
            case var x when x.Equals(KnownNames.RI):
                SetRenderIntent(new RenderIntentName(await entry.Value.LoadValueAsync().CA()));
                break;
            case var x when x.Equals(KnownNames.CA):
                StrokeBrush.SetAlpha(await EntryValueAsync<double>(entry).CA());
                break;
            case var x when x.Equals(KnownNames.ca):
                NonstrokeBrush.SetAlpha(await EntryValueAsync<double>(entry).CA());
                break;
        }
    }

    private static async ValueTask<T> EntryValueAsync<T>(KeyValuePair<PdfDirectObject, PdfIndirectObject> entry)
    {
        return (await entry.Value.LoadValueAsync().CA()).Get<T>();
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
    public ValueTask SetFontAsync(PdfDirectObject font, double size)
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
        RawStrokeColor = color;
        StrokeBrush.SetSolidColor(color);
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
        RawNonstrokeColor = color;
        NonstrokeBrush.SetSolidColor(color);
    }
    #endregion

    /// <summary>
    /// Set the current font.
    /// </summary>
    /// <param name="realizedFont">The font to use.</param>
    public void SetTypeface(IRealizedFont realizedFont) => 
        Typeface = realizedFont;

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
    /// Designate the current transform matrix as the initial matrix for the page.
    /// </summary>
    public void StoreInitialTransform()
    {
        InitialTransformMatrix = TransformMatrix;
    }
}