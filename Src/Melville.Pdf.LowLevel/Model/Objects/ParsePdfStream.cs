using Melville.Pdf.LowLevel.Filters.FilterProcessing;
using Melville.StreamInterfaces;
using Melville.StreamInterfaces.ParsingBuffers;
using System;
using System.Threading.Tasks;

namespace Melville.Pdf.LowLevel.Model.Objects
{
    /// <summary>
    /// Extension classes for creating a ParsingBuffer from a PdfStream.
    /// </summary>
    public static class ParsePdfStream
    {
        extension(PdfStream self)
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
}