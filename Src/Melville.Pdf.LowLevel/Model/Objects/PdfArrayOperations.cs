using System;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;

namespace Melville.Pdf.LowLevel.Model.Objects;

public static class PdfArrayOperations
{
    /// <summary>
    /// Get an array element and statically cast it to a subtype of object
    /// </summary>
    /// <typeparam name="T">The desired PdfObject subtype</typeparam>
    /// <param name="array">The array to get from</param>
    /// <param name="index">The index to access.</param>
    /// <returns>The object at the given index with all indirect links taken</returns>
    public static async ValueTask<T> GetAsync<T>(this PdfArray array, int index) where T: PdfObject =>
        (T) await array[index].CA();

    /// <summary>
    /// Try to get an integer at an index in an array or return an default value if the item is not a number.
    /// </summary>
    /// <param name="arr">Array to access.</param>
    /// <param name="index">Desired index</param>
    /// <param name="defaultVal">Default if the given item is not a number</param>
    /// <returns></returns>
    public static async ValueTask<int> IntAtAsync(this PdfArray arr, int index, int defaultVal = 0) =>
        (await arr[index].CA()) is PdfNumber num ? (int)num.IntValue : defaultVal;

    /// <summary>
    /// Cast a PDfArray to an array of doubles.
    /// </summary>
    /// <param name="array">The source array</param>
    /// <returns>An equivilent array of doubles.</returns>
    public static async ValueTask<double[]> AsDoublesAsync(this PdfArray array)
    {
        var ret = new double[array.Count];
        for (var i = 0; i < ret.Length; i++)
        {
            ret[i] = ((PdfNumber)await array[i].CA()).DoubleValue;
        }
        return ret;
    }
    /// <summary>
    /// Cast a PDfArray to an array of ints.
    /// </summary>
    /// <param name="array">The source array</param>
    /// <returns>An equivilent array of ints.</returns>
    public static async ValueTask<int[]> AsIntsAsync(this PdfArray array)
    {
        var ret = new int[array.Count];
        for (var i = 0; i < ret.Length; i++)
        {
            ret[i] = (int)((PdfNumber)await array[i].CA()).IntValue;
        }
        return ret;
    }

    /// <summary>
    /// Convert a PdfArray to an array of subtype of PdfObject -- with all indirect references resolved.
    /// </summary>
    /// <typeparam name="T">Desired subtype of PdfObject</typeparam>
    /// <param name="array">The source array</param>
    /// <returns>An equivilent C# array of a PdfObject children.</returns>
    public static async ValueTask<T[]> AsAsync<T>(this PdfArray array) where T:PdfObject
    {
        var ret = new T[array.Count];
        for (var i = 0; i < ret.Length; i++)
        {
            ret[i] = (T)await array[i].CA();
        }
        return ret;
    }

    /// <summary>
    /// At a number of places in the PDF spec a single item array can be replaced by just the item. This method resolves this,
    /// a given pdf object is converted into an array of a gived PdfObject child.  IF it is a bare object an array is synthesized.
    /// All indirect references are resolved.
    /// </summary>
    /// <typeparam name="T">Desired PdfObject child type</typeparam>
    /// <param name="source">A PdfObject or PdfArray</param>
    /// <returns>A C# array of PdfObjects that implements the semantics above.</returns>
    public static ValueTask<T[]> AsObjectOrArrayAsync<T>(this PdfObject source) where T:PdfObject => source switch
    {
        T item =>  new(new T[] {item}),
        PdfArray arr => arr.AsAsync<T>(),
        _=> new(Array.Empty<T>())
    };
}