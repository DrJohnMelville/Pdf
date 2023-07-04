using System;
using System.Buffers;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.SequenceReaders;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Parsing.ObjectParsers;

namespace Melville.Pdf.LowLevel.Parsing.ContentStreams.EmbeddedImageParsing;
[Obsolete("use the new one ")]
internal static class InlineImageParser
{
    public static async ValueTask<bool> ParseInlineImageAsync(BufferFromPipe bfp, ContentStreamContext target)
    {
        var dict = new DictionaryBuilder(await PdfParserParts.InlineImageDictionaryParser
            .ParseDictionaryItemsAsync(bfp.CreateParsingReader()).CA());
        SetTypeAsImage(dict);
        var endSearchStrategy = EndSearchStrategyFactory.Create(dict);
        bfp = await bfp.RefreshAsync().CA();
        SequencePosition endPos;
        while (!(endSearchStrategy.SearchForEndSequence(bfp, out endPos)))
        {
            bfp = await bfp.InvalidateAndRefreshAsync().CA();
        }

        await target.HandleInlineImageAsync(dict.AsStream(GrabStreamContent(bfp, endPos))).CA();
        return true;
    }

    private static void SetTypeAsImage(DictionaryBuilder dict) =>
        dict.WithItem(KnownNames.Type, KnownNames.XObject)
            .WithItem(KnownNames.Subtype, KnownNames.Image);

    private static byte[] GrabStreamContent(BufferFromPipe bfp, SequencePosition endPos)
    {
        var buffer = TrimInitialWhiteSpaceAndTerminalOperator(bfp, endPos);
        var data = CopeSequenceToBuffer(buffer);
        bfp.Consume(endPos);
        return data;
    }

    private static ReadOnlySequence<byte> TrimInitialWhiteSpaceAndTerminalOperator(
        in BufferFromPipe bfp, SequencePosition endPos) => bfp.Buffer
        // .Slice(bfp.Buffer.Start, endPos)
        .Slice(1..^2);

    private static byte[] CopeSequenceToBuffer(ReadOnlySequence<byte> buffer)
    {
        var data = new byte[buffer.Length];
        buffer.CopyTo(data);
        return data;
    }

}