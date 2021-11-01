using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Melville.Pdf.LowLevel.Model.Objects;

public static class PdfArrayOperations
{
    public static async ValueTask<T> GetAsync<T>(this PdfArray array, int index) where T: PdfObject =>
        (T) await array[index];

    public static ValueTask<PdfObject> GetOrNullAsync(this PdfArray array, int index) =>
        index < 0 || index >= array.Count ? new(PdfTokenValues.Null) : array[index];

    public static async ValueTask<double[]> AsDoublesAsync(this PdfArray array)
    {
        var ret = new double[array.Count];
        for (var i = 0; i < ret.Length; i++)
        {
            ret[i] = ((PdfNumber)await array[i]).DoubleValue;
        }
        return ret;
    }
    public static async ValueTask<int[]> AsIntsAsync(this PdfArray array)
    {
        var ret = new int[array.Count];
        for (var i = 0; i < ret.Length; i++)
        {
            ret[i] = (int)((PdfNumber)await array[i]).IntValue;
        }
        return ret;
    }
    public static async ValueTask<T[]> AsAsync<T>(this PdfArray array) where T:PdfObject
    {
        var ret = new T[array.Count];
        for (var i = 0; i < ret.Length; i++)
        {
            ret[i] = (T)await array[i];
        }
        return ret;
    }
}