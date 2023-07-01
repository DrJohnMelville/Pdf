using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Postscript.Interpreter.FunctionLibrary;
using Melville.Postscript.Interpreter.InterpreterState;
using Melville.Postscript.Interpreter.Tokenizers;
using Melville.Postscript.Interpreter.Values;
using Melville.Postscript.Interpreter.Values.Execution;

namespace Melville.Pdf.LowLevel.Parsing.ContentStreams;

/// <summary>
/// Parses a content stream (expressed as a PipeReader) and "renders" it to an IContentStreamOperations.
/// </summary>
internal readonly partial struct ContentStreamParser2
{
    [FromConstructor] private readonly IContentStreamOperations target;

    /// <summary>
    /// Render the content stream operations in the given CodeSource pipereader.
    /// </summary>
    /// <param name="source">The content stream to parse.</param>
    public ValueTask ParseAsync(PipeReader source)
    {
        var engine = new PostscriptEngine();
        AddContentStreamOperatorsTo(engine);
        engine.Push(new PostscriptValue(
            target, PostscriptBuiltInOperations.PushArgument, 0));
        return engine.ExecuteAsync(new Tokenizer(source));
    }

    private void AddContentStreamOperatorsTo(PostscriptEngine engine)
    {
        ConfigureAsyncEngine(engine.SystemDict);
        ConfigureEngine(engine.SystemDict);
        CreateRequiredSynonyms(engine);
    }

    private static void CreateRequiredSynonyms(PostscriptEngine engine)
    {
        engine.SystemDict.Put("F", PostscriptValueFactory.Create(FillPath));
        engine.SystemDict.Put("[", PostscriptValueFactory.Create(PostscriptOperators.Nop));
        engine.SystemDict.Put("]", PostscriptValueFactory.Create(PostscriptOperators.Nop));
    }

    [MacroCode("""
        public static IExternalFunction ~0~ = new ~0~BuiltInFuncImpl();
        private sealed class ~0~BuiltInFuncImpl: BuiltInFunction
        {
            public override void Execute(PostscriptEngine engine, in PostscriptValue value)
            {
                ~1~
            }
        }

        """)]
    [MacroCode("    dict.Put(\"~2~\"u8, PostscriptValueFactory.Create(~0~));", 
        Prefix = "private void ConfigureEngine(IPostscriptComposite dict) \r\n{",
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
        int patternLen = engine.OperandStack.Count - 1;
        Span<double> pattern = stackalloc double[patternLen];
        engine.PopSpan(pattern);
        E(engine).SetLineDashPattern(pattern[^1], pattern[..^1]);
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
    private static IContentStreamOperations E(PostscriptEngine engine) =>
        engine.OperandStack[0].Get<IContentStreamOperations>();

    [MacroCode("""
        public static IExternalFunction ~0~ = new ~0~BuiltInFuncImpl();
        private sealed class ~0~BuiltInFuncImpl: AsyncBuiltInFunction
        {
            public override ValueTask ExecuteAsync(PostscriptEngine engine, in PostscriptValue value)
            {
                ~1~
            }
        }

        """)]
    [MacroCode("    dict.Put(\"~2~\"u8, PostscriptValueFactory.Create(~0~));",
        Prefix = "private void ConfigureAsyncEngine(IPostscriptComposite dict) \r\n{",
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
    partial void AsyncMacroHolder();
#if DEBUG
    private static ValueTask ScratchAsync(PostscriptEngine engine)
    {
        var name = TryPopName(engine);
        Span<double> color = stackalloc double[engine.OperandStack.Count - 1];
        engine.PopSpan(color);
        return E(engine).SetStrokeColorExtendedAsync(name, color);
    }
#endif
    private static PdfName? TryPopName(PostscriptEngine engine)
    {
        if (!engine.OperandStack.Peek().TryGet<StringSpanSource>(out var source))
            return null;

        engine.OperandStack.Pop();
        return NameDirectory.Get(source.GetSpan(
            stackalloc byte[PostscriptString.ShortStringLimit]));

    }
    private static PdfName PopName(PostscriptEngine engine) =>
        NameDirectory.Get(engine.PopAs<StringSpanSource>().GetSpan(
            stackalloc byte[PostscriptString.ShortStringLimit]));
}