using Melville.Pdf.LowLevel.Filters.FilterProcessing;
using Melville.StreamInterfaces;
using Melville.StreamInterfaces.ParsingBuffers;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Melville.Pdf.LowLevel.Model.Objects;

/// <summary>
/// Represents the byte content from a PDF Stream object.  PDF streams content all need to
/// ne disposed because they rent from the main PDF reader.  Many filters use parsing buffers that
/// also need to be disposed.  
/// </summary>
public interface IPdfStreamReader : IAsyncReader, IDisposable
{

}

internal class WrapPdfStream(Stream inner) : IPdfStreamReader
{
    public void Dispose() => inner.Dispose();

    public ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellation = default) =>
        inner.ReadAsync(buffer, cancellation);
}
/// <summary>
/// Extension classes for creating a ParsingBuffer from a PdfStream.
/// </summary>
public static class ParsePdfStream
{
    extension (PdfStream self)
    {
        /// <summary>
        /// Creates a parsing buffer for the content of this stream.  Disposing of the
        /// ParsingBuffer will dispose of all the mechanism to decode the stream.
        /// </summary>
        /// <param name="desiredFormat">Format in which to read the stream.</param>
        /// <param name="context">An optional context object for creating the decode filters.</param>
        /// <returns>A ParsingBuffer that will read from the given stream.</returns>
        public async ValueTask<ParsingBuffer> ContentParsingBufferAsync(
            StreamFormat desiredFormat = StreamFormat.PlainText, object? context = null)
        {
            IAsyncReader reader = await self.ReaderForContentAsync(StreamFormat.PlainText, context);
            return new ParsingBuffer(reader).WithDisposedDependency(reader);
        }
    }
}