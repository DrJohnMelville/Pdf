using System;
using System.IO.Pipelines;
using Melville.Pdf.LowLevel.Filters.FilterProcessing;

namespace Melville.Pdf.LowLevel.Writers.ObjectWriters;

internal static class StringWriter
{
    public static void Write(
        PipeWriter writer, in Span<byte> value, IObjectCryptContext encryptor)
    {
        var encrypted = encryptor.StringCipher().Encrypt().CryptSpan(value);
        WriteSpanAsString(writer, encrypted);
    }

    public static void WriteSpanAsString(PipeWriter writer, in ReadOnlySpan<byte> encrypted)
    {
        var buffer = writer.GetSpan(MaximumRenderedStringLength(encrypted.Length));
        var len = CopyToSpan(ref buffer, encrypted);
        writer.Advance(len);
    }

    private const int SpaceForOpenAndClosedParens = 2;
    //every character could be special and therefore require 2 bytes to render.
    private static int MaximumRenderedStringLength(int stringLength) => 
        SpaceForOpenAndClosedParens+(2*stringLength);

    private static int CopyToSpan(ref Span<byte> buffer, in ReadOnlySpan<byte> valueBytes)
    {
        int pos = 0;
        buffer[pos++] = (byte)'(';
        foreach (var item in valueBytes)
        {
            var (isSpecial, finalByte) = IsSpecialByte(item);
            if (isSpecial)
            {
                buffer[pos++] = (byte) '\\';
            }

            buffer[pos++] = finalByte;
        }
        buffer[pos++] = (byte)')';
        return pos;
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