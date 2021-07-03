using System;
using System.Text;

namespace Melville.Pdf.LowLevel.Model
{
    public static class ExtendedAsciiEncodig
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
        public static string ExtendedAsciiString(this ReadOnlySpan<byte> source)
        {
            var sb = new StringBuilder(source.Length);
            foreach (var character in source)
            {
                sb.Append((char) character);
            }

            return sb.ToString();
        }
    }
}