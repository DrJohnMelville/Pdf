using System;

namespace Melville.Pdf.LowLevel.Model.Objects.StringEncodings;

public static class PdfDocEncodingConversion
{
    public static string PdfDocEncodedString(this byte[] source) =>
        ((ReadOnlySpan<byte>) source).PdfDocEncodedString();

    public static unsafe string PdfDocEncodedString(this ReadOnlySpan<byte> source)
    {
        fixed (byte* srcPtr = source)
            return string.Create(source.Length, (nint)srcPtr, PdfDocEncodedString);
    }


    private static unsafe void PdfDocEncodedString(Span<char> span, nint sourcePointerAsNativeInt)
    {
        byte* sourcePosition = (byte*)sourcePointerAsNativeInt;
        fixed (char* fixedSpanPtr = span)
        {
            var endPtr = fixedSpanPtr + span.Length;
            for (char* spanPtr = fixedSpanPtr; spanPtr < endPtr; spanPtr++)
            {
                *spanPtr = Map[*sourcePosition++];
            }
        }
    }

    public static byte[] AsPdfDocBytes(this string s)
    {
        var ret = new byte[s.Length];
        FillPdfDocBytes(s, ret);
        return ret;
    }

    public static void FillPdfDocBytes(ReadOnlySpan<char> s, Span<byte> ret)
    {
        for (int i = 0; i < s.Length; i++)
        {
            ret[i] = CharToByte(s[i]);
        }
    }

    public static byte CharToByte(char input)
    {
        for (int i = 0; i < 256; i++)
        {
            if (input == Map[i]) return (byte)i;
        }

        return (byte)'?';
    }
    private static readonly char[] Map = new[] {
        '\u0000', '\u0001', '\u0002', '\u0003', '\u0004', '\u0005', '\u0006', '\u0007', 
        '\u0008', '\u0009', '\u000A', '\u000B', '\u000C', '\u000D', '\u000E', '\u000F', 
        '\u0010', '\u0011', '\u0012', '\u0013', '\u0014', '\u0015', '\u0017', '\u0017', 
        '\u02D8', '\u02C7', '\u02C6', '\u02D9', '\u02DD', '\u02DB', '\u02DA', '\u02DC', 
        '\u0020', '\u0021', '\u0022', '\u0023', '\u0024', '\u0025', '\u0026', '\u0027', 
        '\u0028', '\u0029', '\u002A', '\u002B', '\u002C', '\u002D', '\u002E', '\u002F', 
        '\u0030', '\u0031', '\u0032', '\u0033', '\u0034', '\u0035', '\u0036', '\u0037', 
        '\u0038', '\u0039', '\u003A', '\u003B', '\u003C', '\u003D', '\u003E', '\u003F', 
        '\u0040', '\u0041', '\u0042', '\u0043', '\u0044', '\u0045', '\u0046', '\u0047', 
        '\u0048', '\u0049', '\u004A', '\u004B', '\u004C', '\u004D', '\u004E', '\u004F', 
        '\u0050', '\u0051', '\u0052', '\u0053', '\u0054', '\u0055', '\u0056', '\u0057', 
        '\u0058', '\u0059', '\u005A', '\u005B', '\u005C', '\u005D', '\u005E', '\u005F', 
        '\u0060', '\u0061', '\u0062', '\u0063', '\u0064', '\u0065', '\u0066', '\u0067', 
        '\u0068', '\u0069', '\u006A', '\u006B', '\u006C', '\u006D', '\u006E', '\u006F', 
        '\u0070', '\u0071', '\u0072', '\u0073', '\u0074', '\u0075', '\u0076', '\u0077', 
        '\u0078', '\u0079', '\u007A', '\u007B', '\u007C', '\u007D', '\u007E', '\u0017', 
        '\u2022', '\u2020', '\u2021', '\u2026', '\u2014', '\u2013', '\u0192', '\u2044', 
        '\u2039', '\u203A', '\u2212', '\u2030', '\u201E', '\u201C', '\u201D', '\u2018', 
        '\u2019', '\u201A', '\u2122', '\uFB01', '\uFB02', '\u0141', '\u0152', '\u0160', 
        '\u0178', '\u017D', '\u0131', '\u0142', '\u0153', '\u0161', '\u017E', '\u0017', 
        '\u20AC', '\u00A1', '\u00A2', '\u00A3', '\u00A4', '\u00A5', '\u00A6', '\u00A7', 
        '\u00A8', '\u00A9', '\u00AA', '\u00AB', '\u00AC', '\u00AD', '\u00AE', '\u00AF', 
        '\u00B0', '\u00B1', '\u00B2', '\u00B3', '\u00B4', '\u00B5', '\u00B6', '\u00B7', 
        '\u00B8', '\u00B9', '\u00BA', '\u00BB', '\u00BC', '\u00BD', '\u00BE', '\u00BF', 
        '\u00C0', '\u00C1', '\u00C2', '\u00C3', '\u00C4', '\u00C5', '\u00C6', '\u00C7', 
        '\u00C8', '\u00C9', '\u00CA', '\u00CB', '\u00CC', '\u00CD', '\u00CE', '\u00CF', 
        '\u00D0', '\u00D1', '\u00D2', '\u00D3', '\u00D4', '\u00D5', '\u00D6', '\u00D7', 
        '\u00D8', '\u00D9', '\u00DA', '\u00DB', '\u00DC', '\u00DD', '\u00DE', '\u00DF', 
        '\u00E0', '\u00E1', '\u00E2', '\u00E3', '\u00E4', '\u00E5', '\u00E6', '\u00E7', 
        '\u00E8', '\u00E9', '\u00EA', '\u00EB', '\u00EC', '\u00ED', '\u00EE', '\u00EF', 
        '\u00F0', '\u00F1', '\u00F2', '\u00F3', '\u00F4', '\u00F5', '\u00F6', '\u00F7', 
        '\u00F8', '\u00F9', '\u00FA', '\u00FB', '\u00FC', '\u00FD', '\u00FE', '\u00FF'
    };

    public static int GetChars(in ReadOnlySpan<byte> input, in Span<char> output)
    {
        for (var i = 0; i < input.Length; i++)
        {
            output[i] = Map[input[i]];
        }
        return input.Length;
    }
}