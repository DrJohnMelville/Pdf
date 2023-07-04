using System;
using System.Buffers;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Postscript.Interpreter.FunctionLibrary;
using Melville.Postscript.Interpreter.InterpreterState;
using Melville.Postscript.Interpreter.Values;

namespace Melville.Pdf.LowLevel.Parsing.ContentStreams.EmbeddedImageParsing;

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
            var name = NameDirectory.Get(engine.PopStringAsSpan(nameSpan));
            switch (last)
            {
                case { IsDouble: true }:
                    builder.WithItem(name, last.Get<double>());
                    break;
                case { IsInteger: true }:
                    builder.WithItem(name, last.Get<long>());
                    break;
                default:
                    builder.WithItem(name,
                        NameDirectory.Get(last.Get<StringSpanSource>().GetSpan(valueSpan)));
                    break;
            }
        }

        return builder;
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
                var resultSequence = result.Buffer.Slice(result.Buffer.Start, endPos);
                var ret = new byte[resultSequence.Length];
                resultSequence.CopyTo(ret.AsSpan());
                reader.AdvanceTo(endPos);
                return ret;
            }
            reader.AdvanceTo(result.Buffer.Start, result.Buffer.End);
        }
    }

}