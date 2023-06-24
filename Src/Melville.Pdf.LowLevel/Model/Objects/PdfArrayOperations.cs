using System;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;

namespace Melville.Pdf.LowLevel.Model.Objects;

/// <summary>
/// Operations to get values out of a PdfArray
/// </summary>
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
    /// <param name="array">The CodeSource array</param>
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
    /// <param name="array">The CodeSource array</param>
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
    /// <param name="array">The CodeSource array</param>
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
}