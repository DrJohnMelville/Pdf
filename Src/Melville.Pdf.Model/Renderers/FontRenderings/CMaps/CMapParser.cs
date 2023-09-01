using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Postscript.Interpreter.FunctionLibrary;
using Melville.Postscript.Interpreter.InterpreterState;
using Melville.Postscript.Interpreter.Values;
using Melville.Postscript.Interpreter.Values.Composites;

namespace Melville.Pdf.Model.Renderers.FontRenderings.CMaps;

internal static class CMapParser
{
    private static readonly IPostscriptDictionary dict =
        PostscriptOperatorCollections.BaseLanguage().With(CmapParserOperations.AddOperations);
    
    public static async ValueTask<CMap> ParseCMapAsync(Stream source)
    {
       var ranges = new List<ByteRange>();
       var parser = new PostscriptEngine(dict){Tag = ranges};
       parser.ResourceLibrary.Put("ProcSet", "CIDInit", PostscriptValueFactory.CreateDictionary());
       parser.ErrorDict.Put("undefined"u8, PostscriptValueFactory.CreateNull());
       await parser.ExecuteAsync(source).CA();
       return new CMap(ranges);
    }

}

[TypeShortcut("Melville.Pdf.Model.Renderers.FontRenderings.CMaps.CMapFactory",
    "new Melville.Pdf.Model.Renderers.FontRenderings.CMaps.CMapFactory((System.Collections.Generic.IList<ByteRange>)engine.Tag)")]
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

}