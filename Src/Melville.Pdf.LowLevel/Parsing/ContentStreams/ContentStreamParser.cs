using System;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Parsing.ContentStreams.EmbeddedImageParsing;
using Melville.Postscript.Interpreter.FunctionLibrary;
using Melville.Postscript.Interpreter.InterpreterState;
using Melville.Postscript.Interpreter.Tokenizers;
using Melville.Postscript.Interpreter.Values;
using Melville.Postscript.Interpreter.Values.Composites;
using Melville.Postscript.Interpreter.Values.Execution;
using Melville.Postscript.Interpreter.Values.Numbers;
using PdfDirectValue = Melville.Pdf.LowLevel.Model.Objects.PdfDirectValue;
using PdfIndirectValue = Melville.Pdf.LowLevel.Model.Objects.PdfIndirectValue;

namespace Melville.Pdf.LowLevel.Parsing.ContentStreams;

[StaticSingleton]
internal sealed partial class ArrayTopMarker
{
    public override string ToString() => "Top of Array Declaration";
}

/// <summary>
/// Parses a content stream (expressed as a PipeReader) and "renders" it to an IContentStreamOperations.
/// </summary>
public readonly partial struct ContentStreamParser
{
    /// <summary>
    /// The target that we are parsing the content stream to.
    /// </summary>
    [FromConstructor] private readonly IContentStreamOperations target;

    /// <summary>
    /// Render the content stream operations in the given CodeSource pipereader.
    /// </summary>
    /// <param name="source">The content stream to parse.</param>
    public async ValueTask ParseAsync(PipeReader source)
    {
        var engine = new PostscriptEngine(contentStreamCommands);
        engine.Push(new PostscriptValue(
            target, PostscriptBuiltInOperations.PushArgument, default));
        await engine.ExecuteAsync(new Tokenizer(source)).CA();
        if (engine.ErrorData.TryGetAs("newerror", out bool value) && value)
            throw new PdfParseException("Error parsing content stream;");
    }

    private static readonly IPostscriptDictionary contentStreamCommands =
        AddContentStreamOperatorsTo();


    private static IPostscriptDictionary AddContentStreamOperatorsTo()
    {
        var ret = PostscriptValueFactory
            .CreateSizedDictionary(80)
            .Get<IPostscriptDictionary>();
        ConfigureAsyncEngine(ret);
        ConfigureEngine(ret);
        CreateRequiredSynonyms(ret);
        return ret;
    }

    private static void CreateRequiredSynonyms(IPostscriptComposite dict)
    {
        dict.Put("true"u8, PostscriptValueFactory.Create(true));
        dict.Put("false"u8, PostscriptValueFactory.Create(false));
        dict.Put("F"u8, dict.Get("f"));
        dict.Put("["u8, PostscriptValueFactory.CreateMark());
        dict.Put("]"u8, new PostscriptValue(ArrayTopMarker.Instance,
            PostscriptBuiltInOperations.PushArgument, default));
        dict.Put("<<"u8, PostscriptValueFactory.CreateMark());
        dict.Put("$IgnoreCount"u8, PostscriptValueFactory.Create(0));
    }

    [MacroCode("""
        private sealed class ~0~BuiltInFuncImpl: BuiltInFunction
        {
            public override void Execute(PostscriptEngine engine, in PostscriptValue value)
            {
                ~1~
            }
        }

        """)]
    [MacroCode("    dict.Put(\"~2~\"u8, PostscriptValueFactory.Create(new ~0~BuiltInFuncImpl()));", 
        Prefix = "private static void ConfigureEngine(IPostscriptComposite dict) \r\n{",
        Postfix = "}")]
    [MacroItem("CloseFillAndStrokePath", "E(engine).CloseFillAndStrokePath();", "b")]
    [MacroItem("CloseFillAndStrokePathEvenOdd", "E(engine).CloseFillAndStrokePathEvenOdd();", "b*")]
    [MacroItem("FillAndStrokePath", "E(engine).FillAndStrokePath();", "B")]
    [MacroItem("FillAndStrokePathEvenOdd", "E(engine).FillAndStrokePathEvenOdd();", "B*")]
    [MacroItem("FillPath", "E(engine).FillPath();", "f")]
    [MacroItem("FillPathEvenOdd", "E(engine).FillPathEvenOdd();", "f*")]
    [MacroItem("EndPathWithNoOp", "E(engine).EndPathWithNoOp();", "n")]
    [MacroItem("CloseAndStrokePath", "E(engine).CloseAndStrokePath();", "s")]
    [MacroItem("StrokePath", "E(engine).StrokePath();", "S")]
    [MacroItem("ClipToPath", "E(engine).ClipToPath();", "W")]
    [MacroItem("ClipToPathEvenOdd", "E(engine).ClipToPathEvenOdd();", "W*")]
    [MacroItem("ClosePath", "E(engine).ClosePath();", "h")]
    [MacroItem("SaveGraphicsState", "E(engine).SaveGraphicsState();", "q")]
    [MacroItem("RestoreGraphicsState", "E(engine).RestoreGraphicsState();", "Q")]
    [MacroItem("CurveTo", """
        Span<double> a = stackalloc double[6];
        engine.PopSpan(a);
        E(engine).CurveTo(a[0], a[1], a[2], a[3], a[4], a[5]);
        """, "c")]
    [MacroItem("CurveToWithoutInitialControl", """
        Span<double> a = stackalloc double[4];
        engine.PopSpan(a);
        E(engine).CurveToWithoutInitialControl(a[0], a[1], a[2], a[3]);
        """, "v")]
    [MacroItem("CurveToWithoutFinalControl", """
        Span<double> a = stackalloc double[4];
        engine.PopSpan(a);
        E(engine).CurveToWithoutFinalControl(a[0], a[1], a[2], a[3]);
        """, "y")]
    [MacroItem("Rectangle", """
        Span<double> a = stackalloc double[4];
        engine.PopSpan(a);
        E(engine).Rectangle(a[0], a[1], a[2], a[3]);
        """, "re")]
    [MacroItem("MoveTo", """
        engine.PopAs(out double x, out double y);
        E(engine).MoveTo(x,y);
        """, "m")]
    [MacroItem("LineTo", """
        engine.PopAs(out double x, out double y);
        E(engine).LineTo(x,y);
        """, "l")]
    [MacroItem("SetLineCap", """
        E(engine).SetLineCap((LineCap)engine.PopAs<long>());
        """, "J")]
    [MacroItem("SetLineJoin", """
        E(engine).SetLineJoinStyle((LineJoinStyle)engine.PopAs<long>());
        """, "j")]
    [MacroItem("SetLineWidth", """
        E(engine).SetLineWidth(engine.PopAs<double>());
        """, "w")]
    [MacroItem("SetMiterLimit", """
        E(engine).SetMiterLimit(engine.PopAs<double>());
        """, "M")]
    [MacroItem("ModifyTransformMatrix", """
        Span<float> a = stackalloc float[6];
        engine.PopSpan(a);
        E(engine).ModifyTransformMatrix(new System.Numerics.Matrix3x2(a[0], a[1], a[2], a[3], a[4], a[5]));
        """, "cm")]
    [MacroItem("SetFlatness", """
        E(engine).SetFlatnessTolerance(engine.PopAs<double>());
        """, "i")]
    [MacroItem("SetRenderingIntent", """
        E(engine).SetRenderIntent(new RenderIntentName(PopName(engine)));
        """, "ri")]
    [MacroItem("SetLineDashPattern", """
        var phase = engine.PopAs<double>();
        var arrayTop = engine.OperandStack.Pop();
        Debug.Assert(IsArrayTopMarker(arrayTop));
        var patternLen = engine.OperandStack.CountToMark();
        Span<double> pattern = stackalloc double[patternLen];
        engine.PopSpan(pattern);
        E(engine).SetLineDashPattern(phase, pattern);
        PopMarkObject(engine);
 """, "d")]
    [MacroItem("SetStrokingColor", """
        int patternLen = engine.OperandStack.Count - 1;
        Span<double> pattern = stackalloc double[patternLen];
        engine.PopSpan(pattern);
        E(engine).SetStrokeColor(pattern);
        """, "SC")]
    [MacroItem("SetNonstrokingColor", """
        int patternLen = engine.OperandStack.Count - 1;
        Span<double> pattern = stackalloc double[patternLen];
        engine.PopSpan(pattern);
        E(engine).SetNonstrokingColor(pattern);
        """, "sc")]
    [MacroItem("SetColoredGlyphMetric", """
        engine.PopAs(out double d1, out double d2);
        E(engine).SetColoredGlyphMetrics(d1,d2);
        """, "d0")]
    [MacroItem("SetUncoloredGlyphMetric", """
        engine.PopAs(out double d3, out double d4, out double d5, out double d6);
        engine.PopAs(out double d1, out double d2);
        E(engine).SetUncoloredGlyphMetrics(d1,d2,d3,d4,d5,d6);
        """, "d1")]
    [MacroItem("BeginText", """
        E(engine).BeginTextObject();
        """, "BT")]
    [MacroItem("EndText", """
        E(engine).EndTextObject();
        """, "ET")]
    [MacroItem("MoveTextPosition", """
        engine.PopAs(out double x, out double y);
        E(engine).MovePositionBy(x,y);
        """, "Td")]
    [MacroItem("MoveTextPositionWithLeading", """
        engine.PopAs(out double x, out double y);
        E(engine).MovePositionByWithLeading(x,y);
        """, "TD")]
    [MacroItem("MoveToNextLine", """
        E(engine).MoveToNextTextLine();
        """, "T*")]
    [MacroItem("SetCharSpace", """
        E(engine).SetCharSpace(engine.PopAs<double>());
        """, "Tc")]
    [MacroItem("SetTextLeading", """
        E(engine).SetTextLeading(engine.PopAs<double>());
        """, "TL")]
    [MacroItem("SetTextRise", """
        E(engine).SetTextRise(engine.PopAs<double>());
        """, "Ts")]
    [MacroItem("SetWordSpace", """
        E(engine).SetWordSpace(engine.PopAs<double>());
        """, "Tw")]
    [MacroItem("SetHorizontalTextScaling", """
        E(engine).SetHorizontalTextScaling(engine.PopAs<double>());
        """, "Tz")]
    [MacroItem("SetTextRender", """
        E(engine).SetTextRender((TextRendering)engine.PopAs<long>());
        """, "Tr")]
    [MacroItem("SetTextMatrix", """
        Span<float> a = stackalloc float[6];
        engine.PopSpan(a);
        E(engine).SetTextMatrix(a[0], a[1], a[2], a[3], a[4], a[5]);
        """, "Tm")]
    [MacroItem("MarkedContentPoint", """
        E(engine).MarkedContentPoint(PopName(engine));
        """, "MP")]
    [MacroItem("BeginMarkedRange", """
        E(engine).BeginMarkedRange(PopName(engine));
        """, "BMC")]
    [MacroItem("EndMarkedRange", """
        E(engine).EndMarkedRange();
        """, "EMC")]
    [MacroItem("CreateDictionary", """
        CreatePdfDictionary(engine);
        """, ">>")]
    [MacroItem("BeginCompat", """
        E(engine).BeginCompatibilitySection();
        if (IncrementIgnoreCount(engine, 1) != 1) return;
        engine.ErrorDict.Put("undefined"u8, PostscriptValueFactory.Create(IgnoreError.Instance));
        """, "BX")]
    [MacroItem("EndCompat", """
        E(engine).EndCompatibilitySection();
        if (IncrementIgnoreCount(engine, -1) != 0) return;
        engine.ErrorDict.Undefine("undefined"u8);
        """, "EX")]
    [MacroItem("BeginInlineImage", """
        engine.Push(PostscriptValueFactory.CreateMark());
        engine.EnablePdfArrayParsing();
        """, "BI")]
    private static IContentStreamOperations E(PostscriptEngine engine) =>
        engine.OperandStack[0].Get<IContentStreamOperations>();

    [MacroCode("""
        private  sealed class ~0~BuiltInFuncImpl: AsyncBuiltInFunction
        {
            public override ValueTask ExecuteAsync(PostscriptEngine engine, in PostscriptValue value)
            {
                ~1~
            }
        }

        """)]
    [MacroCode("    dict.Put(\"~2~\"u8, PostscriptValueFactory.Create(new ~0~BuiltInFuncImpl()));",
        Prefix = "private static void ConfigureAsyncEngine(IPostscriptComposite dict) \r\n{",
        Postfix = "}")]
    [MacroItem("Do", """
                return E(engine).DoAsync(PopName(engine));
        """, "Do")]
    [MacroItem("LoadGraphicStateDictionary", """
               return E(engine).LoadGraphicStateDictionaryAsync(PopName(engine));
        """, "gs")]
    [MacroItem("SetNonstrokingColorSpace", """
               return E(engine).SetNonstrokingColorSpaceAsync(PopName(engine));
        """, "cs")]
    [MacroItem("SetStrokingColorSpace", """
               return E(engine).SetStrokingColorSpaceAsync(PopName(engine));
        """, "CS")]
    [MacroItem("SetStrokingCmyk", """
        engine.PopAs(out double c, out double m, out double y, out double k);
               return E(engine).SetStrokeCMYKAsync(c,m,y,k);
        """, "K")]
    [MacroItem("SetsonStrokingCmyk", """
        engine.PopAs(out double c, out double m, out double y, out double k);
               return E(engine).SetNonstrokingCMYKAsync(c,m,y,k);
        """, "k")]
    [MacroItem("SetStrokingRgb", """
        engine.PopAs(out double r, out double g, out double b);
               return E(engine).SetStrokeRGBAsync(r,g,b);
        """, "RG")]
    [MacroItem("SetNonStrokingRgb", """
        engine.PopAs(out double r, out double g, out double b);
                       return E(engine).SetNonstrokingRgbAsync(r,g,b);
        """, "rg")]
    [MacroItem("SetStrokingGray", """
               return E(engine).SetStrokeGrayAsync(engine.PopAs<double>());
        """, "G")]
    [MacroItem("SetNonStrokingGray", """
                       return E(engine).SetNonstrokingGrayAsync(engine.PopAs<double>());
        """, "g")]
    [MacroItem("SetNonStrokingExtended", """
        var name = TryPopName(engine);
        Span<double> color = stackalloc double[engine.OperandStack.Count - 1];
        engine.PopSpan(color);
        return E(engine).SetNonstrokingColorExtendedAsync(name, color);
        """, "scn")]
    [MacroItem("SetStrokingExtended", """
        var name = TryPopName(engine);
        Span<double> color = stackalloc double[engine.OperandStack.Count - 1];
        engine.PopSpan(color);
        return E(engine).SetStrokeColorExtendedAsync(name, color);
        """, "SCN")]
    [MacroItem("PaintShader", """
        return E(engine).PaintShaderAsync(PopName(engine));
        """, "sh")]
    [MacroItem("ShowString", """
        return ShowStringAsync(engine);
        """, "Tj")]
    [MacroItem("MoveToNextLineAndShowString", """
        return MoveToNextLineAndShowStringAsync(engine);
        """, @"\'")]
    [MacroItem("MoveToNextLineAndShowString2", """
        return MoveToNextLineAndShowString2Async(engine);
        """, @"\""")]
    [MacroItem("ShowSpacedString", """
        return ShowSpacedStringAsync(engine);
        """, @"TJ")]
    [MacroItem("SetFont", """
        var size = engine.PopAs<double>();
        var name = PopName(engine);
        return E(engine).SetFontAsync(name, size);
        """, "Tf")]
    [MacroItem("MarkedContentPoint2", """
        var dictValue = engine.OperandStack.Pop();
        var name = PopName(engine);
        return dictValue.IsLiteralName
            ? E(engine).MarkedContentPointAsync(
                name, MapSimpleValue(dictValue))
            : E(engine).MarkedContentPointAsync(name, dictValue.Get<PdfValueDictionary>());
        """, "DP")]
    [MacroItem("BeginMarkedRange2", """
        var dictValue = engine.OperandStack.Pop();
        var name = PopName(engine);
        return dictValue.IsLiteralName
            ? E(engine).BeginMarkedRangeAsync(
                name, MapSimpleValue(dictValue))
            : E(engine).BeginMarkedRangeAsync(name, dictValue.Get<PdfValueDictionary>());
        """, "BDC")]
    [MacroItem("ParseMarkedImage", """
        engine.DisablePdfArrayParsing();
        return new InlineImageParser(engine, E(engine)).ParseAsync();
        """, "ID")]
    partial void AsyncMacroHolder();
#if DEBUG
    private static ValueTask ScratchAsync(PostscriptEngine engine)
    {
        var phase = engine.PopAs<double>();
        var arrayTop = engine.OperandStack.Pop();
        Debug.Assert(IsArrayTopMarker(arrayTop));
        var patternLen = engine.OperandStack.CountToMark();
        Span<double> pattern = stackalloc double[patternLen];
        engine.PopSpan(pattern);
        E(engine).SetLineDashPattern(phase, pattern);
        return ValueTask.CompletedTask;
    }
#endif

    private static int IncrementIgnoreCount(PostscriptEngine engine, int increment)
    {
        var ret = engine.SystemDict.GetAs<long>("$IgnoreCount"u8);
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


    private static void CreatePdfDictionary(PostscriptEngine engine)
    {
        var builder = new ValueDictionaryBuilder();
        while (engine.OperandStack.TryPop(out var item) && item is { IsMark: false })
        {
            if (IsArrayTopMarker(item))
            {
                item = CreatePdfArray(engine);
            }
            var key = PopName(engine);
            builder.WithItem(key, PostscriptObjectToPdfObject(item));
        }
        engine.OperandStack.Push(new PostscriptValue(builder.AsDictionary(), PostscriptBuiltInOperations.PushArgument,default));
    }

    private static bool IsArrayTopMarker(PostscriptValue item) => 
        item.TryGet(out ArrayTopMarker _);

    private static PostscriptValue CreatePdfArray(PostscriptEngine engine)
    {
        var ret = new PdfIndirectValue[engine.OperandStack.CountToMark()];
        for (int i = ret.Length - 1; i >= 0; i--)
        {
            ret[i] = PostscriptObjectToPdfObject(engine.OperandStack.Pop());
        }

        PopMarkObject(engine);
        return new PostscriptValue(new PdfValueArray(ret),
            PostscriptBuiltInOperations.PushArgument, default);
    }

    private static void PopMarkObject(PostscriptEngine engine)
    {
        var mark = engine.OperandStack.Pop();
        Debug.Assert(mark.IsMark);
    }


    private static PdfDirectValue PostscriptObjectToPdfObject(PostscriptValue item) => item switch
    {
        { IsInteger: true } or
        { IsDouble: true } or
        { IsString: true } or
        { IsBoolean: true } or
        {IsLiteralName:true} => MapSimpleValue(item),
        var x when x.TryGet(out PdfValueDictionary? innerDictionary) => innerDictionary,
        var x when x.TryGet(out PdfValueArray? innerArray) => innerArray,
        _=> throw new PdfParseException("Cannot convert PostScriptToPdfObject")
    };

    private static async ValueTask ShowSpacedStringAsync(PostscriptEngine engine)
    {
        var arrayTop = engine.OperandStack.Pop();
        Debug.Assert(IsArrayTopMarker(arrayTop));
        var builder = E(engine).GetSpacedStringBuilder();
        for (int i = 2; i < engine.OperandStack.Count; i++)
        {
            var item = engine.OperandStack[i];
            if (item.IsNumber)
                await builder.SpacedStringComponentAsync(item.Get<double>()).CA();
            else
            {
                using var text = item.Get<RentedMemorySource>();
                await builder.SpacedStringComponentAsync(text.Memory).CA();
            }
        }
        await builder.DoneWritingAsync().CA();
        while (engine.OperandStack.Count > 1) engine.OperandStack.Pop();
    }
    private static async ValueTask ShowStringAsync(PostscriptEngine engine)
    {
        using var strSource = engine.PopAs<RentedMemorySource>();
        await E(engine).ShowStringAsync(strSource.Memory).CA();
    }

    private static async ValueTask MoveToNextLineAndShowStringAsync(PostscriptEngine engine)
    {
        using var strSource = engine.PopAs<RentedMemorySource>();
        await E(engine).MoveToNextLineAndShowStringAsync(strSource.Memory).CA();
    }
    private static async ValueTask MoveToNextLineAndShowString2Async(PostscriptEngine engine)
    {
        using var strSource = engine.PopAs<RentedMemorySource>();
        engine.PopAs(out double wordSpace, out double charSpace);
        await E(engine).MoveToNextLineAndShowStringAsync(
            wordSpace, charSpace, strSource.Memory).CA();
    }

    private static PdfDirectValue? TryPopName(PostscriptEngine engine)
    {
        var posName = engine.OperandStack.Peek();
        if (!posName.IsLiteralName) return null;
        engine.OperandStack.Pop();
        return MapSimpleValue(posName);
    }

    private static PdfDirectValue PopName(PostscriptEngine engine) =>
        TryPopName(engine) ?? throw new PdfParseException("Name exoected");

    private static PdfDirectValue MapSimpleValue(in PostscriptValue source)
    {
        Debug.Assert(source.ValueStrategy is PostscriptString or PostscriptDouble or PostscriptBoolean or PostscriptInteger);
        return new PdfDirectValue(source.ValueStrategy, source.Memento);
    }
}