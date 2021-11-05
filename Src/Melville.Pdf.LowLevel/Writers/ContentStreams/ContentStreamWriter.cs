using System;
using System.IO.Pipelines;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;

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
    public void ModifyTransformMatrix(double a, double b, double c, double d, double e, double f) =>
        destPipe.WriteOperator(ContentStreamOperatorNames.cm, a, b, c, d, e, f);
    
    public void SetLineCap(LineCap cap) =>
        destPipe.WriteOperator(ContentStreamOperatorNames.J, (double)cap);

    public void SetLineJoinStyle(LineJoinStyle cap) =>
        destPipe.WriteOperator(ContentStreamOperatorNames.j, (double)cap);
    
    public void SetLineDashPattern(double dashPhase, in ReadOnlySpan<double> dashArray)
    {
        destPipe.WriteDoubleArray(dashArray);
        destPipe.WriteOperator(ContentStreamOperatorNames.d, dashPhase);
    }

    public void SetRenderIntent(RenderingIntentName intent) =>
        destPipe.WriteOperator(ContentStreamOperatorNames.ri, intent);

    public void LoadGraphicStateDictionary(PdfName dictionaryName) =>
        destPipe.WriteOperator(ContentStreamOperatorNames.gs, dictionaryName);

    public void SetFont(PdfName font, double size)
    {
        destPipe.WriteName(font);
        destPipe.WriteOperator(ContentStreamOperatorNames.Tf, size);
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

    public void SetStrokingColorSpace(PdfName colorSpace) => 
        destPipe.WriteOperator(ContentStreamOperatorNames.CS, colorSpace);

    public void SetNonstrokingColorSpace(PdfName colorSpace) => 
        destPipe.WriteOperator(ContentStreamOperatorNames.cs, colorSpace);
    
    public void SetStrokeColor(in ReadOnlySpan<double> components) => 
        destPipe.WriteOperator(ContentStreamOperatorNames.SC, components);

    public void SetStrokeColorExtended(PdfName? patternName, in ReadOnlySpan<double> colors)
    {
        destPipe.WriteDoubleSpan(colors);
        if (patternName is not null) destPipe.WriteName(patternName);
        destPipe.WriteOperator(ContentStreamOperatorNames.SCN);
    }

    public void SetNonstrokingColor(in ReadOnlySpan<double> components) => 
        destPipe.WriteOperator(ContentStreamOperatorNames.sc, components);

    public void SetNonstrokingColorExtended(PdfName? patternName, in ReadOnlySpan<double> colors)
    {
        destPipe.WriteDoubleSpan(colors);
        if (patternName is not null) destPipe.WriteName(patternName);
        destPipe.WriteOperator(ContentStreamOperatorNames.scn);
    }

    public void SetStrokeGray(double grayLevel) =>
        destPipe.WriteOperator(ContentStreamOperatorNames.G, grayLevel);

    public void SetStrokeRGB(double red, double green, double blue) => 
        destPipe.WriteOperator(ContentStreamOperatorNames.RG, red, green, blue);

    public void SetStrokeCMYK(double cyan, double magenta, double yellow, double black) => 
        destPipe.WriteOperator(ContentStreamOperatorNames.K, cyan, magenta, yellow, black);
    
    public void SetNonstrokingGray(double grayLevel) =>
        destPipe.WriteOperator(ContentStreamOperatorNames.g, grayLevel);

    public void SetNonstrokingRGB(double red, double green, double blue) => 
        destPipe.WriteOperator(ContentStreamOperatorNames.rg, red, green, blue);

    public void SetNonstrokingCMYK(double cyan, double magenta, double yellow, double black) => 
        destPipe.WriteOperator(ContentStreamOperatorNames.k, cyan, magenta, yellow, black);

    public void Do(PdfName name) => destPipe.WriteOperator(
        ContentStreamOperatorNames.Do, name);
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

    void ITextObjectOperations.ShowString(in ReadOnlyMemory<byte> decodedString) =>
        destPipe.WriteOperator(ContentStreamOperatorNames.Tj, decodedString.Span);
    void ITextObjectOperations.MoveToNextLineAndShowString(in ReadOnlyMemory<byte> decodedString) =>
        destPipe.WriteOperator(ContentStreamOperatorNames.SingleQuote, decodedString.Span);

    void ITextObjectOperations.MoveToNextLineAndShowString(
        double wordSpace, double charSpace, in ReadOnlyMemory<byte> decodedString)
    {
        destPipe.WriteDoubleAndSpace(wordSpace);
        destPipe.WriteDouble(charSpace);
        destPipe.WriteOperator(ContentStreamOperatorNames.DoubleQuote, decodedString.Span);

    }

    void ITextObjectOperations.ShowSpacedString(in InterleavedArray<Memory<byte>, double> values)
    {
        destPipe.WriteChar('[');
        values.Iterate(new ShowSpacedStringIterator(this));
        destPipe.WriteChar(']');
        destPipe.WriteOperator(ContentStreamOperatorNames.TJ);
    }
    
    private readonly struct ShowSpacedStringIterator: IInterleavedTarget<Memory<byte>, double>
    {
        private readonly ContentStreamWriter writer;
        public ShowSpacedStringIterator(ContentStreamWriter writer)
        {
            this.writer = writer;
        }
        public void Handle(Memory<byte> item) => writer.destPipe.WriteString(item.Span);
        public void Handle(double item) => writer.destPipe.WriteDoubleAndSpace(item);
    }
    

    #endregion

    #region Marked Content Operations

    public void MarkedContentPoint(PdfName tag) => destPipe.WriteOperator(ContentStreamOperatorNames.MP, tag);

    public void MarkedContentPoint(PdfName tag, PdfName properties) =>
        destPipe.WriteOperator(ContentStreamOperatorNames.DP, tag, properties);

    #endregion
}