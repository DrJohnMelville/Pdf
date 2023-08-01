using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Melville.Parsing.Streams;
using Melville.Pdf.LowLevel.Filters.FilterProcessing;
using Melville.Pdf.LowLevel.Model.Objects.StreamParts;

namespace Melville.Pdf.LowLevel.Model.Objects;

/// <summary>
/// This struct is a builder that creates Dictionary objects using a fluent API
/// </summary>
public readonly struct ValueDictionaryBuilder
{
    private readonly List<KeyValuePair<PdfDirectValue, PdfIndirectValue>> attributes = new();

    /// <summary>
    /// Creates the Dictionary Builder
    /// </summary>
    public ValueDictionaryBuilder()
    {
    }

    /// <summary>
    /// Create a Dictionary Builder and copy an Enumerable object of Key value pairs.
    /// </summary>
    /// <param name="other">The items to copy into the new builder</param>
    public ValueDictionaryBuilder(Memory<KeyValuePair<PdfDirectValue, PdfIndirectValue>> other)
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
    public ValueDictionaryBuilder(IEnumerable<KeyValuePair<PdfDirectValue, PdfIndirectValue>> other)
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
    public ValueDictionaryBuilder WithItem(PdfDirectValue name, PdfIndirectValue value) => 
        IsEmptyObject(value)?this: WithForcedItem(name, value);

    /// <summary>
    /// Check if this object can be omitted per the PDF spec 
    /// </summary>
    /// <param name="value">The object to test</param>
    /// <returns>True if the object can be omitted from the PDF file, false otherwise.</returns>
    public static bool IsEmptyObject(PdfIndirectValue value) => value.IsNull;

    /// <summary>
    /// Add an item to this builder, potentially replacing and item with the same key.
    /// </summary>
    /// <param name="name">The key for the item.</param>
    /// <param name="value">The associated value.</param>
    /// <returns>This builder.</returns>
    public ValueDictionaryBuilder WithForcedItem(PdfDirectValue name, PdfIndirectValue value)
    {
        if (!name.IsName)
            throw new InvalidOperationException("Dictionary keys must be names");
        TryDelete(name);
        attributes.Add(new KeyValuePair<PdfDirectValue, PdfIndirectValue>(name, value));
        return this;
    }

    private void TryDelete(PdfDirectValue name)
    {
        for (int i = 0; i < attributes.Count; i++)
        {
            if (name.Equals(attributes[i].Key))
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
    public ValueDictionaryBuilder WithMultiItem(IEnumerable<KeyValuePair<PdfDirectValue, PdfIndirectValue>> items) => 
        items.Aggregate(this, (agg, item) => agg.WithItem(item.Key, item.Value));

    /// <summary>
    /// Create a dictionary from this builder.
    /// </summary>
    /// <returns>The new dictionary.</returns>
    public PdfValueDictionary AsDictionary() => new PdfValueDictionary(AsArray());
    
    /// <summary>
    /// Create a stream from the builder
    /// </summary>
    /// <param name="stream">The data to include in the body of the stream.</param>
    /// <param name="format">The format in which the data is provided.</param>
    /// <returns>The created stream.</returns>
    public PdfValueStream AsStream(MultiBufferStreamSource stream, StreamFormat format = StreamFormat.PlainText) =>
        new(new LiteralStreamSource(stream.Stream, format), AsArray());
    internal PdfValueStream AsStream(IStreamDataSource source) =>
        new(source, AsArray());
    
    private KeyValuePair<PdfDirectValue, PdfIndirectValue>[] AsArray() => attributes.ToArray();

    /// <summary>
    /// Copy items from a given dictionary to the current builder.
    /// </summary>
    /// <param name="sourceDict">The dictionary to copy from.</param>
    public void CopyFrom(PdfValueDictionary sourceDict)
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
    public bool TryGetValue(PdfDirectValue id, [NotNullWhen(true)] out PdfIndirectValue? output)
    {
        foreach (var attribute in attributes)
        {
            if (attribute.Key.Equals(id))
            {
                output = attribute.Value;
                return true;
            }
        }
        output = null;
        return false;
    }
}
 