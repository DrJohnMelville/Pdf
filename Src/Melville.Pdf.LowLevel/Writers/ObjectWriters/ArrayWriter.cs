using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Objects2;
using Melville.Pdf.LowLevel.Visitors;

namespace Melville.Pdf.LowLevel.Writers.ObjectWriters;

internal static class ArrayWriter
{

    public static void WriteArray(in PdfObjectWriter writer, PdfValueArray arr)
    {
        writer.Write("["u8);
        if (arr.Count > 0)
        {
            writer.Write(arr.RawItems[0]);
            for (int i = 1; i < arr.Count; i++)
            {
                #warning -- Use NeedsLeadingSpace to eliminate some spaces;
                writer.Write(" "u8);
                writer.Write(arr.RawItems[i]);
            }
        }
        writer.Write("]"u8);
    }

}