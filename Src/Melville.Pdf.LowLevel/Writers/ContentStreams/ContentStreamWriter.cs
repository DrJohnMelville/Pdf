using System;
using System.IO;
using System.IO.Pipelines;
using System.Numerics;
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
             public void Set~1~(double value) => destPipe.WriteOperator("~0~"u8, value);
             """)]
    public void ModifyTransformMatrix(in Matrix3x2 newTransform)
    {
        destPipe.WriteDoubleAndSpace(newTransform.M11);
        destPipe.WriteDoubleAndSpace(newTransform.M12);
        destPipe.WriteDoubleAndSpace(newTransform.M21);
        destPipe.WriteDoubleAndSpace(newTransform.M22);
        destPipe.WriteDoubleAndSpace(newTransform.M31);
        destPipe.WriteDoubleAndSpace(newTransform.M32);
        destPipe.WriteOperator("cm"u8);
    }

    /// <inheritdoc />
    public void SetLineCap(LineCap cap) =>
        destPipe.WriteOperator("J"u8, (double)cap);

    /// <inheritdoc />
    public void SetLineJoinStyle(LineJoinStyle lineJoinStyle) =>
        destPipe.WriteOperator("j"u8, (double)lineJoinStyle);

    /// <inheritdoc />
    public void SetLineDashPattern(double dashPhase, in ReadOnlySpan<double> dashArray)
    {
        destPipe.WriteDoubleArray(dashArray);
        destPipe.WriteOperator("d"u8, dashPhase);
    }

    /// <inheritdoc />
    public void SetRenderIntent(RenderIntentName intent) =>
        destPipe.WriteOperator("ri"u8, intent);

    /// <inheritdoc />
    public ValueTask LoadGraphicStateDictionaryAsync(PdfDirectObject dictionaryName)
    {
        destPipe.WriteOperator("gs"u8, dictionaryName);
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public ValueTask SetFontAsync(PdfDirectObject font, double size)
    {
        destPipe.WriteName(font);
        destPipe.WriteOperator("Tf"u8, size);
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public void SetTextRender(TextRendering rendering) =>
        destPipe.WriteOperator("Tr"u8, (double)rendering);
    #endregion

    #region Drawing Operations

    /// <inheritdoc />
    [MacroItem("q", "SaveGraphicsState")]
    [MacroItem("Q", "RestoreGraphicsState")]
    [MacroItem("h", "ClosePath")]
    [MacroItem("S", "StrokePath")]
    [MacroItem("s", "CloseAndStrokePath")]
    [MacroItem("f", "FillPath")]
    [MacroItem("f*", "FillPathEvenOdd")]
    [MacroItem("B", "FillAndStrokePath")]
    [MacroItem("B*", "FillAndStrokePathEvenOdd")]
    [MacroItem("b", "CloseFillAndStrokePath")]
    [MacroItem("b*", "CloseFillAndStrokePathEvenOdd")]
    [MacroItem("n","EndPathWithNoOp")]
    [MacroItem("W","ClipToPath")]
    [MacroItem("W*","ClipToPathEvenOdd")]
    [MacroItem("BT", "BeginTextObject")]
    [MacroItem("ET", "EndTextObject")]
    [MacroCode("""
            /// <inheritdoc />
            public void ~1~() => destPipe.WriteOperator("~0~"u8);
            """)]
    public void MoveTo(double x, double y) => 
        destPipe.WriteOperator("m"u8, x, y);

    /// <inheritdoc />
    public void LineTo(double x, double y) => destPipe.WriteOperator("l"u8, x,y);

    /// <inheritdoc />
    public void CurveTo(
        double control1X, double control1Y,
        double control2X, double control2Y,
        double finalX, double finalY) => destPipe.WriteOperator("c"u8, 
        control1X, control1Y, control2X, control2Y, finalX, finalY);

    /// <inheritdoc />
    public void CurveToWithoutInitialControl(
        double control2X, double control2Y,
        double finalX, double finalY)=> destPipe.WriteOperator("v"u8, 
        control2X, control2Y, finalX, finalY);

    /// <inheritdoc />
    public void CurveToWithoutFinalControl(
        double control1X, double control1Y,
        double finalX, double finalY)=> destPipe.WriteOperator("y"u8, 
        control1X, control1Y, finalX, finalY);

    /// <inheritdoc />
    public void Rectangle(double x, double y, double width, double height) =>
        destPipe.WriteOperator("re"u8, x, y, width, height);

    /// <inheritdoc />
    public ValueTask PaintShaderAsync(PdfDirectObject name)
    {
        destPipe.WriteName(name);
        destPipe.WriteOperator("sh"u8);
        return ValueTask.CompletedTask;
    }

    #endregion

    #region Color Operations

    /// <inheritdoc />
    public ValueTask SetStrokingColorSpaceAsync(PdfDirectObject colorSpace)
    {
        destPipe.WriteOperator("CS"u8, colorSpace);
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public ValueTask SetNonstrokingColorSpaceAsync(PdfDirectObject colorSpace)
    {
        destPipe.WriteOperator("cs"u8, colorSpace);
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public void SetStrokeColor(in ReadOnlySpan<double> components) => 
        destPipe.WriteOperator("SC"u8, components);

    /// <inheritdoc />
    public ValueTask SetStrokeColorExtendedAsync(PdfDirectObject? patternName, in ReadOnlySpan<double> colors)
    {
        destPipe.WriteDoubleSpan(colors);
        if (patternName.HasValue) destPipe.WriteName(patternName.Value);
        destPipe.WriteOperator("SCN"u8);
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public void SetNonstrokingColor(in ReadOnlySpan<double> components) => 
        destPipe.WriteOperator("sc"u8, components);

    /// <inheritdoc />
    public ValueTask SetNonstrokingColorExtendedAsync(PdfDirectObject? patternName, in ReadOnlySpan<double> colors)
    {
        destPipe.WriteDoubleSpan(colors);
        if (patternName.HasValue) destPipe.WriteName(patternName.Value);
        destPipe.WriteOperator("scn"u8);
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public ValueTask SetStrokeGrayAsync(double grayLevel)
    {
        destPipe.WriteOperator("G"u8, grayLevel);
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public ValueTask SetStrokeRGBAsync(double red, double green, double blue)
    {
        destPipe.WriteOperator("RG"u8, red, green, blue);
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public ValueTask SetStrokeCMYKAsync(double cyan, double magenta, double yellow, double black)
    {
        destPipe.WriteOperator("K"u8, cyan, magenta, yellow, black);
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public ValueTask SetNonstrokingGrayAsync(double grayLevel)
    {
        destPipe.WriteOperator("g"u8, grayLevel);
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public ValueTask SetNonstrokingRgbAsync(double red, double green, double blue)
    {
        destPipe.WriteOperator("rg"u8, red, green, blue);
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public ValueTask SetNonstrokingCMYKAsync(double cyan, double magenta, double yellow, double black)
    {
        destPipe.WriteOperator("k"u8, cyan, magenta, yellow, black);
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public ValueTask DoAsync(PdfDirectObject name)
    {
        destPipe.WriteOperator("Do"u8, name);
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public async ValueTask DoAsync(PdfStream inlineImage)
    {
        destPipe.WriteInlineImageDict(inlineImage);
        await using (var str = await DiskRepresentationAsync(inlineImage).CA())
        {
            await destPipe.WriteStreamContentAsync(str).CA();
        }
        destPipe.WriteBytes(InlineImageTerminator);
    }

    private static ValueTask<Stream> DiskRepresentationAsync(PdfStream inlineImage) => 
        inlineImage.StreamContentAsync(StreamFormat.DiskRepresentation, NullSecurityHandler.Instance);

    private static ReadOnlySpan<byte> InlineImageTerminator => "EI"u8;
    #endregion

    #region Text Block

    /// <summary>
    /// Creates a TextBlockWriter structure and emits the code to begin a text block
    /// </summary>
    /// <returns>A TextBlockWriter that can be used to write text in the block.</returns>
    public TextBlockWriter StartTextBlock() => new(this);

    /// <inheritdoc />
    void ITextObjectOperations.MovePositionBy(double x, double y) =>
        destPipe.WriteOperator("Td"u8, x, y);

    /// <inheritdoc />
    void ITextObjectOperations.MovePositionByWithLeading(double x, double y) =>
        destPipe.WriteOperator("TD"u8, x, y);

    /// <inheritdoc />
    void ITextObjectOperations.SetTextMatrix(
        double a, double b, double c, double d, double e, double f) =>
        destPipe.WriteOperator("Tm"u8, a, b, c, d, e, f);

    /// <inheritdoc />
    void ITextObjectOperations.MoveToNextTextLine() =>
        destPipe.WriteOperator("T*"u8);

    /// <inheritdoc />
    ValueTask ITextObjectOperations.ShowStringAsync(ReadOnlyMemory<byte> decodedString)
    {
        destPipe.WriteOperator("Tj"u8, decodedString.Span);
        return new();
    }

    /// <inheritdoc />
    ValueTask ITextObjectOperations.MoveToNextLineAndShowStringAsync(ReadOnlyMemory<byte> decodedString)
    {
        destPipe.WriteOperator("\'"u8, decodedString.Span);
        return new();
    }

    /// <inheritdoc />
    ValueTask ITextObjectOperations.MoveToNextLineAndShowStringAsync(
        double wordSpace, double charSpace, ReadOnlyMemory<byte> decodedString)
    {
        destPipe.WriteDoubleAndSpace(wordSpace);
        destPipe.WriteDouble(charSpace);
        destPipe.WriteOperator("\""u8, decodedString.Span);
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
        destPipe.WriteOperator("TJ"u8);
        return ValueTask.CompletedTask;
    }

    #endregion

    #region Marked Content Operations

    /// <inheritdoc />
    public void MarkedContentPoint(PdfDirectObject tag) => destPipe.WriteOperator("MP"u8, tag);

    /// <inheritdoc />
    public ValueTask MarkedContentPointAsync(PdfDirectObject tag, PdfDirectObject properties)
    {
        destPipe.WriteOperator("DP"u8, tag, properties);
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public ValueTask MarkedContentPointAsync(PdfDirectObject tag, PdfDictionary dict)
    {
        destPipe.WriteName(tag);
        destPipe.WriteDictionary(dict);
        destPipe.WriteOperator("DP"u8);
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Begins a marked range with a given name.
    /// </summary>
    /// <param name="tag">The name for the marked range</param>
    /// <returns>A disposable struct that will close the marked range when disposed.</returns>
    public DeferedClosingTask BeginMarkedRange(PdfDirectObject tag)
    {
        ((IMarkedContentCSOperations)this).BeginMarkedRange(tag);
        return new DeferedClosingTask(destPipe, "EMC"u8);
    }

    /// <summary>
    /// Begins a marked range with a given name and associated dictionary.
    /// </summary>
    /// <param name="tag">The name for the marked range</param>
    /// <param name="dictName">The name that refers to a dictionary that describes the range</param>
    /// <returns>A disposable struct that will close the marked range when disposed.</returns>
    public async ValueTask<DeferedClosingTask> BeginMarkedRangeAsync(PdfDirectObject tag, PdfDirectObject dictName)
    {
        await ((IMarkedContentCSOperations)this).BeginMarkedRangeAsync(tag, dictName).CA();
        return new DeferedClosingTask(destPipe, "EMC"u8);
    }

    /// <summary>
    /// Begins a marked range with a given name and associated dictionary.
    /// </summary>
    /// <param name="tag">The name for the marked range</param>
    /// <param name="dictionary">The dictionary that describes the range</param>
    /// <returns>A disposable struct that will close the marked range when disposed.</returns>
    public async ValueTask<DeferedClosingTask> BeginMarkedRangeAsync(PdfDirectObject tag, PdfDictionary dictionary)
    {
        await ((IMarkedContentCSOperations)this).BeginMarkedRangeAsync(tag, dictionary).CA();
        return new DeferedClosingTask(destPipe, "EMC"u8);
    }

    /// <inheritdoc />
    void IMarkedContentCSOperations.BeginMarkedRange(PdfDirectObject tag) => 
        destPipe.WriteOperator("BMC"u8, tag);

    /// <inheritdoc />
    ValueTask IMarkedContentCSOperations.BeginMarkedRangeAsync(PdfDirectObject tag, PdfDirectObject dictName)
    {
        destPipe.WriteOperator("BDC"u8, tag, dictName);
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    ValueTask IMarkedContentCSOperations.BeginMarkedRangeAsync(PdfDirectObject tag, PdfDictionary dictionary)
    {
        destPipe.WriteName(tag);
        destPipe.WriteDictionary(dictionary);
        destPipe.WriteOperator("BDC"u8);
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    void IMarkedContentCSOperations.EndMarkedRange() => 
        destPipe.WriteOperator("EMC"u8);

    #endregion

    #region Compatibility Regions

    /// <summary>
    /// Begin a compatibility section
    /// </summary>
    /// <returns>An structure that will close the compatibility section upon disposal.</returns>
    public DeferedClosingTask BeginCompatibilitySection()
    {
        ((ICompatibilityOperations)this).BeginCompatibilitySection();
        return new DeferedClosingTask(destPipe, "EX"u8);
    }

    /// <inheritdoc />
    void ICompatibilityOperations.BeginCompatibilitySection() =>
        destPipe.WriteOperator("BX"u8);

    /// <inheritdoc />
    void ICompatibilityOperations.EndCompatibilitySection() => 
        destPipe.WriteOperator("EX"u8);

    #endregion

    #region Type 3 Font Glyph Metrics

    /// <inheritdoc />
    public void SetColoredGlyphMetrics(double wX, double wY) => 
        destPipe.WriteOperator("d0"u8, wX, wY);

    /// <inheritdoc />
    public void SetUncoloredGlyphMetrics(double wX, double wY, double llX, double llY, double urX, double urY) => 
        destPipe.WriteOperator("d1"u8, wX, wY, llX, llY, urX, urY);

    #endregion
    /// <summary>
    /// Write unencoded content into the content stream.  The text provided must be valid content stream syntax.
    /// </summary>
    /// <param name="content">The string that, interpreted as ASCII bytes, will be added to the content stream.</param>
    public void WriteLiteral(string content) => destPipe.WriteLiteral(content);
}