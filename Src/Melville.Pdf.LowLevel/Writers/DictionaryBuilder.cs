using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Melville.Pdf.LowLevel.Filters.FilterProcessing;
using Melville.Pdf.LowLevel.Filters.StreamFilters;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Writers
{
    public readonly struct DictionaryBuilder
    {
        private readonly Dictionary<PdfName, PdfObject> attributes = new();

        
        public DictionaryBuilder WithItem(PdfName name, PdfObject? value) => 
            value is null || value.IsEmptyObject()?this: WithForcedItem(name, value);

        public DictionaryBuilder WithForcedItem(PdfName name, PdfObject value)
        {
            attributes.Add(name, value);
            return this;
        }

        public DictionaryBuilder WithFilter(FilterName? filter) => WithItem(KnownNames.Filter, filter);
        public DictionaryBuilder WithFilter(params FilterName[] filters) => 
            WithItem(KnownNames.Filter, new PdfArray((IReadOnlyList<FilterName>)filters));
        public DictionaryBuilder WithFilterParam(PdfObject? param) =>
            WithItem(KnownNames.DecodeParms, param);
        public DictionaryBuilder WithFilterParam(params PdfObject[] param) =>
            WithItem(KnownNames.DecodeParms, new PdfArray(param));

        public DictionaryBuilder WithMultiItem(IEnumerable<KeyValuePair<PdfName, PdfObject>> items) => 
            items.Aggregate(this, (agg, item) => agg.WithItem(item.Key, item.Value));

        public PdfDictionary AsDictionary() => new(attributes);

        public PdfStream AsStream(string data, StreamFormat format = StreamFormat.PlainText) =>
            AsStream(data.AsExtendedAsciiBytes(), format);
        public PdfStream AsStream(byte[] data, StreamFormat format = StreamFormat.PlainText) =>
            AsStream(data.Length > 0 ? new MultiBufferStream(data) : new MultiBufferStream(1), format);
        public PdfStream AsStream(Stream stream, StreamFormat format = StreamFormat.PlainText) =>
            new(new LiteralStreamSource(ForceMultiBufferStream(stream), format), attributes);
        
        private static MultiBufferStream ForceMultiBufferStream(Stream s) => 
            s is MultiBufferStream mbs ? mbs : CopyToMultiBufferStream(s);

        private static MultiBufferStream CopyToMultiBufferStream(Stream s)
        {
            var ret = new MultiBufferStream(DesiredStreamLength(s));
            s.CopyTo(ret);
            ret.Seek(0, SeekOrigin.Begin); // the returned steam must be immediately readable.
            return ret;
        }

        private static int DesiredStreamLength(Stream s) => 
            s.Length > 0?(int)s.Length:4096;
    }
}