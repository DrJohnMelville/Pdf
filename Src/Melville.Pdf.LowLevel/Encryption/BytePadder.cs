using System;
using System.Diagnostics;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.LowLevel.Encryption
{
    public static class BytePadder
    {
        public static byte[] Pad(in ReadOnlySpan<byte> input)
        {
            if (input.Length == 0) return PdfPasswordPaddingBytes;
            var ret = new byte[32];
            Pad(input, ret);
            return ret;
        }

        public static void Pad(in ReadOnlySpan<byte> input, in Span<byte> destination)
        {
            Debug.Assert(destination.Length == 32);
            int origBytes = Math.Min(input.Length, 32);
            input[..origBytes].CopyTo(destination);
            PdfPasswordPaddingBytes.AsSpan(0, 32-origBytes).CopyTo(destination[origBytes..]);
        }

        public static readonly byte[] PdfPasswordPaddingBytes =
        {
            0x28, 0xbf, 0x4e, 0x5e, 0x4e, 0x75, 0x8a, 0x41, 
            0x64, 0x00, 0x4e, 0x56, 0xff, 0xfa, 0x01, 0x08, 
            0x2e, 0x2e, 0x00, 0xb6, 0xd0, 0x68, 0x3e, 0x80, 
            0x2f, 0x0c, 0xa9, 0xfe, 0x64, 0x53, 0x69, 0x7a
        };
    }
}