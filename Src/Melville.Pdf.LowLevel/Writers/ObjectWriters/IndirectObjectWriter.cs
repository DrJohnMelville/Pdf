using System;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Objects2;

namespace Melville.Pdf.LowLevel.Writers.ObjectWriters;

internal static class IndirectObjectWriter
{
    private static ReadOnlySpan<byte> ReferenceLabel => " R"u8;

    public static void WriteObjectReference(this PdfObjectWriter writer,
        int objNum, int generation) =>
        WriteObjectHeader(writer, objNum, generation, " R"u8);

    public static async ValueTask WriteObjectDefinition(this PdfObjectWriter writer,
        int objNum, int generation, PdfDirectValue value)
    {
        WriteObjectHeader(writer, objNum, generation, " obj "u8);
        if (value.TryGet(out PdfValueStream stream))
            await writer.WriteStreamAsync(stream).CA();
        else
            writer.Write(value);
        writer.Write(" endobj\n"u8);
    }

    private static void WriteObjectHeader(PdfObjectWriter writer, int objNum, int generation,
        ReadOnlySpan<byte> objectHeaderOperation)
    {
        writer.Write(objNum);
        writer.Write(" "u8);
        writer.Write(generation);
        writer.Write(objectHeaderOperation);
    }
}