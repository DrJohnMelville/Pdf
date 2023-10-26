using System;

namespace Melville.Pdf.LowLevel.Writers.ObjectWriters;

/// <summary>
/// Write PDf String to a Span in PDf string syntax
/// </summary>
public static class WritePdfString
{
    /// <summary>
    /// Write PDf String to a Span in PDf string syntax
    /// </summary>
    public static Span<byte> ToSpan(in ReadOnlySpan<byte> source, in Span<byte> target)
    {
        int pos = 0;
        target[pos++] = (byte)'(';
        foreach (var item in source)
        {
            var (isSpecial, finalByte) = IsSpecialByte(item);
            if (isSpecial)
            {
                target[pos++] = (byte)'\\';
            }

            target[pos++] = finalByte;
        }

        target[pos++] = (byte)')';
        return target[..pos];
    }

    private static (bool isSpecial, byte suffix) IsSpecialByte(byte input) => input switch
    {
        (byte)'(' or (byte)')' or (byte)'\\' => (true, input),
        (byte)'\n' => (true, (byte)'n'),
        (byte)'\r' => (true, (byte)'r'),
        (byte)'\t' => (true, (byte)'t'),
        (byte)'\b' => (true, (byte)'b'),
        (byte)'\f' => (true, (byte)'f'),
        _ => (false, input)
    };
}