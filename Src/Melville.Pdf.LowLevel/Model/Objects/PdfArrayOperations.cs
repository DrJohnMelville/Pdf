using System;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;

namespace Melville.Pdf.LowLevel.Model.Objects;

public static class PdfArrayOperations
{
    public static async ValueTask<T> GetAsync<T>(this PdfArray array, int index) where T: PdfObject =>
        (T) await array[index].CA();

    public static ValueTask<PdfObject> GetOrNullAsync(this PdfArray array, int index) =>
        index < 0 || index >= array.Count ? new(PdfTokenValues.Null) : array[index];

    public static async ValueTask<int> IntAtAsync(this PdfArray arr, int index, int defaultVal = 0) =>
        (await arr[index].CA()) is PdfNumber num ? (int)num.IntValue : defaultVal;

    public static async ValueTask<double[]> AsDoublesAsync(this PdfArray array)
    {
        var ret = new double[array.Count];
        for (var i = 0; i < ret.Length; i++)
        {
            ret[i] = ((PdfNumber)await array[i].CA()).DoubleValue;
        }
        return ret;
    }
    public static async ValueTask<int[]> AsIntsAsync(this PdfArray array)
    {
        var ret = new int[array.Count];
        for (var i = 0; i < ret.Length; i++)
        {
            ret[i] = (int)((PdfNumber)await array[i].CA()).IntValue;
        }
        return ret;
    }
    public static async ValueTask<T[]> AsAsync<T>(this PdfArray array) where T:PdfObject
    {
        var ret = new T[array.Count];
        for (var i = 0; i < ret.Length; i++)
        {
            ret[i] = (T)await array[i].CA();
        }
        return ret;
    }

    public static ValueTask<T[]> AsObjectOrArrayAsync<T>(this PdfObject source) where T:PdfObject => source switch
    {
        T item =>  new(new T[] {item}),
        PdfArray arr => arr.AsAsync<T>(),
        _=> new(Array.Empty<T>())
    };
}