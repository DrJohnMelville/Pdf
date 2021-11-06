using System;
using System.Collections.Generic;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.LowLevel.Parsing.ContentStreams;

public class ContentStreamContext
{
    private readonly IContentStreamOperations target;
    private readonly List<long> longs;
    private readonly List<PdfName> names;
    private readonly InterleavedArrayBuilder<Memory<byte>, double> interleavedArray;
    private int compatibilitySectionCount;

    private double DoubleStack(int n) => interleavedArray.GetT2(n);
    private Memory<byte> StringStack(int n) => interleavedArray.GetT1(n);
    
    public ContentStreamContext(IContentStreamOperations target)
    {
        this.target = target;
        interleavedArray = new();
        longs = new List<long>();
        names = new List<PdfName>();
    }

    public void HandleNumber(double doubleValue, long longValue)
    {
        interleavedArray.Handle(doubleValue);
        longs.Add(longValue);
    }

    public void HandleName(PdfName name) => names.Add(name);
    public void HandleString(in Memory<byte> str) => interleavedArray.Handle(str);

    private T NameAs<T>(int pos = 0) where T : PdfName =>
        names[pos] as T ?? throw new PdfParseException($"Pdf Name of subtype {typeof(T).Name} expectes");

    public void HandleOpCode(ContentStreamOperatorValue opCode)
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
                if (names.Count > 1)
                    target.BeginMarkedRange(names[0], names[1]);
                else
                    target.BeginMarkedRange(names[0], new UnparsedDictionary(StringStack(0)));
                break;
            case ContentStreamOperatorValue.BI:
                break;
            case ContentStreamOperatorValue.BMC:
                target.BeginMarkedRange(names[0]);
                break;
            case ContentStreamOperatorValue.BT:
                target.BeginTextObject();
                break;
            case ContentStreamOperatorValue.BX:
                target.BeginCompatibilitySection();
                compatibilitySectionCount++;
                break;
            case ContentStreamOperatorValue.c:
                target.CurveTo(DoubleStack(0), DoubleStack(1), DoubleStack(2), DoubleStack(3), DoubleStack(4), DoubleStack(5));
                break;
            case ContentStreamOperatorValue.cm:
                target.ModifyTransformMatrix(
                    DoubleStack(0), DoubleStack(1), DoubleStack(2), DoubleStack(3), DoubleStack(4), DoubleStack(5));
                break;
            case ContentStreamOperatorValue.CS:
                target.SetStrokingColorSpace(names[0]);
                break;
            case ContentStreamOperatorValue.cs:
                target.SetNonstrokingColorSpace(names[0]);
                break;
            case ContentStreamOperatorValue.d:
                var span = interleavedArray.GetT2Span();
                target.SetLineDashPattern(span[^1],span[..^1]);
                break;
            case ContentStreamOperatorValue.d0:
                target.SetColoredGlyphMetrics(DoubleStack(0), DoubleStack(1));
                break;
            case ContentStreamOperatorValue.d1:
                target.SetUncoloredGlyphMetrics(
                    DoubleStack(0), DoubleStack(1), DoubleStack(2), DoubleStack(3), DoubleStack(4),
                    DoubleStack(5));
                break;
            case ContentStreamOperatorValue.Do:
                target.Do(names[0]);
                break;
            case ContentStreamOperatorValue.DP:
                if (names.Count > 1)
                    target.MarkedContentPoint(names[0], names[1]);
                else
                    target.MarkedContentPoint(
                        names[0], new UnparsedDictionary(StringStack(0)));
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
            case ContentStreamOperatorValue.F: // pdf spec requires this synonym for backward compatability
                target.FillPath();
                break;
            case ContentStreamOperatorValue.fStar:
                target.FillPathEvenOdd();
                break;
            case ContentStreamOperatorValue.G:
                target.SetStrokeGray(DoubleStack(0));
                break;
            case ContentStreamOperatorValue.g:
                target.SetNonstrokingGray(DoubleStack(0));
                break;
            case ContentStreamOperatorValue.gs:
                target.LoadGraphicStateDictionary(names[0]);
                break;
            case ContentStreamOperatorValue.h:
                target.ClosePath();
                break;
            case ContentStreamOperatorValue.i:
                target.SetFlatnessTolerance(DoubleStack(0));
                break;
            case ContentStreamOperatorValue.ID:
                break;
            case ContentStreamOperatorValue.J:
                target.SetLineCap((LineCap)longs[0]);
                break;
            case ContentStreamOperatorValue.j:
                target.SetLineJoinStyle((LineJoinStyle)longs[0]);
                break;
            case ContentStreamOperatorValue.K:
                target.SetStrokeCMYK(DoubleStack(0), DoubleStack(1), DoubleStack(2), DoubleStack(3));
                break;
            case ContentStreamOperatorValue.k:
                target.SetNonstrokingCMYK(DoubleStack(0), DoubleStack(1), DoubleStack(2), DoubleStack(3));
                break;
            case ContentStreamOperatorValue.l:
                target.LineTo(DoubleStack(0), DoubleStack(1));
                break;
            case ContentStreamOperatorValue.m:
                target.MoveTo(DoubleStack(0), DoubleStack(1));
                break;
            case ContentStreamOperatorValue.M:
                target.SetMiterLimit(DoubleStack(0));
                break;
            case ContentStreamOperatorValue.MP:
                target.MarkedContentPoint(names[0]);
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
                target.Rectangle(DoubleStack(0), DoubleStack(1), DoubleStack(2), DoubleStack(3));
                break;
            case ContentStreamOperatorValue.RG:
                target.SetStrokeRGB(DoubleStack(0), DoubleStack(1), DoubleStack(2));
                break;
            case ContentStreamOperatorValue.rg:
                target.SetNonstrokingRGB(DoubleStack(0), DoubleStack(1), DoubleStack(2));
                break;
            case ContentStreamOperatorValue.ri:
                target.SetRenderIntent(NameAs<RenderingIntentName>());
                break;
            case ContentStreamOperatorValue.s:
                target.CloseAndStrokePath();
                break;
            case ContentStreamOperatorValue.S:
                target.StrokePath();
                break;
            case ContentStreamOperatorValue.sc:
                target.SetNonstrokingColor(interleavedArray.GetT2Span());
                break;
            case ContentStreamOperatorValue.SC:
                target.SetStrokeColor(interleavedArray.GetT2Span());
                break;
            case ContentStreamOperatorValue.SCN:
                target.SetStrokeColorExtended(TryGetFirstName(), interleavedArray.GetT2Span());
                break;
            case ContentStreamOperatorValue.scn:
                target.SetNonstrokingColorExtended(TryGetFirstName(), interleavedArray.GetT2Span());
                break;
            case ContentStreamOperatorValue.sh:
                break;
            case ContentStreamOperatorValue.TStar:
                target.MoveToNextTextLine();
                break;
            case ContentStreamOperatorValue.Tc:
                target.SetCharSpace(DoubleStack(0));
                break;
            case ContentStreamOperatorValue.Td:
                target.MovePositionBy(DoubleStack(0), DoubleStack(1));
                break;
            case ContentStreamOperatorValue.TD:
                target.MovePositionByWithLeading(DoubleStack(0), DoubleStack(1));
                break;
            case ContentStreamOperatorValue.Tf:
                target.SetFont(names[0], DoubleStack(0));
                break;
            case ContentStreamOperatorValue.Tj:
                target.ShowString(StringStack(0));
                break;
            case ContentStreamOperatorValue.TJ:
                target.ShowSpacedString(interleavedArray.GetInterleavedArray());
                break;
            case ContentStreamOperatorValue.TL:
                target.SetTextLeading(DoubleStack(0));
                break;
            case ContentStreamOperatorValue.Tm:
                target.SetTextMatrix(DoubleStack(0),DoubleStack(1),DoubleStack(2),DoubleStack(3),DoubleStack(4),DoubleStack(5));
                break;
            case ContentStreamOperatorValue.Tr:
                target.SetTextRender((TextRendering)longs[0]);
                break;
            case ContentStreamOperatorValue.Ts:
                target.SetTextRise(DoubleStack(0));
                break;
            case ContentStreamOperatorValue.Tw:
                target.SetWordSpace(DoubleStack(0));
                break;
            case ContentStreamOperatorValue.Tz:
                target.SetHorizontalTextScaling(DoubleStack(0));
                break;
            case ContentStreamOperatorValue.v:
                target.CurveToWithoutInitialControl(DoubleStack(0), DoubleStack(1), DoubleStack(2), DoubleStack(3));
                break;
            case ContentStreamOperatorValue.w:
                target.SetLineWidth(DoubleStack(0));
                break;
            case ContentStreamOperatorValue.W:
                target.ClipToPath();
                break;
            case ContentStreamOperatorValue.WStar:
                target.ClipToPathEvenOdd();
                break;
            case ContentStreamOperatorValue.y:
                target.CurveToWithoutFinalControl(DoubleStack(0), DoubleStack(1), DoubleStack(2), DoubleStack(3));
                break;
            case ContentStreamOperatorValue.SingleQuote:
                target.MoveToNextLineAndShowString(StringStack(0));
                break;
            case ContentStreamOperatorValue.DoubleQuote:
                target.MoveToNextLineAndShowString(DoubleStack(0), DoubleStack(1), StringStack(0));
                break;
            default:
                HandleUnknownOperation();
                break;
        }
        
        ClearStacks();
    }

    private void HandleUnknownOperation()
    {
        if (compatibilitySectionCount < 1) 
            throw new PdfParseException("Unknown content stream operator");
        // otherwise just ignore the unknown operator
    }

    private PdfName? TryGetFirstName()
    {
        return names.Count > 0 ? names[0] : null;
    }

    private void ClearStacks()
    {
        interleavedArray.Clear();
        longs.Clear();
        names.Clear();
    }
}