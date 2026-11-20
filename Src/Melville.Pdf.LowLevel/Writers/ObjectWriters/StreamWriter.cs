using Melville.IntersectionTypes;
using Melville.Parsing.MultiplexSources;
using Melville.Parsing.Streams;
using Melville.Pdf.LowLevel.Filters.FilterProcessing;
using Melville.Pdf.LowLevel.Filters.JpxDecodeFilters;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Objects.StreamParts;
using Melville.StreamInterfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Melville.Pdf.LowLevel.Writers.ObjectWriters;

internal static class StreamWriter
{
    private static ReadOnlySpan<byte> StreamToken => " stream\r\n"u8;
    private static ReadOnlySpan<byte> EndStreamToken => "\r\nendstream"u8;
    public static async ValueTask WriteAsync(
        PdfObjectWriter innerWriter, PdfStream item,
        IObjectCryptContext encryptor)
    {
        var rawStream = await item.StreamEncryptedContentAsync(
            StreamFormat.DiskRepresentation, encryptor, null).CA();
        using var diskrep = await EnsureStreamHasKnownLengthAsync(rawStream).CA();
            
        DictionaryWriter.Write(innerWriter, 
            MergeDictionaryItems(item.RawItems, (KnownNames.Length, diskrep.Length)));
        innerWriter.Write(StreamToken);
        await innerWriter.CopyFromStreamAsync(diskrep.AsStreamInefficient()).CA();
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

    private static async Task<ReaderWithLength> EnsureStreamHasKnownLengthAsync(
        IPdfStreamReader rawStream)
    {
        if (ReaderWithLength.TryCreateFrom(rawStream, out var ret)) return ret;

        var (bytes, _) = await new AsyncReaderToByteArray(rawStream, false).ReadAsync().CA();
        rawStream.Dispose();
        return new PdfMemoryReader(bytes);
    }
}

[IntersectionType]
internal readonly partial struct ReaderWithLength: IAsyncReader, IStreamLength, IDisposable { }