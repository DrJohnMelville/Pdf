using System;
using System.IO.Pipelines;
using Melville.Pdf.LowLevel.Model.ShortStrings;

namespace Melville.Pdf.LowLevel.Writers;

internal static class PipeWriterExtension
{
    public static void WriteBytes(this PipeWriter pw, ReadOnlySpan<byte> text)
    {
        var span = pw.GetSpan(text.Length);
        text.CopyTo(span);
        pw.Advance(text.Length);
    }
    public static void WriteBytes(this PipeWriter pw, IShortString text)
    {
        var length = text.Length();
        var span = pw.GetSpan(length);
        text.Fill(span);
        pw.Advance(length);
    }

    public static void WriteByte(this PipeWriter pw, byte b)
    {
        var span = pw.GetSpan(1);
        span[0] = b;
        pw.Advance(1);
    }
}