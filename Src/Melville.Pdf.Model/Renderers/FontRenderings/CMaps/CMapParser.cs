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
       parser.ErrorDict.Put("undefined"u8, PostscriptValueFactory.CreateNull());
       await parser.ExecuteAsync(source).CA();
       return new CMap(ranges);
    }

}

[TypeShortcut("Melville.Pdf.Model.Renderers.FontRenderings.CMaps.CMapFactory",
    "new Melville.Pdf.Model.Renderers.FontRenderings.CMaps.CMapFactory((System.Collections.Generic.IList<ByteRange>)engine.Tag)")]
internal static partial class CmapParserOperations
{
    [PostscriptMethod("findresource")]
    private static PostscriptValue FindResource(in StringSpanSource name, in StringSpanSource category)
    {
        Debug.Assert(category.GetSpan().SequenceEqual("ProcSet"u8));
        Debug.Assert(name.GetSpan().SequenceEqual("CIDInit"u8));
        return PostscriptValueFactory.CreateSizedDictionary(1);
    }

    [PostscriptMethod("begincmap")]
    [PostscriptMethod("endcmap")]
    private static void NoOperation(){}

    [PostscriptMethod("begincodespacerange")]
    private static PostscriptValue BeginCodespaceRange(int rangeLength) =>
        PostscriptValueFactory.CreateMark();

    [PostscriptMethod("endcodespacerange")]
    private static void EndCodeSpaceRange(CMapFactory factory, OperandStack stack)
    {
//        factory.AddCodespaces(stack.CountToMark());
        
    }

}