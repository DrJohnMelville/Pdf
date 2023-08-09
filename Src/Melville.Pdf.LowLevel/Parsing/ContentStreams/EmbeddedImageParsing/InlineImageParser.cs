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

namespace Melville.Pdf.LowLevel.Parsing.ContentStreams.EmbeddedImageParsing;

internal readonly partial struct InlineImageParser
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
        return new PdfObjectCreator(engine.OperandStack, DictionaryTranslator.Image)
            .PopDictionaryBuilderFromStack(DefaultImageDictionaryBuilder());
    }

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