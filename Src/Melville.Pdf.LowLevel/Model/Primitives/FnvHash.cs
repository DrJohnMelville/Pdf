using System;

namespace Melville.Pdf.LowLevel.Model.Primitives
{
    // stolen from https://en.wikipedia.org/wiki/Fowler%E2%80%93Noll%E2%80%93Vo_hash_function
    public static class FnvHash
    {
        private const uint offsetBasis = 0x811c9dc5;
        private const uint prime = 0x01000193;
        public static int ComputeFnvHash( ReadOnlySpan<byte> bytes)
        {
                var hash = offsetBasis;
                foreach (var item in bytes)
                {
                    hash = SingleHashStep(hash, item);
                }
                unchecked
                {
                    return (int)hash;
                }
        }

        public static uint EmptyStringHash() => offsetBasis;
        public static uint SingleHashStep(uint hash, byte item)
        {
            unchecked
            {
                return (hash * prime) ^ item;
            }
        }
    }
}