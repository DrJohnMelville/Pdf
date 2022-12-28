using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Melville.Parsing.Streams;
using Melville.Pdf.LowLevel.Filters.FilterProcessing;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Model.Primitives;

public readonly struct DictionaryBuilder
{
    private readonly List<KeyValuePair<PdfName, PdfObject>> attributes = new();

    public DictionaryBuilder()
    {
    }

    public DictionaryBuilder(Memory<KeyValuePair<PdfName,PdfObject>> other)
    {
        foreach (var pair in other.Span)
        {
            WithItem(pair.Key, pair.Value);
        }
    }
    public DictionaryBuilder(IEnumerable<KeyValuePair<PdfName,PdfObject>> other)
    {
        foreach (var pair in other)
        {
            WithItem(pair.Key, pair.Value);
        }
    }

    public DictionaryBuilder WithItem(PdfName name, PdfObject? value) => 
        value is null || value.IsEmptyObject()?this: WithForcedItem(name, value);

    public DictionaryBuilder WithItem(PdfName name, long value) => WithItem(name, new PdfInteger(value));
    public DictionaryBuilder WithItem(PdfName name, double value) => WithItem(name, new PdfDouble(value));
    public DictionaryBuilder WithItem(PdfName name, string value) => WithItem(name, PdfString.CreateAscii(value));
    public DictionaryBuilder WithItem(PdfName name, bool value) => 
        WithItem(name, value?PdfBoolean.True:PdfBoolean.False);

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

    public DictionaryBuilder WithMultiItem(IEnumerable<KeyValuePair<PdfName, PdfObject>> items) => 
        items.Aggregate(this, (agg, item) => agg.WithItem(item.Key, item.Value));

    public PdfDictionary AsDictionary() => new(attributes.ToArray());

    public PdfStream AsStream(MultiBufferStreamSource stream, StreamFormat format = StreamFormat.PlainText) =>
        new(new LiteralStreamSource(stream.Stream, format), AsArray());
    public PdfStream AsStream(IStreamDataSource source) =>
        new(source, AsArray());

    public KeyValuePair<PdfName, PdfObject>[] AsArray() => attributes.ToArray();

    public void CopyFrom(PdfDictionary sourceDict)
    {
        foreach (var item in sourceDict.RawItems)
        {
            attributes.Add(item);
        }
    }

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