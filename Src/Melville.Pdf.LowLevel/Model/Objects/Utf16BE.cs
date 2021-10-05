using System;
using System.Text;

namespace Melville.Pdf.LowLevel.Model.Objects
{
    public static class Utf16BE
    {
        private static readonly UnicodeEncoding UtfEncoding = new(true, true);

        public static byte[] GetBytesWithBOM(string text)
        {
            if (text.Length == 0) return Array.Empty<byte>();
            var len = UtfEncoding.GetByteCount(text);
            var ret = new byte[len + 2];
            UtfEncoding.Preamble.CopyTo(ret);
            UtfEncoding.GetBytes(text, ret.AsSpan(2));
            return ret;
        }

        public static string GetString(byte[] bytes)
        {
            if (!HasUtf16BOM(bytes))
            {
                if (bytes.Length == 0) return "";
                throw new ArgumentException("Invalid ByteOrderMark on UtfString");
            }
            return UtfEncoding.GetString(bytes.AsSpan(2));
        }

        public static bool HasUtf16BOM(byte[] bytes) =>
            bytes.Length >= 2 && bytes[0] == 0xFE && bytes[1] == 0xFF;
    }
}