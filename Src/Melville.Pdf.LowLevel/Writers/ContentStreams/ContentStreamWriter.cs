using System;
using System.IO;
using System.IO.Pipelines;
using System.Numerics;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Pdf.LowLevel.Encryption.SecurityHandlers;
using Melville.Pdf.LowLevel.Filters.FilterProcessing;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Wrappers.ContentValueStreamUnions;

namespace Melville.Pdf.LowLevel.Writers.ContentStreams;


public partial class ContentStreamWriter : IContentStreamOperations
{
    private readonly ContentStreamPipeWriter destPipe;

    public ContentStreamWriter(PipeWriter destPipe)
    {
        this.destPipe = new ContentStreamPipeWriter(destPipe);
    }

    #region Graphic State Operations
    [MacroItem("w","LineWidth")]
    [MacroItem("M","MiterLimit")]
    [MacroItem("i","FlatnessTolerance")]
    [MacroItem("Tc","CharSpace")]
    [MacroItem("Tw","WordSpace")]
    [MacroItem("Tz","HorizontalTextScaling")]
    [MacroItem("TL","TextLeading")]
    [MacroItem("Ts","TextRise")]
    [MacroCode("public void Set~1~(double value) => destPipe.WriteOperator(ContentStreamOperatorNames.~0~, value);")]
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

    public void SetLineCap(LineCap cap) =>
        destPipe.WriteOperator(ContentStreamOperatorNames.J, (double)cap);

    public void SetLineJoinStyle(LineJoinStyle lineJoinStyle) =>
        destPipe.WriteOperator(ContentStreamOperatorNames.j, (double)lineJoinStyle);
    
    public void SetLineDashPattern(double dashPhase, in ReadOnlySpan<double> dashArray)
    {
        destPipe.WriteDoubleArray(dashArray);
        destPipe.WriteOperator(ContentStreamOperatorNames.d, dashPhase);
    }

    public void SetRenderIntent(RenderIntentName intent) =>
        destPipe.WriteOperator(ContentStreamOperatorNames.ri, intent);

    public ValueTask LoadGraphicStateDictionary(PdfName dictionaryName)
    {
        destPipe.WriteOperator(ContentStreamOperatorNames.gs, dictionaryName);
        return ValueTask.CompletedTask;
    }

    public ValueTask SetFont(PdfName font, double size)
    {
        destPipe.WriteName(font);
        destPipe.WriteOperator(ContentStreamOperatorNames.Tf, size);
        return ValueTask.CompletedTask;
    }

    public void SetTextRender(TextRendering rendering) =>
        destPipe.WriteOperator(ContentStreamOperatorNames.Tr, (double)rendering);
    #endregion

    #region Drawing Operations

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
    [MacroCode("public void ~1~() => destPipe.WriteOperator(ContentStreamOperatorNames.~0~);")]

    public void MoveTo(double x, double y) => 
        destPipe.WriteOperator(ContentStreamOperatorNames.m, x, y);

    public void LineTo(double x, double y) => destPipe.WriteOperator(ContentStreamOperatorNames.l, x,y);

    public void CurveTo(
        double control1X, double control1Y,
        double control2X, double control2Y,
        double finalX, double finalY) => destPipe.WriteOperator(ContentStreamOperatorNames.c, 
        control1X, control1Y, control2X, control2Y, finalX, finalY);

    public void CurveToWithoutInitialControl(
        double control2X, double control2Y,
        double finalX, double finalY)=> destPipe.WriteOperator(ContentStreamOperatorNames.v, 
        control2X, control2Y, finalX, finalY);

    public void CurveToWithoutFinalControl(
        double control1X, double control1Y,
        double finalX, double finalY)=> destPipe.WriteOperator(ContentStreamOperatorNames.y, 
        control1X, control1Y, finalX, finalY);

    public void Rectangle(double x, double y, double width, double height) =>
        destPipe.WriteOperator(ContentStreamOperatorNames.re, x, y, width, height);

    #endregion

    #region Color Operations

    public ValueTask SetStrokingColorSpace(PdfName colorSpace)
    {
        destPipe.WriteOperator(ContentStreamOperatorNames.CS, colorSpace);
        return ValueTask.CompletedTask;
    }

    public ValueTask SetNonstrokingColorSpace(PdfName colorSpace)
    {
        destPipe.WriteOperator(ContentStreamOperatorNames.cs, colorSpace);
        return ValueTask.CompletedTask;
    }

    public void SetStrokeColor(in ReadOnlySpan<double> components) => 
        destPipe.WriteOperator(ContentStreamOperatorNames.SC, components);

    public ValueTask SetStrokeColorExtended(PdfName? patternName, in ReadOnlySpan<double> colors)
    {
        destPipe.WriteDoubleSpan(colors);
        if (patternName is not null) destPipe.WriteName(patternName);
        destPipe.WriteOperator(ContentStreamOperatorNames.SCN);
        return ValueTask.CompletedTask;
    }

    public void SetNonstrokingColor(in ReadOnlySpan<double> components) => 
        destPipe.WriteOperator(ContentStreamOperatorNames.sc, components);

    public ValueTask SetNonstrokingColorExtended(PdfName? patternName, in ReadOnlySpan<double> colors)
    {
        destPipe.WriteDoubleSpan(colors);
        if (patternName is not null) destPipe.WriteName(patternName);
        destPipe.WriteOperator(ContentStreamOperatorNames.scn);
        return ValueTask.CompletedTask;
    }

    public ValueTask SetStrokeGray(double grayLevel)
    {
        destPipe.WriteOperator(ContentStreamOperatorNames.G, grayLevel);
        return ValueTask.CompletedTask;
    }

    public ValueTask SetStrokeRGB(double red, double green, double blue)
    {
        destPipe.WriteOperator(ContentStreamOperatorNames.RG, red, green, blue);
        return ValueTask.CompletedTask;
    }

    public ValueTask SetStrokeCMYK(double cyan, double magenta, double yellow, double black)
    {
        destPipe.WriteOperator(ContentStreamOperatorNames.K, cyan, magenta, yellow, black);
        return ValueTask.CompletedTask;
    }

    public ValueTask SetNonstrokingGray(double grayLevel)
    {
        destPipe.WriteOperator(ContentStreamOperatorNames.g, grayLevel);
        return ValueTask.CompletedTask;
    }

    public ValueTask SetNonstrokingRGB(double red, double green, double blue)
    {
        destPipe.WriteOperator(ContentStreamOperatorNames.rg, red, green, blue);
        return ValueTask.CompletedTask;
    }

    public ValueTask SetNonstrokingCMYK(double cyan, double magenta, double yellow, double black)
    {
        destPipe.WriteOperator(ContentStreamOperatorNames.k, cyan, magenta, yellow, black);
        return ValueTask.CompletedTask;
    }

    public ValueTask DoAsync(PdfName name)
    {
        destPipe.WriteOperator(ContentStreamOperatorNames.Do, name);
        return ValueTask.CompletedTask;
    }

    public async ValueTask DoAsync(PdfStream inlineImage)
    {
        await destPipe.WriteInlineImageDict(inlineImage).ConfigureAwait(false);
        await using (var str = await DiskRepresentation(inlineImage).ConfigureAwait(false))
        {
            await destPipe.WriteStreamContent(str).ConfigureAwait(false);
        }
        destPipe.WriteBytes(inlineImageTerminator);
    }

    private static ValueTask<Stream> DiskRepresentation(PdfStream inlineImage) => 
        inlineImage.StreamContentAsync(StreamFormat.DiskRepresentation, NullSecurityHandler.Instance);

    private static readonly byte[] inlineImageTerminator = { (byte)'E', (byte)'I' };  
    #endregion

    #region Text Block

    void ITextBlockOperations.BeginTextObject() => destPipe.WriteOperator(ContentStreamOperatorNames.BT);

    void ITextBlockOperations.EndTextObject() => destPipe.WriteOperator(ContentStreamOperatorNames.ET);

    public TextBlock StartTextBlock() => new(this);

    public readonly partial struct TextBlock: IDisposable, ITextObjectOperations
    {
        private readonly ContentStreamWriter parent;

        [DelegateTo()]
        private ITextObjectOperations Inner() => parent;

        public TextBlock(ContentStreamWriter parent)
        {
            this.parent = parent;
            ((ITextBlockOperations)parent).BeginTextObject();
        }
        
        public void ShowString(string decodedString) =>
            Inner().ShowString(decodedString.AsExtendedAsciiBytes()); // eventually handle encodings here.
        public void MoveToNextLineAndShowString(string decodedString) =>
            Inner().MoveToNextLineAndShowString(decodedString.AsExtendedAsciiBytes()); // eventually handle encodings here.
        public void MoveToNextLineAndShowString(double wordSpace, double charSpace, string decodedString) =>
            Inner().MoveToNextLineAndShowString(
                wordSpace, charSpace, decodedString.AsExtendedAsciiBytes()); // eventually handle encodings here.

        public void ShowSpacedString(params object[] values)
        {
            parent.destPipe.WriteChar('[');
            foreach (var value in values)
            {
                switch (value)
                {
                    case double d: parent.destPipe.WriteDoubleAndSpace(d); break;
                    case int d: parent.destPipe.WriteDoubleAndSpace(d); break;
                    case uint d: parent.destPipe.WriteDoubleAndSpace(d); break;
                    case long d: parent.destPipe.WriteDoubleAndSpace(d); break;
                    case ulong d: parent.destPipe.WriteDoubleAndSpace(d); break;
                    case short d: parent.destPipe.WriteDoubleAndSpace(d); break;
                    case ushort d: parent.destPipe.WriteDoubleAndSpace(d); break;
                    case byte [] d: parent.destPipe.WriteString(d); break;
                    default: parent.destPipe.WriteString(
                        (value.ToString()??"").AsExtendedAsciiBytes()); break;
                }
            }
            parent.destPipe.WriteChar(']');
            parent.destPipe.WriteOperator(ContentStreamOperatorNames.TJ);
        }
        
        public ValueTask ShowSpacedString(in Span<ContentStreamValueUnion> values) => 
            ((ITextObjectOperations)parent).ShowSpacedString(values);

        public void Dispose() => ((ITextBlockOperations)parent).EndTextObject();
    }

    void ITextObjectOperations.MovePositionBy(double x, double y) =>
        destPipe.WriteOperator(ContentStreamOperatorNames.Td, x, y);
    void ITextObjectOperations.MovePositionByWithLeading(double x, double y) =>
        destPipe.WriteOperator(ContentStreamOperatorNames.TD, x, y);

    void ITextObjectOperations.SetTextMatrix(
        double a, double b, double c, double d, double e, double f) =>
        destPipe.WriteOperator(ContentStreamOperatorNames.Tm, a, b, c, d, e, f);

    void ITextObjectOperations.MoveToNextTextLine() =>
        destPipe.WriteOperator(ContentStreamOperatorNames.TStar);

    ValueTask ITextObjectOperations.ShowString(ReadOnlyMemory<byte> decodedString)
    {
        destPipe.WriteOperator(ContentStreamOperatorNames.Tj, decodedString.Span);
        return new();
    }

    ValueTask ITextObjectOperations.MoveToNextLineAndShowString(ReadOnlyMemory<byte> decodedString)
    {
        destPipe.WriteOperator(ContentStreamOperatorNames.SingleQuote, decodedString.Span);
        return new();
    }

    ValueTask ITextObjectOperations.MoveToNextLineAndShowString(
        double wordSpace, double charSpace, ReadOnlyMemory<byte> decodedString)
    {
        destPipe.WriteDoubleAndSpace(wordSpace);
        destPipe.WriteDouble(charSpace);
        destPipe.WriteOperator(ContentStreamOperatorNames.DoubleQuote, decodedString.Span);
        return new();
    }

    ValueTask ITextObjectOperations.ShowSpacedString(in Span<ContentStreamValueUnion> values)
    {
        destPipe.WriteChar('[');
        foreach (var value in values)
        {
            switch (value.Type)
            {
                case ContentStreamValueType.Number:
                    destPipe.WriteDoubleAndSpace(value.Floating);
                    break;
                case ContentStreamValueType.Memory:
                    destPipe.WriteString(value.Bytes.Span);
                    break;
            }
        }
        destPipe.WriteChar(']');
        destPipe.WriteOperator(ContentStreamOperatorNames.TJ);
        return ValueTask.CompletedTask;
    }

    #endregion

    #region Marked Content Operations

    public void MarkedContentPoint(PdfName tag) => destPipe.WriteOperator(ContentStreamOperatorNames.MP, tag);

    public ValueTask MarkedContentPointAsync(PdfName tag, PdfName properties)
    {
        destPipe.WriteOperator(ContentStreamOperatorNames.DP, tag, properties);
        return ValueTask.CompletedTask;
    }

    public async ValueTask MarkedContentPointAsync(PdfName tag, PdfDictionary dict)
    {
        destPipe.WriteName(tag);
        await destPipe.WriteDictionary(dict).ConfigureAwait(false);
        destPipe.WriteOperator(ContentStreamOperatorNames.DP);
    }

    public DeferedClosingTask BeginMarkedRange(PdfName tag)
    {
        ((IMarkedContentCSOperations)this).BeginMarkedRange(tag);
        return new DeferedClosingTask(destPipe, ContentStreamOperatorNames.EMC);
    }

    public async ValueTask<DeferedClosingTask> BeginMarkedRange(PdfName tag, PdfName dictName)
    {
        await ((IMarkedContentCSOperations)this).BeginMarkedRangeAsync(tag, dictName).ConfigureAwait(false);
        return new DeferedClosingTask(destPipe, ContentStreamOperatorNames.EMC);
    }
    public async ValueTask<DeferedClosingTask> BeginMarkedRange(PdfName tag, PdfDictionary dictionary)
    {
        await ((IMarkedContentCSOperations)this).BeginMarkedRangeAsync(tag, dictionary).ConfigureAwait(false);
        return new DeferedClosingTask(destPipe, ContentStreamOperatorNames.EMC);
    }

    void IMarkedContentCSOperations.BeginMarkedRange(PdfName tag) => 
        destPipe.WriteOperator(ContentStreamOperatorNames.BMC, tag);

    ValueTask IMarkedContentCSOperations.BeginMarkedRangeAsync(PdfName tag, PdfName dictName)
    {
        destPipe.WriteOperator(ContentStreamOperatorNames.BDC, tag, dictName);
        return ValueTask.CompletedTask;
    }

    async ValueTask IMarkedContentCSOperations.BeginMarkedRangeAsync(PdfName tag, PdfDictionary dictionary)
    {
        destPipe.WriteName(tag);
        await destPipe.WriteDictionary(dictionary).ConfigureAwait(false);
        destPipe.WriteOperator(ContentStreamOperatorNames.BDC);
    }

    void IMarkedContentCSOperations.EndMarkedRange() => 
        destPipe.WriteOperator(ContentStreamOperatorNames.EMC);

    public struct DeferedClosingTask: IDisposable
    {
        private readonly ContentStreamPipeWriter writer;
        private readonly byte[] closingOperator;

        public DeferedClosingTask(ContentStreamPipeWriter writer, byte[] closingOperator)
        {
            this.writer = writer;
            this.closingOperator = closingOperator;
        }

        public void Dispose() => writer.WriteOperator(closingOperator);
    }
    #endregion

    #region Compatibility Regions

    public DeferedClosingTask BeginCompatibilitySection()
    {
        ((ICompatibilityOperations)this).BeginCompatibilitySection();
        return new DeferedClosingTask(destPipe, ContentStreamOperatorNames.EX);
    }
    void ICompatibilityOperations.BeginCompatibilitySection() =>
        destPipe.WriteOperator(ContentStreamOperatorNames.BX);

    void ICompatibilityOperations.EndCompatibilitySection() => 
        destPipe.WriteOperator(ContentStreamOperatorNames.EX);

    #endregion

    #region Type 3 Font Glyph Metrics

    public void SetColoredGlyphMetrics(double wX, double wY) => 
        destPipe.WriteOperator(ContentStreamOperatorNames.d0, wX, wY);

    public void SetUncoloredGlyphMetrics(double wX, double wY, double llX, double llY, double urX, double urY)
    {
        destPipe.WriteOperator(ContentStreamOperatorNames.d1, wX, wY, llX, llY, urX, urY);
    }

    #endregion

    public void WriteLiteral(string contentStream) => destPipe.WriteLiteral(contentStream);
}