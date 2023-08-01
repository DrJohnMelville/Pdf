using System.Collections.Generic;
using Melville.Pdf.LowLevel.Model.Objects2;

namespace Melville.Pdf.LowLevel.Model.Wrappers.Functions;

internal static class ClosedIntervalToPdfArray
{
    public static PdfValueArray AsPdfArray(this ICollection<ClosedInterval> source) =>
        source.AsPdfArray(source.Count);
    public static PdfValueArray AsPdfArray(this IEnumerable<ClosedInterval> source, int count= 0)
    {
        var numArray = new PdfIndirectValue[count*2];
        int position=0;
        foreach (var item in source)
        {
            numArray[position++] = item.MinValue;
            numArray[position++] = item.MaxValue;
        }
        return new PdfValueArray(numArray);
    }
}