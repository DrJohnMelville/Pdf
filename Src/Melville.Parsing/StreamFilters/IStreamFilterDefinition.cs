using System.Buffers;

namespace Melville.Parsing.StreamFilters;

/// <summary>
/// An interface that represents a filter that compresses or decompresses a stream.
/// </summary>
public interface IStreamFilterDefinition
{
    /// <summary>
    /// Convert a segment of a source stream to the output stream.
    /// </summary>
    /// <param name="source">A sequence reader with source data.</param>
    /// <param name="destination">A span of bytes to receive the compressed or decompressed bytes.</param>
    /// <returns>A tuple of the next byte to be consumed, number of bytes written to the destination, and a bool that can be set to true if
    /// the filter has generated all of the available output.</returns>
    (SequencePosition SourceConsumed, int bytesWritten, bool Done) Convert(
        ref SequenceReader<byte> source, in Span<byte> destination);

    /// <summary>
    /// Convert a segment of a source stream to the output stream.  This method implies there is no more input to process.
    /// </summary>
    /// <param name="source">A sequence reader with source data.</param>
    /// <param name="destination">A span of bytes to receive the compressed or decompressed bytes.</param>
    /// <returns>A tuple of the next byte to be consumed, number of bytes written to the destination, and a bool that can be set to true if
    /// the filter has generated all of the available output.</returns>
    (SequencePosition SourceConsumed, int bytesWritten, bool Done) FinalConvert(
        ref SequenceReader<byte> source, in Span<byte> destination) =>
        (source.Position, 0, true);

    /// <summary>
    /// Minimum size of a buffer that can be written to.
    /// </summary>
    int MinWriteSize => 1;
}