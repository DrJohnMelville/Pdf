using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Postscript.Interpreter.FunctionLibrary;
using Melville.Postscript.Interpreter.InterpreterState;
using Melville.Postscript.Interpreter.Values;
using Melville.Postscript.Interpreter.Values.Execution;

namespace Melville.Pdf.LowLevel.Parsing.ContentStreams.EmbeddedImageParsing;

public static partial class ParsePdfArrays
{
    public static void EnablePdfArrayParsing(this PostscriptEngine engine)
    {
        engine.SystemDict.Put("["u8, PostscriptValueFactory.CreateMark());
        engine.SystemDict.Put("]"u8, PostscriptValueFactory.Create(CreatePdfArray.Instance));
    }
    public static void DisablePdfArrayParsing(this PostscriptEngine engine)
    {
        engine.SystemDict.Put("["u8, PostscriptValueFactory.Create(PostscriptOperators.Nop));
        engine.SystemDict.Put("]"u8, PostscriptValueFactory.Create(PostscriptOperators.Nop));
    }

    [StaticSingleton()]
    private partial class CreatePdfArray : BuiltInFunction
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
                PostscriptBuiltInOperations.PushArgument, 0));
        }
    }

    public static PdfObject ToPdfObject(in this PostscriptValue value) => value switch
    {
        { IsDouble: true } => new PdfDouble(value.Get<double>()),
        { IsInteger: true } => new PdfInteger(value.Get<long>()),
        { IsBoolean: true} => value.Get<bool>() ? PdfBoolean.True:PdfBoolean.False,
        var x when x.TryGet(out PdfObject? pdfObject) => pdfObject,
        _ => NameDirectory.Get(
            ExpandValueSynonym(value.Get<StringSpanSource>().GetSpan(
            stackalloc byte[PostscriptString.ShortStringLimit])))
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

internal readonly partial struct InlineImageParser2
{
    [FromConstructor] private readonly PostscriptEngine engine;
    [FromConstructor] private readonly IContentStreamOperations target;

    public async ValueTask ParseAsync()
    {
        var builder = PopDictionaryFromPostscriptStack();
        var strategy = EndSearchStrategyFactory.Create(builder);
        await target.DoAsync(builder.AsStream(await StreamDataAsync(strategy).CA())).CA();
    }

    private DictionaryBuilder PopDictionaryFromPostscriptStack()
    {
        Span<byte> nameSpan = stackalloc byte[PostscriptString.ShortStringLimit];
        Span<byte> valueSpan = stackalloc byte[PostscriptString.ShortStringLimit];
        var builder = DefaultImageDictionaryBuilder();
        while (engine.OperandStack.TryPop(out var last) && !last.IsMark)
        {
            var name = NameDirectory.Get(
                ExpandNameSynonym(engine.PopStringAsSpan(nameSpan)));

            builder.WithItem(name, last.ToPdfObject());
        }
        return builder;
    }

    private ReadOnlySpan<byte> ExpandNameSynonym(ReadOnlySpan<byte> name) => name switch
    {
        [(byte)'B', (byte)'P',(byte)'C']=> "BitsPerComponent"u8,
        [(byte)'C', (byte)'S']=> "ColorSpace"u8,
        [(byte)'D', (byte)'P']=> "DecodeParms"u8,
        [(byte)'I', (byte)'M']=> "ImageMask"u8,
        [(byte)'D']=> "Decode"u8,
        [(byte)'F']=> "Filter"u8,
        [(byte)'H']=> "Height"u8,
        [(byte)'I']=> "Interpolate"u8,
        [(byte)'L']=> "Length"u8,
        [(byte)'W']=> "Width"u8,
        _=> name

    };
 
    private DictionaryBuilder DefaultImageDictionaryBuilder() =>
        new DictionaryBuilder()
            .WithItem(KnownNames.Type, KnownNames.XObject)
            .WithItem(KnownNames.Subtype, KnownNames.Image);

    private async ValueTask<byte[]> StreamDataAsync(EndSearchStrategy strategy)
    {
        var reader = engine.TokenSource?.CodeSource ??
                     throw new PdfParseException("Did not have a token source to read from");
        while (true)
        {
            var result = await reader.ReadAsync().CA();
            if (strategy.SearchForEndSequence(
                    new SequenceReader<byte>(result.Buffer), result.IsCompleted, out var endPos))
            {
                var ret = CopyDataToArray(result, endPos);
                reader.AdvanceTo(endPos);
                return ret;
            }
            reader.AdvanceTo(result.Buffer.Start, result.Buffer.End);
        }
    }

    private static byte[] CopyDataToArray(ReadResult result, SequencePosition endPos) => 
        TrimBeginningAndEnd(result.Buffer.Slice(result.Buffer.Start, endPos)).ToArray();

    private static ReadOnlySequence<byte> TrimBeginningAndEnd(ReadOnlySequence<byte> streamData) => 
        streamData.Slice(1, streamData.Length - 3);
}