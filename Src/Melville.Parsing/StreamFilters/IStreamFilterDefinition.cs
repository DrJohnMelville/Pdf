using Melville.StreamInterfaces;
using Melville.StreamInterfaces.Adapters;
using Melville.StreamInterfaces.ParsingBuffers;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Buffers;
using System.Reflection;

namespace Melville.Parsing.StreamFilters;


#warning get rid of this when we are done with the sequencereader.
/// <summary>
/// This is a temporaruy for shimming
/// </summary>

/// <summary>
/// An interface that represents a filter that compresses or decompresses a stream.
/// </summary>
public interface IStreamFilterDefinition
{

    /// <summary>
    /// Convert source bytes to an output buffer.
    /// 
    /// Both parameters are ref parameters.  Advancing the input buffer indicates that those
    /// bytes have been fully consumed.  The output variable should be left at the
    /// first bute of the buffer passed in that is not filled.
    /// </summary>
    /// <param name="input">The sourece data.</param>
    /// <param name="output">The span to which the output should be routed.</param>
    /// <returns>True if the filter has no more data to provide, false otherwise.</returns>
    bool Convert(ref SpanParsingBuffer input, ref Span<byte> output);

    /// <summary>
    /// Convert source bytes to an output buffer.
    /// 
    /// Both parameters are ref parameters.  Advancing the input buffer indicates that those
    /// bytes have been fully consumed.  The output variable should be left at the
    /// first bute of the buffer passed in that is not filled.
    /// </summary>
    /// <param name="input">The sourece data.</param>
    /// <param name="output">The span to which the output should be routed.</param>
    /// <returns>True if the filter has no more data to provide, false otherwise.</returns>
    bool FinalConvert(ref SpanParsingBuffer input, ref Span<byte> output);
}

