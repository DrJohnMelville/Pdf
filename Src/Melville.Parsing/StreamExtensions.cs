using Melville.StreamInterfaces.ParsingBuffers;
using System;
using System.Buffers;

namespace Melville.Parsing;

/// <summary>
/// This is a temporary bridge class
/// </summary>
[Obsolete("This needs to go into Melville.StreamInterfaces")]
public static class StreamExtensnions
{
    extension(ParsingBuffer pb)
    {
    }

    extension(ref SpanParsingBuffer self)
    {
    }
}