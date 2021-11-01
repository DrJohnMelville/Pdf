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
    private readonly List<double> doubles;
    private readonly List<long> longs;
    private readonly List<PdfName> names;

    public ContentStreamContext(IContentStreamOperations target)
    {
        this.target = target;
        doubles = new List<double>();
        longs = new List<long>();
        names = new List<PdfName>();
    }

    public void HandleNumber(double doubleValue, long longValue)
    {
        doubles.Add(doubleValue);
        longs.Add(longValue);
    }

    public void HandleName(PdfName name) => names.Add(name);

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
                break;
            case ContentStreamOperatorValue.BX:
                break;
            case ContentStreamOperatorValue.c:
                target.CurveTo(doubles[0], doubles[1], doubles[2], doubles[3], doubles[4], doubles[5]);
                break;
            case ContentStreamOperatorValue.cm:
                target.ModifyTransformMatrix(
                    doubles[0], doubles[1], doubles[2], doubles[3], doubles[4], doubles[5]);
                break;
            case ContentStreamOperatorValue.CS:
                break;
            case ContentStreamOperatorValue.cs:
                break;
            case ContentStreamOperatorValue.d:
                var span = CollectionsMarshal.AsSpan(doubles);
                target.SetLineDashPattern(span[^1],span[..^1]);
                break;
            case ContentStreamOperatorValue.d0:
                break;
            case ContentStreamOperatorValue.d1:
                break;
            case ContentStreamOperatorValue.Do:
                break;
            case ContentStreamOperatorValue.DP:
                break;
            case ContentStreamOperatorValue.EI:
                break;
            case ContentStreamOperatorValue.EMC:
                break;
            case ContentStreamOperatorValue.ET:
                break;
            case ContentStreamOperatorValue.f:
            case ContentStreamOperatorValue.F: // pdf spec requires this synonym for backward compatability
                target.FillPath();
                break;
            case ContentStreamOperatorValue.fStar:
                target.FillPathEvenOdd();
                break;
            case ContentStreamOperatorValue.G:
                break;
            case ContentStreamOperatorValue.g:
                break;
            case ContentStreamOperatorValue.gs:
                target.LoadGraphicStateDictionary(names[0]);
                break;
            case ContentStreamOperatorValue.h:
                target.ClosePath();
                break;
            case ContentStreamOperatorValue.i:
                target.SetFlatnessTolerance(doubles[0]);
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
                break;
            case ContentStreamOperatorValue.k:
                break;
            case ContentStreamOperatorValue.l:
                target.LineTo(doubles[0], doubles[1]);
                break;
            case ContentStreamOperatorValue.m:
                target.MoveTo(doubles[0], doubles[1]);
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
                target.Rectangle(doubles[0], doubles[1], doubles[2], doubles[3]);
                break;
            case ContentStreamOperatorValue.RG:
                break;
            case ContentStreamOperatorValue.rg:
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
            case ContentStreamOperatorValue.SC:
                break;
            case ContentStreamOperatorValue.SCN:
                break;
            case ContentStreamOperatorValue.sh:
                break;
            case ContentStreamOperatorValue.TStar:
                break;
            case ContentStreamOperatorValue.Tc:
                break;
            case ContentStreamOperatorValue.Td:
                break;
            case ContentStreamOperatorValue.TD:
                break;
            case ContentStreamOperatorValue.Tj:
                break;
            case ContentStreamOperatorValue.TJ:
                break;
            case ContentStreamOperatorValue.TL:
                break;
            case ContentStreamOperatorValue.Tm:
                break;
            case ContentStreamOperatorValue.Tr:
                break;
            case ContentStreamOperatorValue.Ts:
                break;
            case ContentStreamOperatorValue.Tw:
                break;
            case ContentStreamOperatorValue.Tz:
                break;
            case ContentStreamOperatorValue.v:
                target.CurveToWithoutInitialControl(doubles[0], doubles[1], doubles[2], doubles[3]);
                break;
            case ContentStreamOperatorValue.w:
                target.SetLineWidth(doubles[0]);
                break;
            case ContentStreamOperatorValue.W:
                target.ClipToPath();
                break;
            case ContentStreamOperatorValue.WStar:
                target.ClipToPathEvenOdd();
                break;
            case ContentStreamOperatorValue.y:
                target.CurveToWithoutFinalControl(doubles[0], doubles[1], doubles[2], doubles[3]);
                break;
            case ContentStreamOperatorValue.SingleQuote:
                break;
            case ContentStreamOperatorValue.DoubleQuote:
                break;
            default:
                throw new PdfParseException("Unknown content stream operator");
        }

        ClearStacks();
    }

    private void ClearStacks()
    {
        doubles.Clear();
        longs.Clear();
        names.Clear();
    }
}