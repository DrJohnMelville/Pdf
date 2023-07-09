using System;
using System.IO;
using System.IO.Pipelines;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Encryption.SecurityHandlers;
using Melville.Pdf.LowLevel.Filters.FilterProcessing;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Writers.ContentStreams;

/// <summary>
///  When a series of drawing operations are called on this class, the
/// class writes the operations out to the destination pipe as a
/// content stream.
/// </summary>
public partial class ContentStreamWriter : IContentStreamOperations, ISpacedStringBuilder
{
    private readonly ContentStreamPipeWriter destPipe;

    /// <summary>
    /// Create the ContentStreamWriter
    /// </summary>
    /// <param name="destPipe">Pipe that is to receive the content stream</param>
    public ContentStreamWriter(PipeWriter destPipe)
    {
        this.destPipe = new ContentStreamPipeWriter(destPipe);
    }

    #region Graphic State Operations

    /// <inheritdoc />
    [MacroItem("w","LineWidth")]
    [MacroItem("M","MiterLimit")]
    [MacroItem("i","FlatnessTolerance")]
    [MacroItem("Tc","CharSpace")]
    [MacroItem("Tw","WordSpace")]
    [MacroItem("Tz","HorizontalTextScaling")]
    [MacroItem("TL","TextLeading")]
    [MacroItem("Ts","TextRise")]
    [MacroCode("""
             /// <inheritdoc />
             public void Set~1~(double value) => destPipe.WriteOperator(ContentStreamOperatorNames.~0~, value);
             """)]
    public void ModifyTransformMatrix(in Matrix3x2 newTransform)
    {
        destPipe.WriteDoubleAndSpace(newTransform.M11);
        destPipe.WriteDoubleAndSpace(newTransform.M12);
        destPipe.WriteDoubleAndSpace(newTransform.M21);
        destPipe.WriteDoubleAndSpace(newTransform.M22);
        destPipe.WriteDoubleAndSpace(newTransform.M31);
        destPipe.WriteDoubleAndSpace(newTransform.M32);
        destPipe.WriteOperator(ContentStreamOperatorNames.cm);
    }

    /// <inheritdoc />
    public void SetLineCap(LineCap cap) =>
        destPipe.WriteOperator(ContentStreamOperatorNames.J, (double)cap);

    /// <inheritdoc />
    public void SetLineJoinStyle(LineJoinStyle lineJoinStyle) =>
        destPipe.WriteOperator(ContentStreamOperatorNames.j, (double)lineJoinStyle);

    /// <inheritdoc />
    public void SetLineDashPattern(double dashPhase, in ReadOnlySpan<double> dashArray)
    {
        destPipe.WriteDoubleArray(dashArray);
        destPipe.WriteOperator(ContentStreamOperatorNames.d, dashPhase);
    }

    /// <inheritdoc />
    public void SetRenderIntent(RenderIntentName intent) =>
        destPipe.WriteOperator(ContentStreamOperatorNames.ri, intent);

    /// <inheritdoc />
    public ValueTask LoadGraphicStateDictionaryAsync(PdfName dictionaryName)
    {
        destPipe.WriteOperator(ContentStreamOperatorNames.gs, dictionaryName);
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public ValueTask SetFontAsync(PdfName font, double size)
    {
        destPipe.WriteName(font);
        destPipe.WriteOperator(ContentStreamOperatorNames.Tf, size);
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public void SetTextRender(TextRendering rendering) =>
        destPipe.WriteOperator(ContentStreamOperatorNames.Tr, (double)rendering);
    #endregion

    #region Drawing Operations

    /// <inheritdoc />
    [MacroItem("q", "SaveGraphicsState")]
    [MacroItem("Q", "RestoreGraphicsState")]
    [MacroItem("h", "ClosePath")]
    [MacroItem("S", "StrokePath")]
    [MacroItem("s", "CloseAndStrokePath")]
    [MacroItem("f", "FillPath")]
    [MacroItem("fStar", "FillPathEvenOdd")]
    [MacroItem("B", "FillAndStrokePath")]
    [MacroItem("BStar", "FillAndStrokePathEvenOdd")]
    [MacroItem("b", "CloseFillAndStrokePath")]
    [MacroItem("bStar", "CloseFillAndStrokePathEvenOdd")]
    [MacroItem("n","EndPathWithNoOp")]
    [MacroItem("W","ClipToPath")]
    [MacroItem("WStar","ClipToPathEvenOdd")]
    [MacroItem("BT", "BeginTextObject")]
    [MacroItem("ET", "EndTextObject")]
    [MacroCode("""
            /// <inheritdoc />
            public void ~1~() => destPipe.WriteOperator(ContentStreamOperatorNames.~0~);
            """)]
    public void MoveTo(double x, double y) => 
        destPipe.WriteOperator(ContentStreamOperatorNames.m, x, y);

    /// <inheritdoc />
    public void LineTo(double x, double y) => destPipe.WriteOperator(ContentStreamOperatorNames.l, x,y);

    /// <inheritdoc />
    public void CurveTo(
        double control1X, double control1Y,
        double control2X, double control2Y,
        double finalX, double finalY) => destPipe.WriteOperator(ContentStreamOperatorNames.c, 
        control1X, control1Y, control2X, control2Y, finalX, finalY);

    /// <inheritdoc />
    public void CurveToWithoutInitialControl(
        double control2X, double control2Y,
        double finalX, double finalY)=> destPipe.WriteOperator(ContentStreamOperatorNames.v, 
        control2X, control2Y, finalX, finalY);

    /// <inheritdoc />
    public void CurveToWithoutFinalControl(
        double control1X, double control1Y,
        double finalX, double finalY)=> destPipe.WriteOperator(ContentStreamOperatorNames.y, 
        control1X, control1Y, finalX, finalY);

    /// <inheritdoc />
    public void Rectangle(double x, double y, double width, double height) =>
        destPipe.WriteOperator(ContentStreamOperatorNames.re, x, y, width, height);

    /// <inheritdoc />
    public ValueTask PaintShaderAsync(PdfName name)
    {
        destPipe.WriteName(name);
        destPipe.WriteOperator(ContentStreamOperatorNames.sh);
        return ValueTask.CompletedTask;
    }

    #endregion

    #region Color Operations

    /// <inheritdoc />
    public ValueTask SetStrokingColorSpaceAsync(PdfName colorSpace)
    {
        destPipe.WriteOperator(ContentStreamOperatorNames.CS, colorSpace);
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public ValueTask SetNonstrokingColorSpaceAsync(PdfName colorSpace)
    {
        destPipe.WriteOperator(ContentStreamOperatorNames.cs, colorSpace);
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public void SetStrokeColor(in ReadOnlySpan<double> components) => 
        destPipe.WriteOperator(ContentStreamOperatorNames.SC, components);

    /// <inheritdoc />
    public ValueTask SetStrokeColorExtendedAsync(PdfName? patternName, in ReadOnlySpan<double> colors)
    {
        destPipe.WriteDoubleSpan(colors);
        if (patternName is not null) destPipe.WriteName(patternName);
        destPipe.WriteOperator(ContentStreamOperatorNames.SCN);
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public void SetNonstrokingColor(in ReadOnlySpan<double> components) => 
        destPipe.WriteOperator(ContentStreamOperatorNames.sc, components);

    /// <inheritdoc />
    public ValueTask SetNonstrokingColorExtendedAsync(PdfName? patternName, in ReadOnlySpan<double> colors)
    {
        destPipe.WriteDoubleSpan(colors);
        if (patternName is not null) destPipe.WriteName(patternName);
        destPipe.WriteOperator(ContentStreamOperatorNames.scn);
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public ValueTask SetStrokeGrayAsync(double grayLevel)
    {
        destPipe.WriteOperator(ContentStreamOperatorNames.G, grayLevel);
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public ValueTask SetStrokeRGBAsync(double red, double green, double blue)
    {
        destPipe.WriteOperator(ContentStreamOperatorNames.RG, red, green, blue);
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public ValueTask SetStrokeCMYKAsync(double cyan, double magenta, double yellow, double black)
    {
        destPipe.WriteOperator(ContentStreamOperatorNames.K, cyan, magenta, yellow, black);
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public ValueTask SetNonstrokingGrayAsync(double grayLevel)
    {
        destPipe.WriteOperator(ContentStreamOperatorNames.g, grayLevel);
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public ValueTask SetNonstrokingRgbAsync(double red, double green, double blue)
    {
        destPipe.WriteOperator(ContentStreamOperatorNames.rg, red, green, blue);
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public ValueTask SetNonstrokingCMYKAsync(double cyan, double magenta, double yellow, double black)
    {
        destPipe.WriteOperator(ContentStreamOperatorNames.k, cyan, magenta, yellow, black);
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public ValueTask DoAsync(PdfName name)
    {
        destPipe.WriteOperator(ContentStreamOperatorNames.Do, name);
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public async ValueTask DoAsync(PdfStream inlineImage)
    {
        await destPipe.WriteInlineImageDictAsync(inlineImage).CA();
        await using (var str = await DiskRepresentationAsync(inlineImage).CA())
        {
            await destPipe.WriteStreamContentAsync(str).CA();
        }
        destPipe.WriteBytes(inlineImageTerminator);
    }

    private static ValueTask<Stream> DiskRepresentationAsync(PdfStream inlineImage) => 
        inlineImage.StreamContentAsync(StreamFormat.DiskRepresentation, NullSecurityHandler.Instance);

    private static readonly byte[] inlineImageTerminator = { (byte)'E', (byte)'I' };  
    #endregion

    #region Text Block

    /// <summary>
    /// Creates a TextBlockWriter structure and emits the code to begin a text block
    /// </summary>
    /// <returns>A TextBlockWriter that can be used to write text in the block.</returns>
    public TextBlockWriter StartTextBlock() => new(this);

    /// <inheritdoc />
    void ITextObjectOperations.MovePositionBy(double x, double y) =>
        destPipe.WriteOperator(ContentStreamOperatorNames.Td, x, y);

    /// <inheritdoc />
    void ITextObjectOperations.MovePositionByWithLeading(double x, double y) =>
        destPipe.WriteOperator(ContentStreamOperatorNames.TD, x, y);

    /// <inheritdoc />
    void ITextObjectOperations.SetTextMatrix(
        double a, double b, double c, double d, double e, double f) =>
        destPipe.WriteOperator(ContentStreamOperatorNames.Tm, a, b, c, d, e, f);

    /// <inheritdoc />
    void ITextObjectOperations.MoveToNextTextLine() =>
        destPipe.WriteOperator(ContentStreamOperatorNames.TStar);

    /// <inheritdoc />
    ValueTask ITextObjectOperations.ShowStringAsync(ReadOnlyMemory<byte> decodedString)
    {
        destPipe.WriteOperator(ContentStreamOperatorNames.Tj, decodedString.Span);
        return new();
    }

    /// <inheritdoc />
    ValueTask ITextObjectOperations.MoveToNextLineAndShowStringAsync(ReadOnlyMemory<byte> decodedString)
    {
        destPipe.WriteOperator(ContentStreamOperatorNames.SingleQuote, decodedString.Span);
        return new();
    }

    /// <inheritdoc />
    ValueTask ITextObjectOperations.MoveToNextLineAndShowStringAsync(
        double wordSpace, double charSpace, ReadOnlyMemory<byte> decodedString)
    {
        destPipe.WriteDoubleAndSpace(wordSpace);
        destPipe.WriteDouble(charSpace);
        destPipe.WriteOperator(ContentStreamOperatorNames.DoubleQuote, decodedString.Span);
        return new();
    }

    /// <inheritdoc />
    public ISpacedStringBuilder GetSpacedStringBuilder()
    {
        destPipe.WriteChar('[');
        return this;
    }

    ValueTask ISpacedStringBuilder.SpacedStringComponentAsync(double value)
    {
        destPipe.WriteDoubleAndSpace(value);
        return ValueTask.CompletedTask;
    }

    ValueTask ISpacedStringBuilder.SpacedStringComponentAsync(Memory<byte> value)
    {
        destPipe.WriteString(value.Span);
        return ValueTask.CompletedTask;
    }

    ValueTask ISpacedStringBuilder.DoneWritingAsync()
    {
        destPipe.WriteChar(']');
        destPipe.WriteOperator(ContentStreamOperatorNames.TJ);
        return ValueTask.CompletedTask;
    }

    #endregion

    #region Marked Content Operations

    /// <inheritdoc />
    public void MarkedContentPoint(PdfName tag) => destPipe.WriteOperator(ContentStreamOperatorNames.MP, tag);

    /// <inheritdoc />
    public ValueTask MarkedContentPointAsync(PdfName tag, PdfName properties)
    {
        destPipe.WriteOperator(ContentStreamOperatorNames.DP, tag, properties);
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public async ValueTask MarkedContentPointAsync(PdfName tag, PdfDictionary dict)
    {
        destPipe.WriteName(tag);
        await destPipe.WriteDictionaryAsync(dict).CA();
        destPipe.WriteOperator(ContentStreamOperatorNames.DP);
    }

    /// <summary>
    /// Begins a marked range with a given name.
    /// </summary>
    /// <param name="tag">The name for the marked range</param>
    /// <returns>A disposable struct that will close the marked range when disposed.</returns>
    public DeferedClosingTask BeginMarkedRange(PdfName tag)
    {
        ((IMarkedContentCSOperations)this).BeginMarkedRange(tag);
        return new DeferedClosingTask(destPipe, ContentStreamOperatorNames.EMC);
    }

    /// <summary>
    /// Begins a marked range with a given name and associated dictionary.
    /// </summary>
    /// <param name="tag">The name for the marked range</param>
    /// <param name="dictName">The name that refers to a dictionary that describes the range</param>
    /// <returns>A disposable struct that will close the marked range when disposed.</returns>
    public async ValueTask<DeferedClosingTask> BeginMarkedRangeAsync(PdfName tag, PdfName dictName)
    {
        await ((IMarkedContentCSOperations)this).BeginMarkedRangeAsync(tag, dictName).CA();
        return new DeferedClosingTask(destPipe, ContentStreamOperatorNames.EMC);
    }

    /// <summary>
    /// Begins a marked range with a given name and associated dictionary.
    /// </summary>
    /// <param name="tag">The name for the marked range</param>
    /// <param name="dictionary">The dictionary that describes the range</param>
    /// <returns>A disposable struct that will close the marked range when disposed.</returns>
    public async ValueTask<DeferedClosingTask> BeginMarkedRangeAsync(PdfName tag, PdfDictionary dictionary)
    {
        await ((IMarkedContentCSOperations)this).BeginMarkedRangeAsync(tag, dictionary).CA();
        return new DeferedClosingTask(destPipe, ContentStreamOperatorNames.EMC);
    }

    /// <inheritdoc />
    void IMarkedContentCSOperations.BeginMarkedRange(PdfName tag) => 
        destPipe.WriteOperator(ContentStreamOperatorNames.BMC, tag);

    /// <inheritdoc />
    ValueTask IMarkedContentCSOperations.BeginMarkedRangeAsync(PdfName tag, PdfName dictName)
    {
        destPipe.WriteOperator(ContentStreamOperatorNames.BDC, tag, dictName);
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    async ValueTask IMarkedContentCSOperations.BeginMarkedRangeAsync(PdfName tag, PdfDictionary dictionary)
    {
        destPipe.WriteName(tag);
        await destPipe.WriteDictionaryAsync(dictionary).CA();
        destPipe.WriteOperator(ContentStreamOperatorNames.BDC);
    }

    /// <inheritdoc />
    void IMarkedContentCSOperations.EndMarkedRange() => 
        destPipe.WriteOperator(ContentStreamOperatorNames.EMC);

    #endregion

    #region Compatibility Regions

    /// <summary>
    /// Begin a compatibility section
    /// </summary>
    /// <returns>An structure that will close the compatibility section upon disposal.</returns>
    public DeferedClosingTask BeginCompatibilitySection()
    {
        ((ICompatibilityOperations)this).BeginCompatibilitySection();
        return new DeferedClosingTask(destPipe, ContentStreamOperatorNames.EX);
    }

    /// <inheritdoc />
    void ICompatibilityOperations.BeginCompatibilitySection() =>
        destPipe.WriteOperator(ContentStreamOperatorNames.BX);

    /// <inheritdoc />
    void ICompatibilityOperations.EndCompatibilitySection() => 
        destPipe.WriteOperator(ContentStreamOperatorNames.EX);

    #endregion

    #region Type 3 Font Glyph Metrics

    /// <inheritdoc />
    public void SetColoredGlyphMetrics(double wX, double wY) => 
        destPipe.WriteOperator(ContentStreamOperatorNames.d0, wX, wY);

    /// <inheritdoc />
    public void SetUncoloredGlyphMetrics(double wX, double wY, double llX, double llY, double urX, double urY) => 
        destPipe.WriteOperator(ContentStreamOperatorNames.d1, wX, wY, llX, llY, urX, urY);

    #endregion
    /// <summary>
    /// Write unencoded content into the content stream.  The text provided must be valid content stream syntax.
    /// </summary>
    /// <param name="content">The string that, interpreted as ASCII bytes, will be added to the content stream.</param>
    public void WriteLiteral(string content) => destPipe.WriteLiteral(content);
}