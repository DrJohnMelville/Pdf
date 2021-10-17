using System;

namespace Melville.Pdf.LowLevel.Model.Primitives
{
    // stolen from https://en.wikipedia.org/wiki/Fowler%E2%80%93Noll%E2%80%93Vo_hash_function
    public static class FnvHash
    {
        private const uint offsetBasis = 0x811c9dc5;
        private const uint prime = 0x01000193;

        public static uint HasStringAsLowerCase(string s)
        {
            var hash = offsetBasis;
            foreach (var character in s)
            {
                hash = SingleHashStep(hash, (byte)Char.ToLower(character));
            }

            return hash;
        }
        
        public static int FnvHashAsInt( ReadOnlySpan<byte> bytes)
        {
            unchecked
            {
                return (int)FnvHashAsUint(bytes);
            }
        }

        public static uint FnvHashAsUint(ReadOnlySpan<byte> bytes)
        {
            var hash = offsetBasis;
            foreach (var item in bytes)
            {
                hash = SingleHashStep(hash, item);
            }
            return hash;
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