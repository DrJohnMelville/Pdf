using Melville.Parsing.StreamFilters;
using System.Buffers;

namespace Melville.CCITT;

/// <summary>
/// This is a quasi-internal interface that is used so that the JBIG parser can reuse the MMR decoder from
/// the CCITT parser.
/// </summary>
public interface IJBigMmrFilter : IStreamFilterDefinition
{
    /// <summary>
    /// This will check the end of the stream for a CCITT termination code sequence.  This is needed because
    /// in the JBIG MMR decoder we have to consume the correct number of bits to remain in sync between the
    /// encoded and the decoder;
    /// </summary>
    /// <param name="source">the sequence we are currently reading from.</param>
    public void RequireTerminator(ref SequenceReader<byte> source);
}