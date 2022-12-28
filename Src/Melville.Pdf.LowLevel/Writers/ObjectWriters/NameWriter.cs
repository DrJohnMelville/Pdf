using System;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Writers.ObjectWriters;

public static class NameWriter
{
    public static ValueTask<FlushResult> Write(PipeWriter target, PdfName name)
    {
        WriteWithoutlush(target, name);
        return target.FlushAsync();
    }

    public static void WriteWithoutlush(PipeWriter target, PdfName name)
    {
        var length = 1 + CountBytes(name.Bytes);
        var span = target.GetSpan(length);
        Fill(span, name.Bytes, length);
        target.Advance(length);
    }

    private static void Fill(Span<byte> span, byte[] nameBytes, int length)
    {
        span[0] = (byte) '/';
        if (NameIsRegularCharactersOnly(nameBytes, length))
        {
            WriteTrivialNameSyntax(span, nameBytes);
        }
        else
        {
            WriteSpecialNameSyntax(span, nameBytes);
        }
    }

    private static void WriteTrivialNameSyntax(Span<byte> span, byte[] nameBytes) => 
        nameBytes.CopyTo(span.Slice(1));

    private static void WriteSpecialNameSyntax(Span<byte> span, byte[] nameBytes)
    {
        var position = 1;
        foreach (var item in nameBytes)
        {
            if (IsSpecialNameChar(item))
            {
                span[position++] = (byte) '#';
                span[position++] = HexMath.HexDigits[item >> 4];
                span[position++] = HexMath.HexDigits[item & 0xF];
            }
            else
            {
                span[position++] = item;
            }
        }
    }

    private static bool NameIsRegularCharactersOnly(byte[] nameBytes, int length) => 
        nameBytes.Length + 1 == length;

    private static int CountBytes(byte[] bytes)
    {
        var ret = 0;
        foreach (var item in bytes)
        {
            ret += IsSpecialNameChar(item) ? 3 : 1;
        }
        return ret;
    }

    private static bool IsSpecialNameChar(byte item) =>
        !CharClassifier.IsRegular(item) || item == (byte)'#';
}