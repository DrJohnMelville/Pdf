using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.CharacterEncoding;
using Melville.Pdf.LowLevel.Parsing.ContentStreams;
using Melville.Pdf.Model.Renderers.FontRenderings.CharacterReaders;
using Melville.Postscript.Interpreter.FunctionLibrary;
using Melville.Postscript.Interpreter.InterpreterState;
using Melville.Postscript.Interpreter.Values;
using Melville.Postscript.Interpreter.Values.Composites;

namespace Melville.Pdf.Model.Renderers.FontRenderings.CMaps;

internal static class CMapParser
{
    private static readonly IPostscriptDictionary dict =
        PostscriptOperatorCollections.BaseLanguage().With(CmapParserOperations.AddOperations);
    
    public static async ValueTask<IReadCharacter> ParseCMapAsync(
        Stream source, IGlyphNameMap names, IReadCharacter innerFontReader, IRetrieveCmapStream library)
    {
       var ranges = new CMapFactory(names, innerFontReader, library);
       await ExecuteCmapDefinitionAsync(ranges, source).CA();
       return ranges.CreateCMap();
    }

    public static async ValueTask ExecuteCmapDefinitionAsync(
        CMapFactory ranges, Stream source)
    {
        var parser = new PostscriptEngine(dict) { Tag = ranges };
        parser.ResourceLibrary.Put("ProcSet", "CIDInit", PostscriptValueFactory.CreateDictionary());
        parser.ErrorDict.Put("undefined"u8, PostscriptValueFactory.CreateNull());
        await parser.ExecuteAsync(source).CA();
    }
}

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
    private static ValueTask UseExternalCMap(CMapFactory factory, PostscriptValue name) =>
        factory.UseOtherCMap(name.AsPdfName());

}