using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Melville.Parsing.Streams;
using Melville.Pdf.LowLevel.Filters.FilterProcessing;
using Melville.Pdf.LowLevel.Filters.StreamFilters;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Objects.StreamDataSources;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Melville.Pdf.LowLevel.Writers;

public readonly struct DictionaryBuilder
{
    private readonly Dictionary<PdfName, PdfObject> attributes = new();

        
    public DictionaryBuilder WithItem(PdfName name, PdfObject? value) => 
        value is null || value.IsEmptyObject()?this: WithForcedItem(name, value);

    public DictionaryBuilder WithItem(PdfName name, long value) => WithItem(name, new PdfInteger(value));
    public DictionaryBuilder WithItem(PdfName name, double value) => WithItem(name, new PdfDouble(value));
    public DictionaryBuilder WithItem(PdfName name, string value) => WithItem(name, PdfString.CreateAscii(value));
    public DictionaryBuilder WithItem(PdfName name, bool value) => 
        WithItem(name, value?PdfBoolean.True:PdfBoolean.False);

    public DictionaryBuilder WithForcedItem(PdfName name, PdfObject value)
    {
        attributes[name] =value;
        return this;
    }
        
    public DictionaryBuilder WithMultiItem(IEnumerable<KeyValuePair<PdfName, PdfObject>> items) => 
        items.Aggregate(this, (agg, item) => agg.WithItem(item.Key, item.Value));

    public PdfDictionary AsDictionary() => new(attributes);

    public PdfStream AsStream(MultiBufferStreamSource stream, StreamFormat format = StreamFormat.PlainText) =>
        new(new LiteralStreamSource(stream.Stream, format), attributes);
}