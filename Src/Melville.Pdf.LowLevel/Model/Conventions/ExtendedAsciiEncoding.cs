using System;

namespace Melville.Pdf.LowLevel.Model.Conventions
{
    public static class ExtendedAsciiEncoding
    {
        public static byte[] AsExtendedAsciiBytes(this string s)
        {
            var ret = new byte[s.Length];
            for (int i = 0; i < s.Length; i++)
            {
                ret[i] = (byte) s[i];
            }
            return ret;
        }

        public static string ExtendedAsciiString(this byte[] source) =>
            ((ReadOnlySpan<byte>) source).ExtendedAsciiString();

        public static unsafe string ExtendedAsciiString(this ReadOnlySpan<byte> source)
        {
            fixed (byte* srcPtr = source)
                return string.Create(source.Length, (nint)srcPtr, ExtendedAsciiString);
        }

        private static unsafe void ExtendedAsciiString(Span<char> span, nint SourcePointerAsNativeInt)
        {
            byte* sourcePosition = (byte*)SourcePointerAsNativeInt;
            for (int i = 0; i < span.Length; i++)
            {
                span[i] = (char)*sourcePosition++;
            }
        }
    }
}