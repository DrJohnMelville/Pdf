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
                break;
            case ContentStreamOperatorValue.B:
                break;
            case ContentStreamOperatorValue.bStar:
                break;
            case ContentStreamOperatorValue.BStar:
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
                break;
            case ContentStreamOperatorValue.F:
                break;
            case ContentStreamOperatorValue.fStar:
                break;
            case ContentStreamOperatorValue.G:
                break;
            case ContentStreamOperatorValue.g:
                break;
            case ContentStreamOperatorValue.gs:
                break;
            case ContentStreamOperatorValue.h:
                break;
            case ContentStreamOperatorValue.i:
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
                break;
            case ContentStreamOperatorValue.m:
                break;
            case ContentStreamOperatorValue.M:
                break;
            case ContentStreamOperatorValue.MP:
                break;
            case ContentStreamOperatorValue.n:
                break;
            case ContentStreamOperatorValue.q:
                target.SaveGraphicsState();
                break;
            case ContentStreamOperatorValue.Q:
                target.RestoreGraphicsState();
                break;
            case ContentStreamOperatorValue.re:
                break;
            case ContentStreamOperatorValue.RG:
                break;
            case ContentStreamOperatorValue.rg:
                break;
            case ContentStreamOperatorValue.ri:
                target.SetRenderIntent(NameAs<RenderingIntentName>());
                break;
            case ContentStreamOperatorValue.s:
                break;
            case ContentStreamOperatorValue.S:
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
                break;
            case ContentStreamOperatorValue.w:
                target.SetLineWidth(doubles[0]);
                break;
            case ContentStreamOperatorValue.W:
                break;
            case ContentStreamOperatorValue.WStar:
                break;
            case ContentStreamOperatorValue.y:
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