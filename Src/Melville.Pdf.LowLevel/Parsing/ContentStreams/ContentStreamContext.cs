using System;
using System.Numerics;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Model.Wrappers.ContentValueStreamUnions;

namespace Melville.Pdf.LowLevel.Parsing.ContentStreams;

/// <summary>
/// A PDF content stream is a stream of arguments and operators in postfix notation.
/// The purpose of this class is to convert a sequence of arguments and operators (
/// expressed as a sequence of calls to the handle... methods) into a a serries of
/// callse to the target object witih the apppopriate number and type of arguments.
/// </summary>
internal class ContentStreamContext
{
    private readonly IContentStreamOperations target;
    private readonly ContentStreamValueStack arguments = new();
    private int compatibilitySectionCount;

    public ContentStreamContext(IContentStreamOperations target)
    {
        this.target = target;
    }

    public void HandleNumber(double doubleValue, long longValue) => 
        arguments.Add(doubleValue, longValue);

    public void HandleName(PdfName name) => arguments.Add(name);
    public void HandleDictionary(PdfObject dict) => arguments.Add(dict);

    public void HandleString(in Memory<byte> str) => arguments.Add(str);

    public async ValueTask HandleOpCode(ContentStreamOperatorValue opCode)
    {
        switch (opCode)
        {
            case ContentStreamOperatorValue.b:
                target.CloseFillAndStrokePath();
                break;
            case ContentStreamOperatorValue.B:
                target.FillAndStrokePath();
                break;
            case ContentStreamOperatorValue.bStar:
                target.CloseFillAndStrokePathEvenOdd();
                break;
            case ContentStreamOperatorValue.BStar:
                target.FillAndStrokePathEvenOdd();
                break;
            case ContentStreamOperatorValue.BDC:
                await BeginMarkedRange().CA();
                break;
            case ContentStreamOperatorValue.BI:
                break;
            case ContentStreamOperatorValue.BMC:
                target.BeginMarkedRange(arguments.NamaAt(0));
                break;
            case ContentStreamOperatorValue.BT:
                target.BeginTextObject();
                break;
            case ContentStreamOperatorValue.BX:
                target.BeginCompatibilitySection();
                compatibilitySectionCount++;
                break;
            case ContentStreamOperatorValue.c:
                target.CurveTo(arguments.DoubleAt(0), arguments.DoubleAt(1), arguments.DoubleAt(2),
                    arguments.DoubleAt(3), arguments.DoubleAt(4), arguments.DoubleAt(5));
                break;
            case ContentStreamOperatorValue.cm:
                target.ModifyTransformMatrix(new Matrix3x2(
                    arguments.FloatAt(0), arguments.FloatAt(1), 
                    arguments.FloatAt(2), arguments.FloatAt(3), 
                    arguments.FloatAt(4), arguments.FloatAt(5)));
                break;
            case ContentStreamOperatorValue.CS:
                await target.SetStrokingColorSpace(arguments.NamaAt(0)).CA();
                break;
            case ContentStreamOperatorValue.cs:
                await target.SetNonstrokingColorSpace(arguments.NamaAt(0)).CA();
                break;
            case ContentStreamOperatorValue.d:
                SetLineDashPattern();
                break;
            case ContentStreamOperatorValue.d0:
                target.SetColoredGlyphMetrics(arguments.DoubleAt(0), arguments.DoubleAt(1));
                break;
            case ContentStreamOperatorValue.d1:
                target.SetUncoloredGlyphMetrics(
                    arguments.DoubleAt(0), arguments.DoubleAt(1), arguments.DoubleAt(2),
                    arguments.DoubleAt(3), arguments.DoubleAt(4), arguments.DoubleAt(5));
                break;
            case ContentStreamOperatorValue.Do:
                await target.DoAsync(arguments.NamaAt(0)).CA();
                break;
            case ContentStreamOperatorValue.DP:
                await MarkedContentPoint().CA();
                break;
            case ContentStreamOperatorValue.EI:
                break;
            case ContentStreamOperatorValue.EMC:
                target.EndMarkedRange();
                break;
            case ContentStreamOperatorValue.ET:
                target.EndTextObject(); 
                break;
            case ContentStreamOperatorValue.EX:
                target.EndCompatibilitySection();
                compatibilitySectionCount--;
                break;
            case ContentStreamOperatorValue.f:
            // pdf spec requires this synonym for backward compatability
            case ContentStreamOperatorValue.F:
                target.FillPath();
                break;
            case ContentStreamOperatorValue.fStar:
                target.FillPathEvenOdd();
                break;
            case ContentStreamOperatorValue.G:
                await target.SetStrokeGray(arguments.DoubleAt(0)).CA();
                break;
            case ContentStreamOperatorValue.g:
                await target.SetNonstrokingGray(arguments.DoubleAt(0)).CA();
                break;
            case ContentStreamOperatorValue.gs:
                await target.LoadGraphicStateDictionary(arguments.NamaAt(0)).CA();
                break;
            case ContentStreamOperatorValue.h:
                target.ClosePath();
                break;
            case ContentStreamOperatorValue.i:
                target.SetFlatnessTolerance(arguments.DoubleAt(0));
                break;
            case ContentStreamOperatorValue.ID:
                break;
            case ContentStreamOperatorValue.J:
                target.SetLineCap((LineCap)(double)arguments.LongAt(0));
                break;
            case ContentStreamOperatorValue.j:
                target.SetLineJoinStyle((LineJoinStyle)(double)arguments.LongAt(0));
                break;
            case ContentStreamOperatorValue.K:
                await target.SetStrokeCMYK(arguments.DoubleAt(0), arguments.DoubleAt(1),
                    arguments.DoubleAt(2), arguments.DoubleAt(3)).CA();
                break;
            case ContentStreamOperatorValue.k:
                await target.SetNonstrokingCMYK(arguments.DoubleAt(0), arguments.DoubleAt(1),
                    arguments.DoubleAt(2), arguments.DoubleAt(3)).CA();
                break;
            case ContentStreamOperatorValue.l:
                target.LineTo(arguments.DoubleAt(0), arguments.DoubleAt(1));
                break;
            case ContentStreamOperatorValue.m:
                target.MoveTo(arguments.DoubleAt(0), arguments.DoubleAt(1));
                break;
            case ContentStreamOperatorValue.M:
                target.SetMiterLimit(arguments.DoubleAt(0));
                break;
            case ContentStreamOperatorValue.MP:
                target.MarkedContentPoint(arguments.NamaAt(0));
                break;
            case ContentStreamOperatorValue.n:
                target.EndPathWithNoOp();
                break;
            case ContentStreamOperatorValue.q:
                target.SaveGraphicsState();
                break;
            case ContentStreamOperatorValue.Q:
                target.RestoreGraphicsState();
                break;
            case ContentStreamOperatorValue.re:
                target.Rectangle(arguments.DoubleAt(0), arguments.DoubleAt(1), arguments.DoubleAt(2),
                    arguments.DoubleAt(3));
                break;
            case ContentStreamOperatorValue.RG:
                await target.SetStrokeRGB(arguments.DoubleAt(0), arguments.DoubleAt(1), arguments.DoubleAt(2)).CA();
                break;
            case ContentStreamOperatorValue.rg:
                await target.SetNonstrokingRGB(arguments.DoubleAt(0), arguments.DoubleAt(1),
                    arguments.DoubleAt(2)).CA();
                break;
            case ContentStreamOperatorValue.ri:
                target.SetRenderIntent(new RenderIntentName(arguments.NamaAt(0)));
                break;
            case ContentStreamOperatorValue.s:
                target.CloseAndStrokePath();
                break;
            case ContentStreamOperatorValue.S:
                target.StrokePath();
                break;
            case ContentStreamOperatorValue.sc:
                SetNonstrokingColor();
                break;
            case ContentStreamOperatorValue.SC:
                SetStrokingColor();
                break;
            case ContentStreamOperatorValue.SCN:
                await SetStrokingColorExtended().CA();
                break;
            case ContentStreamOperatorValue.scn:
                await SetNonstrokingColorExtended().CA();
                break;
            case ContentStreamOperatorValue.sh:
                await target.PaintShader(arguments.NamaAt(0)).CA();
                break;
            case ContentStreamOperatorValue.TStar:
                target.MoveToNextTextLine();
                break;
            case ContentStreamOperatorValue.Tc:
                target.SetCharSpace(arguments.DoubleAt(0));
                break;
            case ContentStreamOperatorValue.Td:
                target.MovePositionBy(arguments.DoubleAt(0), arguments.DoubleAt(1));
                break;
            case ContentStreamOperatorValue.TD:
                target.MovePositionByWithLeading(arguments.DoubleAt(0), arguments.DoubleAt(1));
                break;
            case ContentStreamOperatorValue.Tf:
                await target.SetFont(arguments.NamaAt(0), arguments.DoubleAt(1)).CA();
                break;
            case ContentStreamOperatorValue.Tj:
                await target.ShowString(arguments.BytesAt(0)).CA();
                break;
            case ContentStreamOperatorValue.TJ:
                await target.ShowSpacedString(arguments.NativeSpan()).CA();
                break;
            case ContentStreamOperatorValue.TL:
                target.SetTextLeading(arguments.DoubleAt(0));
                break;
            case ContentStreamOperatorValue.Tm:
                target.SetTextMatrix(arguments.DoubleAt(0), arguments.DoubleAt(1), arguments.DoubleAt(2),
                    arguments.DoubleAt(3), arguments.DoubleAt(4), arguments.DoubleAt(5));
                break;
            case ContentStreamOperatorValue.Tr:
                target.SetTextRender((TextRendering)(double)arguments.LongAt(0));
                break;
            case ContentStreamOperatorValue.Ts:
                target.SetTextRise(arguments.DoubleAt(0));
                break;
            case ContentStreamOperatorValue.Tw:
                target.SetWordSpace(arguments.DoubleAt(0));
                break;
            case ContentStreamOperatorValue.Tz:
                target.SetHorizontalTextScaling(arguments.DoubleAt(0));
                break;
            case ContentStreamOperatorValue.v:
                target.CurveToWithoutInitialControl(arguments.DoubleAt(0), arguments.DoubleAt(1),
                    arguments.DoubleAt(2), arguments.DoubleAt(3));
                break;
            case ContentStreamOperatorValue.w:
                target.SetLineWidth(arguments.DoubleAt(0));
                break;
            case ContentStreamOperatorValue.W:
                target.ClipToPath();
                break;
            case ContentStreamOperatorValue.WStar:
                target.ClipToPathEvenOdd();
                break;
            case ContentStreamOperatorValue.y:
                target.CurveToWithoutFinalControl(arguments.DoubleAt(0), arguments.DoubleAt(1),
                    arguments.DoubleAt(2), arguments.DoubleAt(3));
                break;
            case ContentStreamOperatorValue.SingleQuote:
                await target.MoveToNextLineAndShowString(arguments.BytesAt(0)).CA();
                break;
            case ContentStreamOperatorValue.DoubleQuote:
                await target.MoveToNextLineAndShowString(arguments.DoubleAt(0), arguments.DoubleAt(1),
                    arguments.BytesAt(2)).CA();
                break;
            default:
                HandleUnknownOperation();
                break;
        }
        arguments.Clear();
    }

    private void SetLineDashPattern()
    {
        Span<double> span = stackalloc double[arguments.Count];
        arguments.FillSpan(span);
        target.SetLineDashPattern(span[^1], span[..^1]);
    }

    private ValueTask MarkedContentPoint() =>
        arguments.ObjectAt<PdfObject>(1) switch
        {
            PdfName name => target.MarkedContentPointAsync(arguments.NamaAt(0), name),
            PdfDictionary dict => target.MarkedContentPointAsync(arguments.NamaAt(0), dict),
            _ => throw new PdfParseException("Invalid MarkedContentPoint parameter.")
        };

    private ValueTask BeginMarkedRange() =>
        arguments.ObjectAt<PdfObject>(1) switch
        {
            PdfName name => target.BeginMarkedRangeAsync(arguments.NamaAt(0), name),
            PdfDictionary dict => target.BeginMarkedRangeAsync(arguments.NamaAt(0), dict),
            _ => throw new PdfParseException("Invalid BeginMarkedRange parameter")
        };

    private (int numericArgsCount, PdfName? name) ExtendedSetColorParams()
    {
        var argsCount = arguments.Count;
        PdfName? name = null;
        if (LastArgumentIsAName(argsCount))
        {
            argsCount--;
            name = arguments.NamaAt(argsCount);
        }

        return (argsCount, name);
    }

    private bool LastArgumentIsAName(int argsCount) => 
        arguments.TypeAt(argsCount - 1) == ContentStreamValueType.Object;

    private (PdfName? name, double[] numericArguments) CollectExtendedColorArgs()
    {
        var (numericArgsCount, name) = ExtendedSetColorParams();
        // have to allocate here because the Set...ColorExtended call could be async
        var span = new double[numericArgsCount];
        arguments.FillSpan(span);
        return (name, span);
    }

    private ValueTask SetNonstrokingColorExtended()
    {
        var (name, numericArguments) = CollectExtendedColorArgs();
        return target.SetNonstrokingColorExtended(name, numericArguments);
    }

    private ValueTask SetStrokingColorExtended()
    {
        var (name, numericArguments) = CollectExtendedColorArgs();
        return target.SetStrokeColorExtended(name, numericArguments);
    }

    private void SetStrokingColor()
    {
        Span<double> span = stackalloc double[arguments.Count];
        arguments.FillSpan(span);
        target.SetStrokeColor(span);
    }

    private void SetNonstrokingColor()
    {
        Span<double> span = stackalloc double[arguments.Count];
        arguments.FillSpan(span);
        target.SetNonstrokingColor(span);
    }

    private void HandleUnknownOperation()
    {
        if (compatibilitySectionCount < 1) 
            throw new PdfParseException("Unknown content stream operator");
        // otherwise just ignore the unknown operator
    }

    public ValueTask HandleInlineImage(PdfStream inlineImage) => target.DoAsync(inlineImage);
}