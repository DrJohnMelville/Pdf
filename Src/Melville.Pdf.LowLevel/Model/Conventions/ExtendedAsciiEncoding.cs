using System;

namespace Melville.Pdf.LowLevel.Model.Conventions;

public static class ExtendedAsciiEncoding
{
    public static byte[] AsExtendedAsciiBytes(this string s)
    {
        var ret = new byte[s.Length];
        EncodeToSpan(s, ret);
        return ret;
    }

    public static void EncodeToSpan(string s, in Span<byte> ret)
    {
        for (int i = 0; i < s.Length; i++)
        {
            ret[i] = (byte)s[i];
        }
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
    
    public static int CommonPrefixLength(this byte[] fontName, string familySource)
    {
        var fontNamePos = 0;
        var familyPos = 0;
        while (true)
        {
            if (fontNamePos >= fontName.Length || familyPos >= familySource.Length)
                return fontNamePos;
            switch ((fontName[fontNamePos], (byte)(familySource[familyPos])))
            {
                case (32, _) : fontNamePos++; break;
                case (_, 32) : familyPos++; break;
                case var(a,b) when a==b:
                    fontNamePos++;
                    familyPos++;
                    break;
                default: return fontNamePos;
            }
        }
    }
}