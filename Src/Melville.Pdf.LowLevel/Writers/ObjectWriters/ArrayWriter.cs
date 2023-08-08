using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Writers.ObjectWriters;

internal static class ArrayWriter
{

    public static void WriteArray(in PdfObjectWriter writer, PdfArray arr)
    {
        writer.Write("["u8);
        if (arr.Count > 0)
        {
            writer.Write(arr.RawItems[0]);
            for (int i = 1; i < arr.Count; i++)
            {
                writer.Write(" "u8); // I think  spaces are required by the spec.
                writer.Write(arr.RawItems[i]);
            }
        }
        writer.Write("]"u8);
    }

}