using Melville.StreamInterfaces.ParsingBuffers;
using System;
using System.Buffers;

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

        /// <summary>
        /// Advance to and consume the next index that complies with the accept parameter 
        /// </summary>
        /// <param name="accept">A search values of items to accept.</param>
        /// <param name="value">the found Character</param>
        /// <returns>true if compliant character is found, false otherwise</returns>
        public bool TryGetNext(SearchValues<byte> accept, out byte value)
        {
            if (self.TryAdvanceTo(accept))
            {
                value = self.GetUInt8();
                return true;
            }
            else
            {
                value = 0;
                return false;
            }
        }
        /// <summary>
        /// Advance to and consume the next index that does not match the exclude parameter 
        /// </summary>
        /// <param name="exclude">A search values of items to exclude.</param>
        /// <param name="value">the found Character</param>
        /// <returns>true if compliant character is found, false otherwise</returns>
        public bool TryGetNextExcept(SearchValues<byte> exclude, out byte value)
        {

            if (self.TryAdvanceToExcept(exclude))
            {
                value = self.GetUInt8();
                return true;
            }
            else
            {
                value = 0;
                return false;
            }
        }

        /// <summary>
        /// Attempt to the next item matching the accept parameter.  Does not move if no item found
        /// </summary>
        /// <param name="accept">SearchVales describing items to accept</param>
        /// <returns>True if the advance was successful false if not.</returns>
        public bool TryAdvanceTo(SearchValues<byte> accept)
        {
            var index = self.Peek().IndexOfAny(accept);
            if (index < 0) return false;
            self.Advance(index);
            return true;
        }
        /// <summary>
        /// Attempt to the next item matching the accept parameter.  Does not move if no item found
        /// </summary>
        /// <param name="accept">SearchVales describing items to accept</param>
        /// <returns>True if the advance was successful false if not.</returns>
        public bool TryAdvanceToExcept(SearchValues<byte> exclude)
        {
            var index = self.Peek().IndexOfAnyExcept(exclude);
            if (index < 0) return false;
            self.Advance(index);
            return true;
        }
    }
}