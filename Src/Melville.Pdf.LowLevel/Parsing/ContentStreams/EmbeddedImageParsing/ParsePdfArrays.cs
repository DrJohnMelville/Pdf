using System;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Postscript.Interpreter.FunctionLibrary;
using Melville.Postscript.Interpreter.InterpreterState;
using Melville.Postscript.Interpreter.Values;
using Melville.Postscript.Interpreter.Values.Composites;
using Melville.Postscript.Interpreter.Values.Execution;

namespace Melville.Pdf.LowLevel.Parsing.ContentStreams.EmbeddedImageParsing;

internal static class ParsePdfArrays
{
    private static readonly IPostscriptDictionary ArrayParsingOperators =
        PostscriptValueFactory.CreateSizedDictionary(2)
            .Get<IPostscriptDictionary>()
            .With("["u8, PostscriptValueFactory.CreateMark())
            .With("]"u8, new CreatePdfArray());

    public static void EnablePdfArrayParsing(this PostscriptEngine engine) => 
        engine.DictionaryStack.Push(ArrayParsingOperators);

    public static void DisablePdfArrayParsing(this PostscriptEngine engine) => 
        engine.DictionaryStack.Pop();

    private class CreatePdfArray : BuiltInFunction
    {
        public override void Execute(PostscriptEngine engine, in PostscriptValue value)
        {
            var count = engine.OperandStack.CountToMark();
            var items = new PdfObject[count];
            for (int i = count - 1; i >= 0; i--)
            {
                items[i] = engine.OperandStack.Pop().ToPdfObject();
            }
            engine.OperandStack.Pop();
            engine.OperandStack.Push(new PostscriptValue(new PdfArray(items), 
                PostscriptBuiltInOperations.PushArgument, default));
        }
    }

    public static PdfObject ToPdfObject(in this PostscriptValue value) => value switch
    {
        { IsDouble: true } => new PdfDouble(value.Get<double>()),
        { IsInteger: true } => new PdfInteger(value.Get<long>()),
        { IsBoolean: true} => value.Get<bool>() ? PdfBoolean.True:PdfBoolean.False,
        var x when x.TryGet(out PdfObject? pdfObject) => pdfObject,
        _ => NameDirectory.Get(
            ExpandValueSynonym(value.Get<StringSpanSource>().GetSpan()))
    };
    private static ReadOnlySpan<byte> ExpandValueSynonym(ReadOnlySpan<byte> name) => name switch
    {
        [(byte)'A', (byte)'H',(byte)'x']=> "ASCIIHexDecode"u8,
        [(byte)'A', (byte)'8',(byte)'5']=> "ASCII85Decode"u8,
        [(byte)'L', (byte)'Z',(byte)'W']=> "LZWDecode"u8,
        [(byte)'C', (byte)'C',(byte)'F']=> "CCITTFaxDecode"u8,
        [(byte)'D', (byte)'C', (byte)'T'] => "DCTDecode"u8,
        [(byte)'R', (byte)'G', (byte)'B'] => "DeviceRGB"u8,
        [(byte)'C', (byte)'M', (byte)'Y', (byte)'K'] => "DeviceCMYK"u8,
        [(byte)'F', (byte)'l']=> "FlateDecode"u8,
        [(byte)'R', (byte)'L']=> "RunLengthDecode"u8,
        [(byte)'G']=> "DeviceGray"u8,
        [(byte)'I']=> "Indexed"u8,
        _=> name

    };
}