using Melville.StreamInterfaces;
using Melville.StreamInterfaces.ParsingBuffers;
using System;
using System.Buffers;
using System.Runtime.CompilerServices;


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
        public bool TryGetUInt16BigEndian(out ushort value)
        {
            if (self.RemainingLength < 2)
            {
                value = 0;
                return false;
            }
            value = self.GetUInt16BigEndian();
            return true;
        }
    }
}
