using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;

namespace Melville.Parsing.SequenceReaders;

/// <summary>
/// This interface can be implemented by a class that already has the data in a buffer.
/// This lets readers optionally reuse, reather than copying an existing buffer.
/// </summary>
public interface ISequenceSource
{
    /// <summary>
    /// The data for the complete stream (independent of current position) as a sequence.
    /// </summary>
    /// <returns></returns>
    ReadOnlySequence<byte> AsSequence();
}
