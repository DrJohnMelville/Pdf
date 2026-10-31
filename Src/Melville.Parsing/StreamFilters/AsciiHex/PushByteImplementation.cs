using System;

namespace Melville.Parsing.StreamFilters.AsciiHex
{
    public static class PushByteImplementation
    {
        extension<T>(ref Span<T> self)
        {
            /// <summary>
            /// Set the initial element to a value, then incremet the ref span beyond the written value.
            /// </summary>
            /// <param name="element">The element to add to the span</param>
            public void SetAndIncrement(T element)
            {
                self[0] = element;
                self = self[1..];
            }

            /// <summary>
            /// Set the initial element to a value, then incremet the ref span beyond the written value.
            /// </summary>
            /// <param name="element">The element to add to the span</param>
            /// <returns>True if the operation succeeds, false if the span was empty.</returns>
            public bool TrySetAndIncrement(T element)
            {
                if (self.Length is 0) return false;
                self.SetAndIncrement(element);
                return true;
            }
        }
    }
}