using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Melville.Parsing.Streams;
using Melville.Pdf.LowLevel.Filters.FilterProcessing;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Model.Primitives;

/// <summary>
/// This struct is a builder that creates Dictionary objects using a fluent API
/// </summary>
public readonly struct DictionaryBuilder
{
    private readonly List<KeyValuePair<PdfName, PdfObject>> attributes = new();

    /// <summary>
    /// Creates the Dictionary Builder
    /// </summary>
    public DictionaryBuilder()
    {
    }

    /// <summary>
    /// Create a Dictionary Builder and copy an Enumerable object of Key value pairs.
    /// </summary>
    /// <param name="other">The items to copy into the new builder</param>
    public DictionaryBuilder(Memory<KeyValuePair<PdfName,PdfObject>> other)
    {
        foreach (var pair in other.Span)
        {
            WithItem(pair.Key, pair.Value);
        }
    }
    /// <summary>
    /// Create a Dictionary Builder and copy an Enumerable object of Key value pairs.
    /// </summary>
    /// <param name="other">The items to copy into the new builder</param>
    public DictionaryBuilder(IEnumerable<KeyValuePair<PdfName,PdfObject>> other)
    {
        foreach (var pair in other)
        {
            WithItem(pair.Key, pair.Value);
        }
    }

    /// <summary>
    /// Adds an item to this builder.
    /// </summary>
    /// <param name="name">Tje key associated with the item.</param>
    /// <param name="value">The value of the new item.</param>
    /// <returns>This builder.</returns>
    public DictionaryBuilder WithItem(PdfName name, PdfObject? value) => 
        IsEmptyObject(value)?this: WithForcedItem(name, value);

    /// <summary>
    /// Check if this object can be omitted per the PDF spec 
    /// </summary>
    /// <param name="value">The object to test</param>
    /// <returns>True if the object can be omitted from the PDF file, false otherwise.</returns>
    public static bool IsEmptyObject([NotNullWhen(false)]PdfObject? value) =>
        value is null ||
        value == PdfTokenValues.Null ||
        value is PdfArray { Count: 0 } ||
        value is PdfDictionary { Count: 0 } and not PdfStream;

    /// <summary>
    /// Add an item to this builder, potentially replacing and item with the same key.
    /// </summary>
    /// <param name="name">The key for the item.</param>
    /// <param name="value">The associated value.</param>
    /// <returns>This builder.</returns>
    public DictionaryBuilder WithForcedItem(PdfName name, PdfObject value)
    {
        TryDelete(name);
        attributes.Add(new KeyValuePair<PdfName, PdfObject>(name, value));
        return this;
    }

    private void TryDelete(PdfName name)
    {
        for (int i = 0; i < attributes.Count; i++)
        {
            if (ReferenceEquals(name, attributes[i].Key))
            {
                attributes.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// Adding multiple items to this builder
    /// </summary>
    /// <param name="items">The items to be added to the builder.</param>
    /// <returns>This builder.</returns>
    public DictionaryBuilder WithMultiItem(IEnumerable<KeyValuePair<PdfName, PdfObject>> items) => 
        items.Aggregate(this, (agg, item) => agg.WithItem(item.Key, item.Value));

    /// <summary>
    /// Create a dictionary from this builder.
    /// </summary>
    /// <returns>The new dictionary.</returns>
    public PdfDictionary AsDictionary() => new PdfDictionaryConcrete(attributes.ToArray());

    /// <summary>
    /// Create a stream from the builder
    /// </summary>
    /// <param name="stream">The data to include in the body of the stream.</param>
    /// <param name="format">The format in which the data is provided.</param>
    /// <returns>The created stream.</returns>
    public PdfStream AsStream(MultiBufferStreamSource stream, StreamFormat format = StreamFormat.PlainText) =>
        new(new LiteralStreamSource(stream.Stream, format), AsArray());
    internal PdfStream AsStream(IStreamDataSource source) =>
        new(source, AsArray());
    
    private KeyValuePair<PdfName, PdfObject>[] AsArray() => attributes.ToArray();

    /// <summary>
    /// Copy items from a given dictionary to the current builder.
    /// </summary>
    /// <param name="sourceDict">The dictionary to copy from.</param>
    public void CopyFrom(PdfDictionary sourceDict)
    {
        foreach (var item in sourceDict.RawItems)
        {
            attributes.Add(item);
        }
    }

    /// <summary>
    /// Try to get a item from the builder by the key.
    /// </summary>
    /// <param name="id">The key of the desired item.</param>
    /// <param name="output">If the item is found, it is put in this parameter.</param>
    /// <returns>True if the key is found, false otherwise.</returns>
    public bool TryGetValue(PdfName id, [NotNullWhen(true)] out PdfObject? output)
    {
        foreach (var attribute in attributes)
        {
            if (ReferenceEquals(attribute.Key, id))
            {
                output = attribute.Value;
                return true;
            }
        }
        output = null;
        return false;
    }
}