using System;
using System.IO.Pipelines;
using System.Threading.Tasks;

namespace Melville.Postscript.Interpreter.Tokenizers;

/// <summary>
/// This class allows pipes and memory objects to be read with a single interface, using
/// methods borrowed from PipeReader.  Some implementations of this interface support both
/// synchronous and asynchronous reads.
/// </summary>
public interface ICodeSource
{
    /// <summary>
    /// Retrieve data from the source, possibly reading from an external device
    /// </summary>
    /// <returns>A ReadResult containing a sequence with data to inspect</returns>
    ValueTask<ReadResult> ReadAsync();

    /// <summary>
    /// Read data from the internal buffer, if possible
    /// </summary>
    /// <returns>A ReadResult containing a sequence of the remainind data</returns>
    internal ReadResult Read();

    /// <summary>
    /// Mark data up to the given point as consumed.
    /// </summary>
    /// <param name="consumed">The first byte than has not been consumed.</param>
    void AdvanceTo(SequencePosition consumed);

    /// <summary>
    /// Mark data than has been consumed and examined.
    /// </summary>
    /// <param name="consumed">The first byte  not yet consumed by the reader.</param>
    /// <param name="examined">The first byte not yet examined by the reader.</param>
    void AdvanceTo(SequencePosition consumed, SequencePosition examined);
}