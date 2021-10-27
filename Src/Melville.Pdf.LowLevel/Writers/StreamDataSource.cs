using System.Collections.Generic;
using System.IO;
using Melville.Pdf.LowLevel.Filters.FilterProcessing;
using Melville.Pdf.LowLevel.Filters.StreamFilters;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Writers
{
    public readonly struct StreamDataSource
    {
        private readonly MultiBufferStream stream;
        private readonly Dictionary<PdfName, PdfObject> attributes;

        public StreamDataSource(string data) : this(data.AsExtendedAsciiBytes()) { }
        public StreamDataSource(byte[] data) : this(
            data.Length > 0? new MultiBufferStream(data):new MultiBufferStream(1)) { }
        public StreamDataSource(Stream stream)
        {
            attributes = new Dictionary<PdfName, PdfObject>();
            this.stream = ForceMultiBufferStream(stream);
        }
        
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
        
        public StreamDataSource WithItem(PdfName name, PdfObject? value) => 
            value is null || value.IsEmptyObject()?this: WithForcedItem(name, value);

        public StreamDataSource WithForcedItem(PdfName name, PdfObject value)
        {
            attributes.Add(name, value);
            return this;
        }

        public StreamDataSource WithFilter(FilterName? filter) => WithItem(KnownNames.Filter, filter);
        public StreamDataSource WithFilter(params FilterName[] filters) => 
            WithItem(KnownNames.Filter, new PdfArray((IReadOnlyList<FilterName>)filters));

        public StreamDataSource WithFilterParam(PdfObject? param) =>
            WithItem(KnownNames.DecodeParms, param);
        public StreamDataSource WithFilterParam(params PdfObject[] param) =>
            WithItem(KnownNames.DecodeParms, new PdfArray(param));

        public StreamDataSource WithMultiItem(IEnumerable<KeyValuePair<PdfName, PdfObject>> items)
        {
            foreach (var item in items)
            {
                attributes.Add(item.Key, item.Value);
            }
            return this;
        }

        public PdfStream AsStream(StreamFormat format = StreamFormat.PlainText) =>
            new PdfStream(new LiteralStreamSource(stream, format), attributes);
    }
}