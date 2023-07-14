using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.LowLevel.Model.Objects2;

/// <summary>
/// Operations to get values out of a PDF dictionary
/// </summary>
public static class PdfValueDictionaryOperations
{
    /// <summary>
    /// Get a value from the PDF Dictionary of a given type
    /// </summary>
    /// <typeparam name="T">The expected subtype of PdfObject</typeparam>
    /// <param name="source">The dictionary</param>
    /// <param name="key">Key for the desired value</param>
    /// <returns>The value corresponding to the given key</returns>
    /// <exception cref="PdfParseException">The item does not exist or is the wrong type</exception>
    public static async ValueTask<T> GetAsync<T>(
        this PdfValueDictionary source, PdfDirectValue key)
    {
        if (source.TryGetValue(key, out var obj) && (await obj.CA()).TryGet(out T? ret)) 
            return ret;
        throw new PdfParseException($"Item {key} is not in dictionary or is wrong type");
    }

    /// <summary>
    /// Gets an item from the dictionary or null if the key does not exist.
    /// </summary>
    /// <param name="dict">The dictionary</param>
    /// <param name="name">The key for the desired item</param>
    /// <returns>The desired item, after resolving indirect references</returns>
    public static ValueTask<PdfDirectValue> GetOrNullAsync(
        this PdfValueDictionary dict, PdfDirectValue name) =>
        dict.TryGetValue(name, out var obj) ? obj: new(PdfDirectValue.CreateNull());


    /// <summary>
    /// Get the value from a key, or a default if the item does not exist or is not the correct type
    /// </summary>
    /// <typeparam name="T">The desired subtype of PdfObject</typeparam>
    /// <param name="dict">The dictionary</param>
    /// <param name="name">The key for the desired item</param>
    /// <param name="defaultValue">The default value, if the item does not exist or is wrong type</param>
    /// <returns>The desired item, after resolving indirect references</returns>
    public static async ValueTask<T> GetOrDefaultAsync<T>(
        this PdfValueDictionary dict, PdfDirectValue name, T defaultValue) =>
        dict.TryGetValue(name, out var obj) && (await obj.CA()).TryGet(out T? definiteObj)
        ? definiteObj: defaultValue;
}