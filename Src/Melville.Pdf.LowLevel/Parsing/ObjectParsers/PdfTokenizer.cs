using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Model.ShortStrings;
using Melville.Pdf.LowLevel.Parsing.ParserContext;
using Melville.Postscript.Interpreter.Tokenizers;
using Melville.Postscript.Interpreter.Values;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading.Tasks;
namespace Melville.Pdf.LowLevel.Parsing.ObjectParsers;

#warning make this Disposable or make sure that the ParsingBuffer gets disposed.
internal readonly partial struct PdfTokenizer
{
    private readonly Tokenizer postScriptTokenizer;
    private readonly IAsyncEnumerator<PostscriptValue> enumerator;

    public PdfTokenizer(ParsingReader reader)
    {
        //            var asyncReader = new IByteSourceToIAsyncReadStream(reader.Reader);
        postScriptTokenizer = new Tokenizer(reader.Buffer);
        enumerator = postScriptTokenizer.TokensAsync().GetAsyncEnumerator();
    }
    public async ValueTask<PdfDirectObject> NextTokenAsync()
    {
        if (!await enumerator.MoveNextAsync()) return PdfDirectObject.CreateNull();
        return TranslatePostscriptObject(enumerator.Current);
    }
    PdfDirectObject TranslatePostscriptObject(in PostscriptValue current)
    {
        return current switch
        {
            { IsNull: true } or
            { IsNumber: true } or
            { IsString: true } => ToDirectValue(current),
            { IsLiteralName: true } => ParseDirectName(current),
            { IsExecutedName: true } => ParseExecutedName(current),
            _ => throw new PdfParseException($"Unknown PDF content stream token {current}")
        };
    }
    PdfDirectObject ParseDirectName(in PostscriptValue current)
    {
        return PdfNameTokenizer.Parse(current.ForceGet<StringSpanSource>().GetSpan());
    }
    private static PdfDirectObject ToDirectValue(PostscriptValue psNum) =>
        new(psNum.ValueStrategy, psNum.Memento);


    private static PdfDirectObject ParseExecutedName(in PostscriptValue current)
    {
        return specialNames.TryGetValue(current, out var value)?
            value : throw new PdfParseException($"Unknown token: {current}");

    }

    private static FrozenDictionary<PostscriptValue, PdfDirectObject> specialNames = CreateSpecialNames();
    static FrozenDictionary<PostscriptValue, PdfDirectObject> CreateSpecialNames() => 
        FrozenDictionary.Create<PostscriptValue, PdfDirectObject>(
            [
              CreateKey("true"u8, true),
              CreateKey("false"u8, false),
              CreateKey("obj"u8, PdfParsingCommand.ObjOperator),
              CreateKey("endobj"u8, PdfParsingCommand.EndObject),
              CreateKey("stream"u8, PdfParsingCommand.StreamOperator),
              CreateKey("endstream"u8, PdfParsingCommand.EndStreamOperator),
              CreateKey("<<"u8, PdfParsingCommand.PushMark),
              CreateKey(">>"u8, PdfParsingCommand.CreateDictionary),
              CreateKey("null"u8, PdfDirectObject.CreateNull()),
              CreateKey("["u8, PdfParsingCommand.PushMark),
              CreateKey("]"u8, PdfParsingCommand.CreateArray),
              CreateKey("R"u8, PdfParsingCommand.CreateReference),
        ]);
    static KeyValuePair<PostscriptValue, PdfDirectObject> CreateKey(ReadOnlySpan<byte> name, PdfDirectObject value)
    {
        return new KeyValuePair<PostscriptValue, PdfDirectObject>(
            PostscriptValueFactory.CreateString(name, StringKind.Name), value);
    }
}