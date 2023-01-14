using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Linq;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.Streams;
using Melville.Pdf.LowLevel.Filters.FilterProcessing;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Writers.ObjectWriters;

internal static class StreamWriter
{
    private static ReadOnlySpan<byte> StreamToken => " stream\r\n"u8;
    private static ReadOnlySpan<byte> EndStreamToken => "\r\nendstream"u8;
    public static async ValueTask<FlushResult> Write(
        PipeWriter target, PdfObjectWriter innerWriter, PdfStream item,
        IObjectCryptContext encryptor)
    {
        Stream diskrep;
        await using var rawStream = await item.StreamContentAsync(StreamFormat.DiskRepresentation, encryptor).CA();
        diskrep = await EnsureStreamHasKnownLength(rawStream).CA();
            
        await DictionaryWriter.WriteAsync(target, innerWriter, 
            MergeDictionaryItems(item.RawItems, (KnownNames.Length, new PdfInteger(diskrep.Length)))).CA();
        target.WriteBytes(StreamToken);
        await diskrep.CopyToAsync(target).CA();
        target.WriteBytes(EndStreamToken);
        return await target.FlushAsync().CA();
    }

    private static IEnumerable<KeyValuePair<PdfName, PdfObject>> MergeDictionaryItems(
        IEnumerable<KeyValuePair<PdfName, PdfObject>> sources, params (PdfName Key, PdfObject Item)[] items)
    {
        foreach (var source in sources)
        {
            if (!items.Any(i => i.Key ==  source.Key)) yield return source;
        }

        foreach (var item in items)
        {
            yield return KeyValuePair.Create(item.Key, item.Item);
        }
    }

    private static async Task<Stream> EnsureStreamHasKnownLength(Stream rawStream)
    {
        Stream diskrep;
        if (rawStream.Length < 1)
        {
            var mbs = new MultiBufferStream(2048);
            await rawStream.CopyToAsync(mbs).CA();
            diskrep = mbs.CreateReader();
        }
        else
        {
            diskrep = rawStream;
        }

        return diskrep;
    }
}