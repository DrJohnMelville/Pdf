using System;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Parsing.ContentStreams.EmbeddedImageParsing;
using Melville.Postscript.Interpreter.FunctionLibrary;
using Melville.Postscript.Interpreter.InterpreterState;
using Melville.Postscript.Interpreter.Values;
using Melville.Postscript.Interpreter.Values.Execution;

namespace Melville.Pdf.LowLevel.Parsing.ContentStreams;

[TypeShortcut("Melville.Pdf.LowLevel.Model.ContentStreams.IContentStreamOperations",
    "GetTarget(engine)")]
internal static partial class ContentStreamParsingOperations
{
    private static IContentStreamOperations GetTarget(PostscriptEngine engine) =>
        (IContentStreamOperations)engine.Tag;
    private static void PopMarkObject(OperandStack stack)
    {
        var mark = stack.Pop();
        Debug.Assert(mark.IsMark);
    }

    [PostscriptMethod("true")]
    private static bool True() => true;

    [PostscriptMethod("false")]
    private static bool False() => false;

    [PostscriptMethod("[")]
    [PostscriptMethod("<<")]
    private static PostscriptValue ArrayBegin() => 
        PostscriptValueFactory.CreateMark();

    [PostscriptMethod("]")]
    private static PostscriptValue ArrayEnd() =>
        new PostscriptValue(ArrayTopMarker.Instance,
            PostscriptBuiltInOperations.PushArgument, default);

    [PostscriptMethod("$IgnoreCount")]
    private static int IgnoreCount() => 0;

    [PostscriptMethod("b")]
    private static void CloseFillAndStrokePath(IContentStreamOperations target) =>
        target.CloseFillAndStrokePath();

    [PostscriptMethod("b*")]
    private static void CloseFillAndStrokePathEvenOdd(IContentStreamOperations target) =>
        target.CloseFillAndStrokePathEvenOdd();

    [PostscriptMethod("B")]
    private static void FillAndStrokePath(IContentStreamOperations target) =>
        target.FillAndStrokePath();

    [PostscriptMethod("B*")]
    private static void FillAndStrokePathEvenOdd(IContentStreamOperations target) =>
        target.FillAndStrokePathEvenOdd();

    [PostscriptMethod("f")]
    [PostscriptMethod("F")]
    private static void FillPath(IContentStreamOperations target) => target.FillPath();

    [PostscriptMethod("f*")]
    private static void FillPathEvenOdd(IContentStreamOperations target) => target.FillPathEvenOdd();

    [PostscriptMethod("n")]
    private static void EndPathWithNoOp(IContentStreamOperations target) => target.EndPathWithNoOp();

    [PostscriptMethod("s")]
    private static void CloseAndStrokePath(IContentStreamOperations target) => target.CloseAndStrokePath();

    [PostscriptMethod("S")]
    private static void StrokePath(IContentStreamOperations target) => target.StrokePath();

    [PostscriptMethod("W")]
    private static void ClipToPath(IContentStreamOperations target) => target.ClipToPath();

    [PostscriptMethod("W*")]
    private static void ClipToPathEvenOdd(IContentStreamOperations target) => target.ClipToPathEvenOdd();

    [PostscriptMethod("h")]
    private static void ClosePath(IContentStreamOperations target) => target.ClosePath();

    [PostscriptMethod("q")]
    private static void SaveGraphicsState(IContentStreamOperations target) => target.SaveGraphicsState();

    [PostscriptMethod("Q")]
    private static void RestoreGraphicsState(IContentStreamOperations target) => target.RestoreGraphicsState();

    [PostscriptMethod("c")]
    private static void CurveTo(IContentStreamOperations target,
        double a1, double a2, double a3, double a4, double a5, double a6) =>
        target.CurveTo(a1, a2, a3, a4, a5, a6);

    [PostscriptMethod("v")]
    private static void CurveToWithoutInitialControl(IContentStreamOperations target,
        double a1, double a2, double a3, double a4) =>
        target.CurveToWithoutInitialControl(a1, a2, a3, a4);

    [PostscriptMethod("y")]
    private static void CurveToWithoutFinalControl(IContentStreamOperations target,
        double a1, double a2, double a3, double a4) =>
        target.CurveToWithoutFinalControl(a1, a2, a3, a4);
    [PostscriptMethod("re")]
    private static void Rectangle(IContentStreamOperations target,
        double a1, double a2, double a3, double a4) =>
        target.Rectangle(a1, a2, a3, a4);

    [PostscriptMethod("m")]
    private static void MoveTo(IContentStreamOperations target, double x, double y) => target.MoveTo(x, y);

    [PostscriptMethod("l")]
    private static void LineTo(IContentStreamOperations target, double x, double y) => target.LineTo(x, y);

    [PostscriptMethod("J")]
    private static void SetLineCap(IContentStreamOperations target, long cap) => target.SetLineCap((LineCap)cap);

    [PostscriptMethod("j")]
    private static void SetLineJoin(IContentStreamOperations target, long join) =>
        target.SetLineJoinStyle((LineJoinStyle)join);

    [PostscriptMethod("w")]
    private static void SetLineWidth(IContentStreamOperations target, double width) =>
        target.SetLineWidth(width);

    [PostscriptMethod("M")]
    private static void SetMiterLimit(IContentStreamOperations target, double miter) =>
        target.SetMiterLimit(miter);

    [PostscriptMethod("cm")]
    private static void ModifyTransformMatrix(IContentStreamOperations target,
        float a1, float a2, float a3, float a4, float a5, float a6) =>
        target.ModifyTransformMatrix(new Matrix3x2(a1, a2, a3, a4, a5, a6));

    [PostscriptMethod("i")]
    private static void SetFlatness(IContentStreamOperations target, double flatness) =>
        target.SetFlatnessTolerance(flatness);

    [PostscriptMethod("ri")]
    private static void SetRenderIntent(IContentStreamOperations target, PostscriptValue renderIntent) =>
        target.SetRenderIntent(new RenderIntentName(renderIntent.AsPdfName()));

    [PostscriptMethod("d")]
    private static void SetLineDashPattern(IContentStreamOperations target, OperandStack stack, 
        PostscriptValue arrayTop, double phase)
    {
        Debug.Assert(IsArrayTopMarker(arrayTop));

        var patternSegment = stack.SpanAboveMark();
        Span<double> pattern = stackalloc double[patternSegment.Count()];
        stack.PopSpan(pattern);
        target.SetLineDashPattern(phase, pattern);
        patternSegment.PopDataAndMark();
    }

    private static bool IsArrayTopMarker(PostscriptValue item) => 
        item.TryGet(out ArrayTopMarker? _);

    [PostscriptMethod("SC")]
    private static void SetStrokingColor(IContentStreamOperations target, OperandStack stack)
    {
        Span<double> color = stackalloc double[stack.Count];
        stack.PopSpan(color);
        target.SetStrokeColor(color);

    }

    [PostscriptMethod("sc")]
    private static void SetNonstrokingColor(IContentStreamOperations target, OperandStack stack)
    {
        Span<double> color = stackalloc double[stack.Count];
        stack.PopSpan(color);
        target.SetNonstrokingColor(color);
    }

    [PostscriptMethod("d0")]
    private static void SetColoredGlyphMetric(IContentStreamOperations target, double d1, double d2) =>
        target.SetColoredGlyphMetrics(d1, d2);

    [PostscriptMethod("d1")]
    private static void SetUncoloredGlyphMetric(IContentStreamOperations target,
        double d1, double d2, double d3, double d4, double d5, double d6) =>
        target.SetUncoloredGlyphMetrics(d1, d2, d3, d4, d5, d6);

    [PostscriptMethod("BT")]
    private static void BeginText(IContentStreamOperations target) => target.BeginTextObject();

    [PostscriptMethod("ET")]
    private static void EndText(IContentStreamOperations target) => target. EndTextObject();

    [PostscriptMethod("Td")]
    private static void MoveTextPosition(IContentStreamOperations target, double x, double y) =>
        target.MovePositionBy(x, y);

    [PostscriptMethod("TD")]
    private static void MoveTextPositionWithLeading(IContentStreamOperations target, double x, double y) =>
        target.MovePositionByWithLeading(x, y);

    [PostscriptMethod("T*")]
    private static void MoveToNextLine(IContentStreamOperations target) => target.MoveToNextTextLine();

    [PostscriptMethod("Tc")]
    private static void SetCharSpace(IContentStreamOperations target, double space) => target.SetCharSpace(space);

    [PostscriptMethod("TL")]
    private static void SetTextLeading(IContentStreamOperations target, double leading) => target.SetTextLeading(leading);

    [PostscriptMethod("Ts")]
    private static void SetTextRise(IContentStreamOperations target, double rise) => target.SetTextRise(rise);

    [PostscriptMethod("Tw")]
    private static void SetWordSpace(IContentStreamOperations target, double space) => target.SetWordSpace(space);

    [PostscriptMethod("Tz")]
    private static void SetHorizontalTextScaling(IContentStreamOperations target, double scaling) => 
        target.SetHorizontalTextScaling(scaling);

    [PostscriptMethod("Tr")]
    private static void SetTextRender(IContentStreamOperations target, long render) =>
        target.SetTextRender((TextRendering)render);

    [PostscriptMethod("Tm")]
    private static void SetTextMatric(IContentStreamOperations target,
        double d1, double d2, double d3, double d4, double d5, double d6) =>
        target.SetTextMatrix(d1, d2, d3, d4, d5, d6);

    [PostscriptMethod("MP")]
    private static void MarkedContentPoint(IContentStreamOperations target, in PostscriptValue name) =>
        target.MarkedContentPoint(name.AsPdfName());

    [PostscriptMethod("BMC")]
    private static void BeginMarkedContent(IContentStreamOperations target, in PostscriptValue name) =>
        target.BeginMarkedRange(name.AsPdfName());

    [PostscriptMethod("EMC")]
    private static void EndMarkedContent(IContentStreamOperations target) => target.EndMarkedRange();

    [PostscriptMethod(">>")]
    private static PostscriptValue CreateDictionary(OperandStack stack) => new(
        new PdfObjectCreator(stack, DictionaryTranslator.None).PopDictionaryBuilderFromStack().AsDictionary(), 
        PostscriptBuiltInOperations.PushArgument, default);

    [PostscriptMethod("BX")]
    private static void BeginCompatibilitySection(IContentStreamOperations target, PostscriptEngine engine)
    {
        target.BeginCompatibilitySection();
        if (IncrementIgnoreCount(engine, 1) != 1) return;
        engine.ErrorDict.Put("undefined"u8, PostscriptValueFactory.Create(IgnoreError.Instance));
    }

    private static int IncrementIgnoreCount(PostscriptEngine engine, int increment)
    {
        var ret = engine.SystemDict.TryGetAs("$IgnoreCount"u8, out long val)?val:0;
        ret += increment;
        engine.SystemDict.Put("$IgnoreCount", ret);
        return (int)ret;
    }

    [StaticSingleton]
    private partial class IgnoreError : BuiltInFunction
    {
        public override void Execute(PostscriptEngine engine, in PostscriptValue value)
        {
            while (engine.OperandStack.Count > 1) engine.OperandStack.Pop();
            engine.ErrorData.Put("newerror"u8, false);
        }
    }

    [PostscriptMethod("EX")]
    private static void EndCompatibilitySection(IContentStreamOperations target, PostscriptEngine engine)
    {
        target.EndCompatibilitySection();
        if (IncrementIgnoreCount(engine, -1) != 0) return;
        engine.ErrorDict.Undefine("undefined"u8);
    }

    [PostscriptMethod("BI")]
    private static void BeginInlineImage(OperandStack stack) => stack.Push(PostscriptValueFactory.CreateMark());

    [PostscriptMethod("ID")]
    private static ValueTask EndInlineImageAsync(PostscriptEngine engine, IContentStreamOperations target) =>
        new InlineImageParser(engine, target).ParseAsync();

    [PostscriptMethod("Do")]
    private static ValueTask DoAsync(IContentStreamOperations operations, in PostscriptValue name) =>
        operations.DoAsync(name.AsPdfName());

    [PostscriptMethod("gs")]
    private static ValueTask LoadGraphicStateDictionaryAsync(IContentStreamOperations target, in PostscriptValue dictName) =>
        target.LoadGraphicStateDictionaryAsync(dictName.AsPdfName());

    [PostscriptMethod("cs")]
    private static ValueTask SetNonstrokingColorSpaceAsync(IContentStreamOperations target, in PostscriptValue csName) =>
        target.SetNonstrokingColorSpaceAsync(csName.AsPdfName());

    [PostscriptMethod("k")]
    private static ValueTask SetNonstrokingCmykAsync(IContentStreamOperations target, 
        double c, double m, double y, double k) =>
        target.SetNonstrokingCMYKAsync(c,m,y,k);

    [PostscriptMethod("rg")]
    private static ValueTask SetNonstrokingRgbAsync(IContentStreamOperations target, 
        double r, double g, double b) =>
        target.SetNonstrokingRgbAsync(r,g,b);

    [PostscriptMethod("g")]
    private static ValueTask SetNonstrokingGrayAsync(IContentStreamOperations target, double g) =>
        target.SetNonstrokingGrayAsync(g);

    [PostscriptMethod("scn")]
    private static ValueTask SetNonStrokingExtendedAsync(
        IContentStreamOperations target, OperandStack stack)
    {
        PdfDirectObject? name = stack.Peek().IsLiteralName ?
            (PdfDirectObject?)stack.Pop().AsPdfName() : null;
        Span<double> color = stackalloc double[stack.Count];
        stack.PopSpan(color);
        return target.SetNonstrokingColorExtendedAsync(name, color);
    }

    [PostscriptMethod("CS")]
    private static ValueTask SetStrokingColorSpaceAsync(IContentStreamOperations target, in PostscriptValue csName) =>
        target.SetStrokingColorSpaceAsync(csName.AsPdfName());

    [PostscriptMethod("K")]
    private static ValueTask SetStrokingCmykAsync(IContentStreamOperations target, 
        double c, double m, double y, double k) =>
        target.SetStrokeCMYKAsync(c,m,y,k);

    [PostscriptMethod("RG")]
    private static ValueTask SetStrokingRgbAsync(IContentStreamOperations target, 
        double r, double g, double b) =>
        target.SetStrokeRGBAsync(r,g,b);

    [PostscriptMethod("G")]
    private static ValueTask SetStrokingGrayAsync(IContentStreamOperations target, double g) =>
        target.SetStrokeGrayAsync(g);

    [PostscriptMethod("SCN")]
    private static ValueTask SetStrokingExtendedAsync(
        IContentStreamOperations target, OperandStack stack)
    {
        PdfDirectObject? name = stack.Peek().IsLiteralName ? (PdfDirectObject?)stack.Pop().AsPdfName() : null;
        Span<double> color = stackalloc double[stack.Count];
        stack.PopSpan(color);
        return target.SetStrokeColorExtendedAsync(name, color);
    }

    [PostscriptMethod("sh")]
    private static ValueTask PaintShaderAsync(IContentStreamOperations target, in PostscriptValue name) =>
        target.PaintShaderAsync(name.AsPdfName());

    [PostscriptMethod("Tj")]
    private static ValueTask ShowStringAsync(IContentStreamOperations target, RentedMemorySource str) =>
        target.ShowStringAsync(str.Memory);

    [PostscriptMethod("'")]
    private static ValueTask
        MoveToNextLineAndShowStringAsync(IContentStreamOperations target, RentedMemorySource str) =>
        target.MoveToNextLineAndShowStringAsync(str.Memory);

    [PostscriptMethod(@"\""")]
    private static ValueTask MoveToNextLineAndShowString2Async(
        IContentStreamOperations target, double wordSpace, double charSpace, RentedMemorySource str) =>
        target.MoveToNextLineAndShowStringAsync(wordSpace, charSpace, str.Memory);

    [PostscriptMethod("TJ")]
    private static async ValueTask ShowSpacedStringAsync(
        IContentStreamOperations target, PostscriptValue arrayTop, OperandStack stack)
    {
        Debug.Assert(IsArrayTopMarker(arrayTop));
        var builder = target.GetSpacedStringBuilder();
        for (int i = 1; i < stack.Count; i++)
        {
            var item = stack[i];
            if (item.IsNumber)
                await builder.SpacedStringComponentAsync(item.Get<double>()).CA();
            else
            {
                using var text = item.Get<RentedMemorySource>();
                await builder.SpacedStringComponentAsync(text.Memory).CA();
            }
        }
        await builder.DoneWritingAsync().CA();
        stack.Clear();
    }

    [PostscriptMethod("Tf")]
    private static ValueTask SetFontAsync(
        IContentStreamOperations target, in PostscriptValue name, double size) =>
        target.SetFontAsync(name.AsPdfName(), size);

    [PostscriptMethod("DP")]
    private static ValueTask MarkedContentPoint2Async(
        IContentStreamOperations target, in PostscriptValue name, in PostscriptValue dictValue) =>
        dictValue.IsLiteralName
            ? target.MarkedContentPointAsync(name.AsPdfName(), dictValue.AsPdfName())
            : target.MarkedContentPointAsync(name.AsPdfName(), dictValue.Get<PdfDictionary>());

    [PostscriptMethod("BDC")]
    private static ValueTask BeginMarkedRange2Async(
        IContentStreamOperations target, in PostscriptValue name, in PostscriptValue dictValue) =>
        dictValue.IsLiteralName
            ? target.BeginMarkedRangeAsync(name.AsPdfName(), dictValue.AsPdfName())
            : target.BeginMarkedRangeAsync(name.AsPdfName(), dictValue.Get<PdfDictionary>());
}