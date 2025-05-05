using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Parsing.ContentStreams;
using Melville.Postscript.Interpreter.FunctionLibrary;
using Melville.Postscript.Interpreter.InterpreterState;
using Melville.Postscript.Interpreter.Values;

namespace Melville.Pdf.Model.Renderers.FontRenderings.CMaps;

[TypeShortcut("Melville.Pdf.Model.Renderers.FontRenderings.CMaps.CMapFactory",
    "(Melville.Pdf.Model.Renderers.FontRenderings.CMaps.CMapFactory)engine.Tag")]
[TypeShortcut("Melville.Postscript.Interpreter.InterpreterState.PostscriptStack<Melville.Postscript.Interpreter.Values.PostscriptValue>.DelimitedStackSegment",
            "engine.OperandStack.SpanAboveMark()")]
internal static partial class CmapParserOperations
{
    [PostscriptMethod("begincmap")]
    [PostscriptMethod("endcmap")]
    private static void NoOperation(){}

    [PostscriptMethod("begincodespacerange")]
    [PostscriptMethod("beginnotdefrange")]
    [PostscriptMethod("begincidrange")]
    [PostscriptMethod("begincidchar")]
    [PostscriptMethod("beginbfrange")]
    [PostscriptMethod("beginbfchar")]
    private static PostscriptValue BeginRange(int rangeLength) =>
        PostscriptValueFactory.CreateMark();

    [PostscriptMethod("endcodespacerange")]
    private static void EndCodeSpaceRange(
        CMapFactory factory, PostscriptStack<PostscriptValue>.DelimitedStackSegment segment)
    {
        factory.AddCodespaces(segment.Span());
        segment.PopDataAndMark();
    }

    [PostscriptMethod("endnotdefrange")]
    private static void EndNotDefRange(
        CMapFactory factory, PostscriptStack<PostscriptValue>.DelimitedStackSegment segment)
    {
        factory.AddNotDefRanges(segment.Span());
        segment.PopDataAndMark();
    }

    [PostscriptMethod("endcidrange")]
    private static void EndCidRange(
        CMapFactory factory, PostscriptStack<PostscriptValue>.DelimitedStackSegment segment)
    {
        factory.AddCidRanges(segment.Span());
        segment.PopDataAndMark();
    }

    [PostscriptMethod("endcidchar")]
    private static void EndCidChar(
        CMapFactory factory, PostscriptStack<PostscriptValue>.DelimitedStackSegment segment)
    {
        factory.AddCidChars(segment.Span());
        segment.PopDataAndMark();
    }

    [PostscriptMethod("endbfrange")]
    private static void EndBaseFontRanges(
        CMapFactory factory, PostscriptStack<PostscriptValue>.DelimitedStackSegment segment)
    {
        factory.AddBaseFontRanges(segment.Span());
        segment.PopDataAndMark();
    }

    [PostscriptMethod("endbfchar")]
    private static void EndBaseFontChars(
        CMapFactory factory, PostscriptStack<PostscriptValue>.DelimitedStackSegment segment)
    {
        factory.AddBaseFontChars(segment.Span());
        segment.PopDataAndMark();
    }

    [PostscriptMethod("usecmap")]
    private static async ValueTask UseExternalCMapAsync(CMapFactory factory, PostscriptValue name) =>
        await factory.ReadFromPdfValueAsync(name.AsPdfName()).CA();

}