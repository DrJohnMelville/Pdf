using Melville.StreamInterfaces;
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
        public async ValueTask<int> EnsureHasChar(SearchValues<byte> chars)
        {
            int index;
            do
            {
                index = pb.Peek().IndexOfAny(chars);
            } while (index < 0 && (await pb.ReadBytesAsync().CA()) > 0);
            return index;
        }
        public async ValueTask<int> EnsureHasNonChar(SearchValues<byte> chars)
        {
            int index;
            do
            {
                index = pb.Peek().IndexOfAnyExcept(chars);
            } while (index < 0 && (await pb.ReadBytesAsync().CA()) > 0);
            return index;
        }
    }

    extension(ref SpanParsingBuffer self)
    {
    }
}