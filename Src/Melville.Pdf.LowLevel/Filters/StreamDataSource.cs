using System.IO;
using System.Runtime.InteropServices.ComTypes;
using Melville.Pdf.LowLevel.Filters.StreamFilters;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.LowLevel.Filters
{
    public readonly struct StreamDataSource
    {
        public MultiBufferStream Stream { get; }

        public StreamDataSource(string data) : this(data.AsExtendedAsciiBytes()) { }
        public StreamDataSource(byte[] data) : this(
            data.Length > 0? new MultiBufferStream(data):new MultiBufferStream(1)) { }
        public StreamDataSource(MultiBufferStream stream)
        {
            Stream = stream;
        }

        public static implicit operator StreamDataSource(Stream s) => 
            new(s is MultiBufferStream mbs ? mbs : CopyToMultiBufferStream(s));

        private static MultiBufferStream CopyToMultiBufferStream(Stream s)
        {
            var ret = new MultiBufferStream(DesiredStreamLength(s));
            s.CopyTo(ret);
            ret.Seek(0, SeekOrigin.Begin); // the returned steam must be immediately readable.
            return ret;
        }

        private static int DesiredStreamLength(Stream s) => 
            s.Length > 0?(int)s.Length:4096;

        public static implicit operator StreamDataSource(byte[] s) => new(s);
        public static implicit operator StreamDataSource(string s) => new(s);
    }
}