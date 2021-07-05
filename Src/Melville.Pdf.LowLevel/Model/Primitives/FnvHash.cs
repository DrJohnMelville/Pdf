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
            unchecked
            {
                var hash = offsetBasis;
                foreach (var item in bytes)
                {
                    hash = (hash * prime) ^ item;
                }

                return (int)hash;
            }
        }
    }
}