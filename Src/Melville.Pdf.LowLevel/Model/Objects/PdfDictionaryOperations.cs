using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.LowLevel.Model.Objects;

/// <summary>
/// Operations to get values out of a PDF dictionary
/// </summary>
public static class PdfDictionaryOperations
{
    /// <summary>
    /// Get a value from the PDF Dictionary of a given type
    /// </summary>
    /// <typeparam name="T">The expected subtype of PdfObject</typeparam>
    /// <param name="source">The dictionary</param>
    /// <param name="key">Key for the desired value</param>
    /// <returns>The value corresponding to the given key</returns>
    /// <exception cref="PdfParseException">The item does not exist or is the wrong type</exception>
    public static async ValueTask<T> GetAsync<T>(this PdfDictionary source, PdfName key)
    {
        if (source.TryGetValue(key, out var obj) && await obj.CA() is T ret) return ret;
        throw new PdfParseException($"Item {key} is not in dictionary or is wrong type");
    }

    /// <summary>
    /// Gets an item from the dictionary or null if the key does not exist.
    /// </summary>
    /// <param name="dict">The dictionary</param>
    /// <param name="name">The key for the desired item</param>
    /// <returns>The desired item, after resolving indirect references</returns>
    public static async ValueTask<PdfObject> GetOrNullAsync(this PdfDictionary dict, PdfName name) =>
        dict.TryGetValue(name, out var obj) && 
        await obj.CA() is {} definiteObj? definiteObj: PdfTokenValues.Null;

    /// <summary>
    /// Get a desired object of a desired type from the dictionary, or c# null if the object does not exist or is wrong type
    /// </summary>
    /// <typeparam name="T">The desired subtype of PdfObject</typeparam>
    /// <param name="dict">The dictionary</param>
    /// <param name="name">The key for the desired item</param>
    /// <returns>The desired item, after resolving indirect references</returns>
    public static async ValueTask<T?> GetOrNullAsync<T>(this PdfDictionary dict, PdfName name) 
        where T:PdfObject=>
        dict.TryGetValue(name, out var obj) && 
        await obj.CA() is T definiteObj? definiteObj: null;

    /// <summary>
    /// Get the long value from a key, or a default if the item does not exist or is the wrong type
    /// </summary>
    /// <param name="dict">The dictionary</param>
    /// <param name="name">The key for the desired item</param>
    /// <param name="defaultValue">The default value, if the item does not exist or is wrong type</param>
    /// <returns>The desired item as a long, after resolving indirect references</returns>
    public static async ValueTask<long> GetOrDefaultAsync(
        this PdfDictionary dict, PdfName name, long defaultValue) =>
        dict.TryGetValue(name, out var obj) && 
        await obj.CA() is PdfNumber definiteObj? definiteObj.IntValue: defaultValue;
    
    /// <summary>
    /// Get the boolean value from a key, or a default if the item does not exist or is the wrong type
    /// </summary>
    /// <param name="dict">The dictionary</param>
    /// <param name="name">The key for the desired item</param>
    /// <param name="defaultValue">The default value, if the item does not exist or is wrong type</param>
    /// <returns>The desired item as a c# bool, after resolving indirect references</returns>
    public static async ValueTask<bool> GetOrDefaultAsync(
        this PdfDictionary dict, PdfName name, bool defaultValue) =>
        dict.TryGetValue(name, out var obj) && 
        await obj.CA() is PdfBoolean definiteObj? definiteObj == PdfBoolean.True: defaultValue;
    
    /// <summary>
    /// Get the double value from a key, or a default if the item does not exist or is the wrong type
    /// </summary>
    /// <param name="dict">The dictionary</param>
    /// <param name="name">The key for the desired item</param>
    /// <param name="defaultValue">The default value, if the item does not exist or is wrong type</param>
    /// <returns>The desired item as a long, after resolving indirect references</returns>
    public static async ValueTask<double> GetOrDefaultAsync(
        this PdfDictionary dict, PdfName name, double defaultValue) =>
        dict.TryGetValue(name, out var obj) && 
        await obj.CA() is PdfNumber definiteObj? definiteObj.DoubleValue: defaultValue;

    /// <summary>
    /// Get the value from a key, or a default if the item does not exist or is not the correct type
    /// </summary>
    /// <typeparam name="T">The desired subtype of PdfObject</typeparam>
    /// <param name="dict">The dictionary</param>
    /// <param name="name">The key for the desired item</param>
    /// <param name="defaultValue">The default value, if the item does not exist or is wrong type</param>
    /// <returns>The desired item, after resolving indirect references</returns>
    public static async ValueTask<T> GetOrDefaultAsync<T>(
        this PdfDictionary dict, PdfName name, T defaultValue) where T:PdfObject? =>
        dict.TryGetValue(name, out var obj) && 
        await obj.CA() is T definiteObj? definiteObj: defaultValue;
}