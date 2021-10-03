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
            if (bytes.Length < 2) return "";
            if (HasUtf16BOM(bytes))
                throw new ArgumentException("Invalid ByteOrderMark on UtfString");
            return UtfEncoding.GetString(bytes.AsSpan(2));
        }

        public static bool HasUtf16BOM(byte[] bytes) => 
            UtfEncoding.Preamble.SequenceCompareTo(bytes.AsSpan(0, 2)) != 0;
    }
}