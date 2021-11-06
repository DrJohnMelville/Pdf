using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Model.Wrappers.ContentValueStreamUnions;

namespace Melville.Pdf.LowLevel.Parsing.ContentStreams;

public readonly struct ContentStreamValueStack
{
    private readonly List<ContentStreamValueUnion> values = new();
    public int Count => values.Count;
    
    public void Add(object item) => values.Add(new ContentStreamValueUnion(item));
    public void Add(in Memory<byte> item) => values.Add(new ContentStreamValueUnion(item));
    public void Add(double floating, long integer) => 
        values.Add(new ContentStreamValueUnion(floating, integer));

    public void Clear() => values.Clear();

    public double DoubleAt(int x) => values[x].Floating;
    public long LongAt(int x) => values[x].Integer;
    public Memory<byte> BytesAt(int x) => values[x].Bytes;
    public ContentStreamValueType TypeAt(int x) => values[x].Type;
    public T ObjectAt<T>(int x)  where T: class => values[x].Object as T ??
                                                   throw new PdfParseException("Wrong type in Content Stream Parser");

    public PdfName NamaAt(int x) => ObjectAt<PdfName>(x);

    public void FillSpan(in Span<double> target)
    {
        for (int i = 0; i < target.Length; i++)
        {
            target[i] = values[i].Floating;
        }
    }

    public Span<ContentStreamValueUnion> NativeSpan() => CollectionsMarshal.AsSpan(values);
}

public class ContentStreamContext
{
    private readonly IContentStreamOperations target;
    private readonly ContentStreamValueStack arguments = new();
    private int compatibilitySectionCount;

    private double GetDouble(int n) => arguments.DoubleAt(n);
    private double GetInteger(int n) => arguments.LongAt(n);
    private Memory<byte> GetString(int n) => arguments.BytesAt(n);
    private PdfName GetName(int n) => arguments.NamaAt(n);
    
    public ContentStreamContext(IContentStreamOperations target)
    {
        this.target = target;
    }

    public void HandleNumber(double doubleValue, long longValue) => 
        arguments.Add(doubleValue, longValue);

    public void HandleName(PdfName name) => arguments.Add(name);
    public void HandleString(in Memory<byte> str) => arguments.Add(str);

    private T NameAs<T>(int pos = 0) where T : PdfName =>
        GetName(pos) as T ?? throw new PdfParseException($"Pdf Name of subtype {typeof(T).Name} expectes");

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
                if (arguments.TypeAt(1) == ContentStreamValueType.Object)
                    target.BeginMarkedRange(GetName(0), GetName(1));
                else
                    target.BeginMarkedRange(GetName(0), new UnparsedDictionary(GetString(1)));
                break;
            case ContentStreamOperatorValue.BI:
                break;
            case ContentStreamOperatorValue.BMC:
                target.BeginMarkedRange(GetName(0));
                break;
            case ContentStreamOperatorValue.BT:
                target.BeginTextObject();
                break;
            case ContentStreamOperatorValue.BX:
                target.BeginCompatibilitySection();
                compatibilitySectionCount++;
                break;
            case ContentStreamOperatorValue.c:
                target.CurveTo(GetDouble(0), GetDouble(1), GetDouble(2), GetDouble(3), GetDouble(4), GetDouble(5));
                break;
            case ContentStreamOperatorValue.cm:
                target.ModifyTransformMatrix(
                    GetDouble(0), GetDouble(1), GetDouble(2), GetDouble(3), GetDouble(4), GetDouble(5));
                break;
            case ContentStreamOperatorValue.CS:
                target.SetStrokingColorSpace(GetName(0));
                break;
            case ContentStreamOperatorValue.cs:
                target.SetNonstrokingColorSpace(GetName(0));
                break;
            case ContentStreamOperatorValue.d:
                Span<double> span = stackalloc double[arguments.Count];
                arguments.FillSpan(span);
                target.SetLineDashPattern(span[^1],span[..^1]);
                break;
            case ContentStreamOperatorValue.d0:
                target.SetColoredGlyphMetrics(GetDouble(0), GetDouble(1));
                break;
            case ContentStreamOperatorValue.d1:
                target.SetUncoloredGlyphMetrics(
                    GetDouble(0), GetDouble(1), GetDouble(2), GetDouble(3), GetDouble(4),
                    GetDouble(5));
                break;
            case ContentStreamOperatorValue.Do:
                target.Do(GetName(0));
                break;
            case ContentStreamOperatorValue.DP:
                switch (arguments.TypeAt(1))
                {
                    case ContentStreamValueType.Object:
                       target.MarkedContentPoint(GetName(0), GetName(1));
                       break;
                    case ContentStreamValueType.Memory:
                        target.MarkedContentPoint(
                            GetName(0), new UnparsedDictionary(arguments.BytesAt(1)));
                        break;
                }
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
                target.SetStrokeGray(GetDouble(0));
                break;
            case ContentStreamOperatorValue.g:
                target.SetNonstrokingGray(GetDouble(0));
                break;
            case ContentStreamOperatorValue.gs:
                target.LoadGraphicStateDictionary(GetName(0));
                break;
            case ContentStreamOperatorValue.h:
                target.ClosePath();
                break;
            case ContentStreamOperatorValue.i:
                target.SetFlatnessTolerance(GetDouble(0));
                break;
            case ContentStreamOperatorValue.ID:
                break;
            case ContentStreamOperatorValue.J:
                target.SetLineCap((LineCap)GetInteger(0));
                break;
            case ContentStreamOperatorValue.j:
                target.SetLineJoinStyle((LineJoinStyle)GetInteger(0));
                break;
            case ContentStreamOperatorValue.K:
                target.SetStrokeCMYK(GetDouble(0), GetDouble(1), GetDouble(2), GetDouble(3));
                break;
            case ContentStreamOperatorValue.k:
                target.SetNonstrokingCMYK(GetDouble(0), GetDouble(1), GetDouble(2), GetDouble(3));
                break;
            case ContentStreamOperatorValue.l:
                target.LineTo(GetDouble(0), GetDouble(1));
                break;
            case ContentStreamOperatorValue.m:
                target.MoveTo(GetDouble(0), GetDouble(1));
                break;
            case ContentStreamOperatorValue.M:
                target.SetMiterLimit(GetDouble(0));
                break;
            case ContentStreamOperatorValue.MP:
                target.MarkedContentPoint(GetName(0));
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
                target.Rectangle(GetDouble(0), GetDouble(1), GetDouble(2), GetDouble(3));
                break;
            case ContentStreamOperatorValue.RG:
                target.SetStrokeRGB(GetDouble(0), GetDouble(1), GetDouble(2));
                break;
            case ContentStreamOperatorValue.rg:
                target.SetNonstrokingRGB(GetDouble(0), GetDouble(1), GetDouble(2));
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
                SetNonstrokingColor();
                break;
            case ContentStreamOperatorValue.SC:
                SetStrokingColor();
                break;
            case ContentStreamOperatorValue.SCN:
                SetStrokingColorExtended();
                break;
            case ContentStreamOperatorValue.scn:
                SetNonstrokingColorExtended();
                break;
            case ContentStreamOperatorValue.sh:
                break;
            case ContentStreamOperatorValue.TStar:
                target.MoveToNextTextLine();
                break;
            case ContentStreamOperatorValue.Tc:
                target.SetCharSpace(GetDouble(0));
                break;
            case ContentStreamOperatorValue.Td:
                target.MovePositionBy(GetDouble(0), GetDouble(1));
                break;
            case ContentStreamOperatorValue.TD:
                target.MovePositionByWithLeading(GetDouble(0), GetDouble(1));
                break;
            case ContentStreamOperatorValue.Tf:
                target.SetFont(GetName(0), GetDouble(1));
                break;
            case ContentStreamOperatorValue.Tj:
                target.ShowString(GetString(0));
                break;
            case ContentStreamOperatorValue.TJ:
                target.ShowSpacedString(arguments.NativeSpan());
                break;
            case ContentStreamOperatorValue.TL:
                target.SetTextLeading(GetDouble(0));
                break;
            case ContentStreamOperatorValue.Tm:
                target.SetTextMatrix(GetDouble(0),GetDouble(1),GetDouble(2),GetDouble(3),GetDouble(4),GetDouble(5));
                break;
            case ContentStreamOperatorValue.Tr:
                target.SetTextRender((TextRendering)GetInteger(0));
                break;
            case ContentStreamOperatorValue.Ts:
                target.SetTextRise(GetDouble(0));
                break;
            case ContentStreamOperatorValue.Tw:
                target.SetWordSpace(GetDouble(0));
                break;
            case ContentStreamOperatorValue.Tz:
                target.SetHorizontalTextScaling(GetDouble(0));
                break;
            case ContentStreamOperatorValue.v:
                target.CurveToWithoutInitialControl(GetDouble(0), GetDouble(1), GetDouble(2), GetDouble(3));
                break;
            case ContentStreamOperatorValue.w:
                target.SetLineWidth(GetDouble(0));
                break;
            case ContentStreamOperatorValue.W:
                target.ClipToPath();
                break;
            case ContentStreamOperatorValue.WStar:
                target.ClipToPathEvenOdd();
                break;
            case ContentStreamOperatorValue.y:
                target.CurveToWithoutFinalControl(GetDouble(0), GetDouble(1), GetDouble(2), GetDouble(3));
                break;
            case ContentStreamOperatorValue.SingleQuote:
                target.MoveToNextLineAndShowString(GetString(0));
                break;
            case ContentStreamOperatorValue.DoubleQuote:
                target.MoveToNextLineAndShowString(GetDouble(0), GetDouble(1), GetString(2));
                break;
            default:
                HandleUnknownOperation();
                break;
        }
        
        arguments.Clear();
    }

    private void SetNonstrokingColorExtended()
    {
        var argsCount = arguments.Count;
        PdfName? name = null;
        if (arguments.TypeAt(argsCount - 1) == ContentStreamValueType.Object)
        {
            argsCount--;
            name = GetName(argsCount);
        }
        Span<double> span = stackalloc double[argsCount];
        arguments.FillSpan(span);
           target.SetNonstrokingColorExtended(name, span);
    }

    private void SetStrokingColorExtended()
    {
        var argsCount = arguments.Count;
        PdfName? name = null;
        if (arguments.TypeAt(argsCount - 1) == ContentStreamValueType.Object)
        {
            argsCount--;
            name = GetName(argsCount);
        }
        Span<double> span = stackalloc double[argsCount];
        arguments.FillSpan(span);
            target.SetStrokeColorExtended(name, span);
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
}