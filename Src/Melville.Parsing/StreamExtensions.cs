using Melville.StreamInterfaces.ParsingBuffers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Melville.Parsing;

[Obsolete("This needs to go into Melville.StreamInterfaces")]
public static class StreamExtensnions
{
    extension(ParsingBuffer pb)
    {
    }

    extension(ref SpanParsingBuffer self)
    {
 
        /// <summary>
        /// Try to get a byte out of the buffer, and advance if successful.
        /// </summary>
        /// <param name="value">The byte if successful, zero otherwise.</param>
        /// <returns>True if a byte is present, false otherwise.</returns>
        public bool TryGetUInt8(out byte value)
        {
            if (self.TryPeekUInt8(out value))
            {
                self.Advance(1);
                return true;
            }
            return false;
        }
        /// <summary>
        /// Try to get a byte out of the buffer, and advance if successful.
        /// </summary>
        /// <param name="value">The byte if successful, zero otherwise.</param>
        /// <returns>True if a byte is present, false otherwise.</returns>
        public bool TryPeekUInt8(out byte value)
        {
            if (self.RemainingLength < 1)
            {
                value = 0;
                return false;
            }
            value = self.Peek()[0];
            return true;
        }
    }
}