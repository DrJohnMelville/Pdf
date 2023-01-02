using System.Collections.Generic;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Model.Wrappers.Functions;

internal static class ClosedIntervalToPdfArray
{
    public static PdfArray AsPdfArray(this ICollection<ClosedInterval> source) =>
        source.AsPdfArray(source.Count);
    public static PdfArray AsPdfArray(this IEnumerable<ClosedInterval> source, int count= 0)
    {
        var numArray = new List<PdfObject>(count*2);
        foreach (var item in source)
        {
            numArray.Add(item.MinValue);
            numArray.Add(item.MaxValue);
        }
        return new PdfArray(numArray);
    }
}