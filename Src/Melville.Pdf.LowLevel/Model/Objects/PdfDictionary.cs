using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Filters.FilterProcessing;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.LowLevel.Model.Objects;

/// <summary>
/// This class represents a PdfDictionary.  PdfStream is a subclass of this class.
/// </summary>
public abstract class PdfDictionary : PdfObject, IReadOnlyDictionary<PdfName, ValueTask<PdfObject>>
{
    /// <summary>
    /// Flyweight object representing an empty dictionary or stream.
    /// </summary>
    public static readonly PdfStream Empty =
        new(new LiteralStreamSource(Array.Empty<byte>(), StreamFormat.DiskRepresentation),
            Array.Empty<KeyValuePair<PdfName,PdfObject>>());
    
    /// <summary>
    /// A C# read only dictionary associating PDF names with unresolved PDF objects.
    /// </summary>
    public IReadOnlyDictionary<PdfName, PdfObject> RawItems { get; }

    protected PdfDictionary(Memory<KeyValuePair<PdfName, PdfObject>> rawItems)
    {
        RawItems = rawItems.Length > 19 ? CreateDictionary(rawItems):
            new SmallReadOnlyDictionary<PdfName, PdfObject>(rawItems);
    }

    private IReadOnlyDictionary<PdfName, PdfObject> CreateDictionary(Memory<KeyValuePair<PdfName, PdfObject>> rawItems)
    {
        var ret = new Dictionary<PdfName, PdfObject>(rawItems.Length);
        foreach (var rawItem in rawItems.Span)
        {
            ret.Add(rawItem.Key, rawItem.Value);
        }
        return ret;
    }

    #region Dictionary Implementation

    /// <summary>
    /// Number of Key-Value pairs in this PdfDictionary
    /// </summary>
    public int Count => RawItems.Count;

    /// <inheritdoc />
    public bool ContainsKey(PdfName key) => RawItems.ContainsKey(key);

    /// <summary>
    /// Gets a value for the given key, resolving indirect references.
    /// </summary>
    /// <param name="key">The key for the desired value</param>
    /// <returns>The value corresponding to the key</returns>
    /// <exception cref="KeyNotFoundException">If the key is not present in the dictionary</exception>
    public ValueTask<PdfObject> this[PdfName key] => RawItems[key].DirectValueAsync();


    /// <inheritdoc />
    public IEnumerable<PdfName> Keys => RawItems.Keys;

    /// <summary>Gets an enumerable collection that contains the values in the read-only dictionary.  This method resolves
    /// indirect PDF references.</summary>
    /// <returns>An enumerable collection that contains the values in the read-only dictionary.</returns>
    IEnumerable<ValueTask<PdfObject>> IReadOnlyDictionary<PdfName, ValueTask<PdfObject>>.Values =>
        RawItems.Values.Select(i => i.DirectValueAsync());

    /// <summary>Returns an enumerator that iterates through the collection.  This method resolves the indirect references</summary>
    /// <returns>An enumerator that can be used to iterate through the collection.</returns>
    public IEnumerator<KeyValuePair<PdfName, ValueTask<PdfObject>>> GetEnumerator() =>
        RawItems
            .Select(i => new KeyValuePair<PdfName, ValueTask<PdfObject>>(i.Key, i.Value.DirectValueAsync()))
            .GetEnumerator();

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();


    /// <summary>Gets the value that is associated with the specified key. This method resolves indirect PDF references</summary>
    /// <param name="key">The key to locate.</param>
    /// <param name="value">When this method returns, the value associated with the specified key, if the key is found; otherwise, the default value for the type of the <paramref name="value" /> parameter. This parameter is passed uninitialized.</param>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="key" /> is <see langword="null" />.</exception>
    /// <returns>
    /// <see langword="true" /> if the object that implements the <see cref="T:System.Collections.Generic.IReadOnlyDictionary`2" /> interface contains an element that has the specified key; otherwise, <see langword="false" />.</returns>
    public bool TryGetValue(PdfName key, out ValueTask<PdfObject> value)
    {
        if (RawItems.TryGetValue(key, out var ret))
        {
            value = ret.DirectValueAsync();
            return true;
        }
        value = default;
        return false;
    }

    /// <summary>
    /// Returns the values in the dictionary after resolving indirect references.
    /// </summary>
    public IEnumerable<ValueTask<PdfObject>> Values => RawItems.Values.Select(i => i.DirectValueAsync());

    #endregion
}