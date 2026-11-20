using Melville.INPC;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Objects.StreamParts;
using Melville.Pdf.LowLevel.Model.Objects.StringEncodings;
using Melville.StreamInterfaces;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Melville.Pdf.Model.Documents;

/// <summary>
/// This record is the primary abstraction for a page in a pdf document.
/// </summary>
[FromConstructor]
public partial class PdfPage: HasRenderableContentStream
{
    /// <summary>
    /// Get the last modified time for a page from the page's dictionary
    /// </summary>
    /// <returns>The last modified time if it exists, null otherwise.</returns>
    public async ValueTask<PdfTime?> LastModifiedAsync()
    {
        return (await LowLevel.GetOrNullAsync(KnownNames.LastModified).CA()) is {IsName:false} str
            ? str.AsPdfTime()
            : null;
    }

    /// <summary>
    /// Gets a C# stream that represents the content stream for this page
    /// </summary>
    public override async ValueTask<IPdfStreamReader> GetContentBytesAsync() =>
        await LowLevel.GetOrNullAsync(KnownNames.Contents).CA() switch
        {
            var x when x.TryGet(out PdfStream? strm) => await strm.ReaderForContentAsync().CA(),
            var x when x.TryGet(out PdfArray? array) => await ContentStreamConcatReader.CreateAsync(array).CA(),
            var x => new PdfMemoryReader(Memory<byte>.Empty)
        };
}
public class ContentStreamConcatReader(Memory<IPdfStreamReader> readers) : IPdfStreamReader
{
    int currentIndex = 0;
    public void Dispose()
    {
        for (int i = currentIndex; i < readers.Length; i++)
        {
            readers.Span[i].Dispose();
        }
    }
    public int Read(Span<byte> buffer)
    {
        while (currentIndex < readers.Length)
        {
            var len = readers.Span[currentIndex].Read(buffer);
            if (len > 0) return len;
            readers.Span[currentIndex].Dispose();
            currentIndex++;
            if (currentIndex >= readers.Length) return 0;
            buffer[0] = (byte)'\n';
            return 1;
        }
        return 0;
    }
    public async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellation)
    {
        while (currentIndex < readers.Length)
        {
            var len = await readers.Span[currentIndex].ReadAsync(buffer, cancellation).CA();
            if (len > 0) return len;
            currentIndex++;
            if (currentIndex >= readers.Length) return 0;
            buffer.Span[0] = (byte)'\n';
            return 1;
        }
        return 0;
    }
    public static async ValueTask<IPdfStreamReader> CreateAsync(PdfArray array)
    {
        var readers = new IPdfStreamReader[array.Count];
        int index = 0;
        await foreach (var directObj in array)
        {
            if (directObj.TryGet<PdfStream>(out var stream))
                readers[index++] = await stream.ReaderForContentAsync().CA();
        }
        return new ContentStreamConcatReader(readers.AsMemory(0, index));
    }
}
