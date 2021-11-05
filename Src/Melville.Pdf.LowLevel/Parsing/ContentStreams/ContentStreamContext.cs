using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.LowLevel.Parsing.ContentStreams;

public readonly struct ContentStreamContext
{
    private readonly IContentStreamOperations target;
    private readonly List<long> longs;
    private readonly List<PdfName> names;
    private readonly InterleavedArrayBuilder<Memory<byte>, double> interleavedArray;
 //   private readonly List<double> doubles;
 //   private readonly List<Memory<byte>> strings;

    public ContentStreamContext(IContentStreamOperations target)
    {
        this.target = target;
        interleavedArray = new();
//        doubles = new List<double>();
        longs = new List<long>();
        names = new List<PdfName>();
  //      strings = new List<Memory<byte>>();
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
                break;
            case ContentStreamOperatorValue.BI:
                break;
            case ContentStreamOperatorValue.BMC:
                break;
            case ContentStreamOperatorValue.BT:
                target.BeginTextObject();
                break;
            case ContentStreamOperatorValue.BX:
                break;
            case ContentStreamOperatorValue.c:
                target.CurveTo(interleavedArray.GetT2(0), interleavedArray.GetT2(1), interleavedArray.GetT2(2), interleavedArray.GetT2(3), interleavedArray.GetT2(4), interleavedArray.GetT2(5));
                break;
            case ContentStreamOperatorValue.cm:
                target.ModifyTransformMatrix(
                    interleavedArray.GetT2(0), interleavedArray.GetT2(1), interleavedArray.GetT2(2), interleavedArray.GetT2(3), interleavedArray.GetT2(4), interleavedArray.GetT2(5));
                break;
            case ContentStreamOperatorValue.CS:
                break;
            case ContentStreamOperatorValue.cs:
                break;
            case ContentStreamOperatorValue.d:
                var span = interleavedArray.GetT2Span();
                target.SetLineDashPattern(span[^1],span[..^1]);
                break;
            case ContentStreamOperatorValue.d0:
                break;
            case ContentStreamOperatorValue.d1:
                break;
            case ContentStreamOperatorValue.Do:
                target.Do(names[0]);
                break;
            case ContentStreamOperatorValue.DP:
                break;
            case ContentStreamOperatorValue.EI:
                break;
            case ContentStreamOperatorValue.EMC:
                break;
            case ContentStreamOperatorValue.ET:
                target.EndTextObject();
                break;
            case ContentStreamOperatorValue.f:
            case ContentStreamOperatorValue.F: // pdf spec requires this synonym for backward compatability
                target.FillPath();
                break;
            case ContentStreamOperatorValue.fStar:
                target.FillPathEvenOdd();
                break;
            case ContentStreamOperatorValue.G:
                target.SetStrokeGray(interleavedArray.GetT2(0));
                break;
            case ContentStreamOperatorValue.g:
                target.SetNonstrokingGray(interleavedArray.GetT2(0));
                break;
            case ContentStreamOperatorValue.gs:
                target.LoadGraphicStateDictionary(names[0]);
                break;
            case ContentStreamOperatorValue.h:
                target.ClosePath();
                break;
            case ContentStreamOperatorValue.i:
                target.SetFlatnessTolerance(interleavedArray.GetT2(0));
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
                target.SetStrokeCMYK(interleavedArray.GetT2(0), interleavedArray.GetT2(1), interleavedArray.GetT2(2), interleavedArray.GetT2(3));
                break;
            case ContentStreamOperatorValue.k:
                target.SetNonstrokingCMYK(interleavedArray.GetT2(0), interleavedArray.GetT2(1), interleavedArray.GetT2(2), interleavedArray.GetT2(3));
                break;
            case ContentStreamOperatorValue.l:
                target.LineTo(interleavedArray.GetT2(0), interleavedArray.GetT2(1));
                break;
            case ContentStreamOperatorValue.m:
                target.MoveTo(interleavedArray.GetT2(0), interleavedArray.GetT2(1));
                break;
            case ContentStreamOperatorValue.M:
                break;
            case ContentStreamOperatorValue.MP:
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
                target.Rectangle(interleavedArray.GetT2(0), interleavedArray.GetT2(1), interleavedArray.GetT2(2), interleavedArray.GetT2(3));
                break;
            case ContentStreamOperatorValue.RG:
                target.SetStrokeRGB(interleavedArray.GetT2(0), interleavedArray.GetT2(1), interleavedArray.GetT2(2));
                break;
            case ContentStreamOperatorValue.rg:
                target.SetNonstrokingRGB(interleavedArray.GetT2(0), interleavedArray.GetT2(1), interleavedArray.GetT2(2));
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
                target.SetCharSpace(interleavedArray.GetT2(0));
                break;
            case ContentStreamOperatorValue.Td:
                target.MovePositionBy(interleavedArray.GetT2(0), interleavedArray.GetT2(1));
                break;
            case ContentStreamOperatorValue.TD:
                target.MovePositionByWithLeading(interleavedArray.GetT2(0), interleavedArray.GetT2(1));
                break;
            case ContentStreamOperatorValue.Tf:
                target.SetFont(names[0], interleavedArray.GetT2(0));
                break;
            case ContentStreamOperatorValue.Tj:
                target.ShowString(interleavedArray.GetT1(0));
                break;
            case ContentStreamOperatorValue.TJ:
                target.ShowSpacedString(interleavedArray.GetInterleavedArray());
                break;
            case ContentStreamOperatorValue.TL:
                target.SetTextLeading(interleavedArray.GetT2(0));
                break;
            case ContentStreamOperatorValue.Tm:
                target.SetTextMatrix(interleavedArray.GetT2(0),interleavedArray.GetT2(1),interleavedArray.GetT2(2),interleavedArray.GetT2(3),interleavedArray.GetT2(4),interleavedArray.GetT2(5));
                break;
            case ContentStreamOperatorValue.Tr:
                target.SetTextRender((TextRendering)longs[0]);
                break;
            case ContentStreamOperatorValue.Ts:
                target.SetTextRise(interleavedArray.GetT2(0));
                break;
            case ContentStreamOperatorValue.Tw:
                target.SetWordSpace(interleavedArray.GetT2(0));
                break;
            case ContentStreamOperatorValue.Tz:
                target.SetHorizontalTextScaling(interleavedArray.GetT2(0));
                break;
            case ContentStreamOperatorValue.v:
                target.CurveToWithoutInitialControl(interleavedArray.GetT2(0), interleavedArray.GetT2(1), interleavedArray.GetT2(2), interleavedArray.GetT2(3));
                break;
            case ContentStreamOperatorValue.w:
                target.SetLineWidth(interleavedArray.GetT2(0));
                break;
            case ContentStreamOperatorValue.W:
                target.ClipToPath();
                break;
            case ContentStreamOperatorValue.WStar:
                target.ClipToPathEvenOdd();
                break;
            case ContentStreamOperatorValue.y:
                target.CurveToWithoutFinalControl(interleavedArray.GetT2(0), interleavedArray.GetT2(1), interleavedArray.GetT2(2), interleavedArray.GetT2(3));
                break;
            case ContentStreamOperatorValue.SingleQuote:
                target.MoveToNextLineAndShowString(interleavedArray.GetT1(0));
                break;
            case ContentStreamOperatorValue.DoubleQuote:
                target.MoveToNextLineAndShowString(interleavedArray.GetT2(0), interleavedArray.GetT2(1), interleavedArray.GetT1(0));
                break;
            default:
                throw new PdfParseException("Unknown content stream operator");
        }

        ClearStacks();
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