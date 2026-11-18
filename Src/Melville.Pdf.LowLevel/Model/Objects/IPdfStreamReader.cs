using Melville.Pdf.LowLevel.Filters.FilterProcessing;
using Melville.StreamInterfaces;
using Melville.StreamInterfaces.ParsingBuffers;
using Melville.StreamInterfaces.Streams;
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
public interface IPdfStreamReader : IAsyncReader, ISyncReader, IDisposable
{
#warning right now must implement ISyncReader, decide if this is a good idea long term.
}

internal class WrapPdfStream(Stream inner) : IPdfStreamReader {
   
    public void Dispose() => inner.Dispose();

    public int Read(Span<byte> buffer) => inner.Read(buffer);
 
    public ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellation = default) =>
        inner.ReadAsync(buffer, cancellation);
}

/// <summary>
/// Shim code for transitioning streams to readers.
/// </summary>
public static class ReaderStreamBridge
{
    extension (Stream self)
    {
        /// <summary>
        /// Shim code for transitioning streams to readers.
        /// </summary>
        [Obsolete("This is for the stream / reader transition only/")]
        public IPdfStreamReader AsReaderInefficient() => new WrapPdfStream(self);
       
    }

    extension (IAsyncReader self)
    {
        /// <summary>
        /// Shim code for transitioning streams to readers.
        /// </summary>
        [Obsolete("This is for the stream / reader transition only/")]
        public Stream AsStreamInefficient() => self.AsStream();
    }
}
