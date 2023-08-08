using System;
using System.Collections.Generic;
using System.IO;
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
    public static async ValueTask WriteAsync(
        PdfObjectWriter innerWriter, PdfStream item,
        IObjectCryptContext encryptor)
    {
        await using var rawStream = await item.StreamContentAsync(StreamFormat.DiskRepresentation, encryptor).CA();
        var diskrep = await EnsureStreamHasKnownLengthAsync(rawStream).CA();
            
        DictionaryWriter.Write(innerWriter, 
            MergeDictionaryItems(item.RawItems, (KnownNames.Length, diskrep.Length)));
        innerWriter.Write(StreamToken);
        await innerWriter.CopyFromStreamAsync(diskrep).CA();
        innerWriter.Write(EndStreamToken);
    }

    private static IEnumerable<KeyValuePair<PdfDirectObject, PdfIndirectObject>> MergeDictionaryItems(
        IReadOnlyDictionary<PdfDirectObject, PdfIndirectObject> sources, params (PdfDirectObject Key, PdfIndirectObject Item)[] items)
    {
        foreach (var source in sources)
        {
            if (!items.Any(i => i.Key.Equals(source.Key))) yield return source;
        }

        foreach (var item in items)
        {
            yield return KeyValuePair.Create(item.Key, item.Item);
        }
    }

    private static async Task<Stream> EnsureStreamHasKnownLengthAsync(Stream rawStream)
    {
        if (rawStream.Length > 0) return rawStream;

        var mbs = new MultiBufferStream(2048);
        await rawStream.CopyToAsync(mbs).CA();
        return mbs.CreateReader();
    }
}