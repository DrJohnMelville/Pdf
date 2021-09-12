using System;
using System.Security.Cryptography;

namespace Melville.Pdf.LowLevel.Encryption.Cryptography
{
    public class RC4
    {
        public RC4()
        {
            SBox = new byte[SBoxLength];
        }
        public RC4(in ReadOnlySpan<byte> rgbKey): this()
        {
            ResetWithNewKey(rgbKey);
        }

        private readonly byte[] SBox;
        private int firstIndex = 0;
        private int secondIndex = 0;
        private const int SBoxLength = byte.MaxValue + 1;

        public void ResetWithNewKey(in ReadOnlySpan<byte> key)
        {
            firstIndex = 0;
            secondIndex = 0;
            
            for (int i = 0; i < SBoxLength; i++)
            {
                SBox[i] = (byte)i;
            }

            for (int i = 0, j = 0; i < SBoxLength; i++)
            {
                j = (j + SBox[i] + key[i % key.Length]) % SBoxLength;
                SBox.Swap(i, j);
            }
        }

        private byte PsuedoRandoNumber()
        {
            unchecked
            {
                firstIndex = (firstIndex + 1) % SBoxLength;
                secondIndex = (secondIndex + SBox[firstIndex]) % SBoxLength;
                SBox.Swap(firstIndex, secondIndex);
                return SBox[(SBox[firstIndex] + SBox[secondIndex]) % SBoxLength];
            }
        }

        public void TransfromInPlace(in Span<byte> cipher) => Transform(cipher, cipher);

        public void Transform(in ReadOnlySpan<byte> input, in Span<byte> output)
        {
            unchecked
            {
                for (int k = 0; k < input.Length; k++)
                {
                    output[k] = (byte)(input[k] ^ PsuedoRandoNumber());
                }
            }
        }
    }
}