
using Melville.Parsing.CountingReaders;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Parsing.ParserContext;

using Melville.Postscript.Interpreter.Tokenizers;
using Melville.Postscript.Interpreter.Values;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Transactions;

namespace Melville.Pdf.LowLevel.Parsing.ObjectParsers
{

#warning make this Disposable or make sure that the ParsingBuffer gets disposed.
    internal readonly partial struct PdfTokenizer2
    {
        private readonly Tokenizer postScriptTokenizer;
        private readonly IAsyncEnumerator<PostscriptValue> enumerator;

        public PdfTokenizer2(ParsingReader reader)
        {
            var asyncReader = new IByteSourceToIAsyncReadStream(reader.Reader);
            postScriptTokenizer = new Tokenizer(asyncReader);
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
                { IsNull: true} or
                { IsNumber: true} or 
                { IsString: true} => ToDirectValue(current),
                { IsLiteralName: true} => ParseDirectName(current),
                { IsExecutedName: true} => ParseExecutedName(current)
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
#warning this is unbelivably wasteful
            return current.ToString() switch
            {
                "true" => true,
                "false" => false,
                "obj" => PdfParsingCommand.ObjOperator,
                "endobj" => PdfParsingCommand.EndObject,
                "stream" => PdfParsingCommand.StreamOperator,
                "endstream" => PdfParsingCommand.EndStreamOperator,
                "<<" => PdfParsingCommand.PushMark,
                ">>" => PdfParsingCommand.CreateDictionary,
                "null" => PdfDirectObject.CreateNull(),
                "[" => ((PdfDirectObject)PdfParsingCommand.PushMark),
                "]" => ((PdfDirectObject)PdfParsingCommand.CreateArray),
                var x => throw new PdfParseException($"Unknown token {x}")
            };
        }
    }
}
