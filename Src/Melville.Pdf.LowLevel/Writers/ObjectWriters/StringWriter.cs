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
        var len = CopyToSpan(encrypted, buffer);
        writer.Advance(len);
    }

    private const int SpaceForOpenAndClosedParens = 2;

    //every character could be special and therefore require 2 bytes to render.
    private static int MaximumRenderedStringLength(int stringLength) =>
        SpaceForOpenAndClosedParens + (2 * stringLength);

    private static int CopyToSpan(in ReadOnlySpan<byte> source, in Span<byte> target)
    {
        return WritePdfString.ToSpan(source, target).Length;
    }
}

public static class WritePdfString
{
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