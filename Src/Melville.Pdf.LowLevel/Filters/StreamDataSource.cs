using System.IO;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.LowLevel.Filters
{
    public readonly struct StreamDataSource
    {
        public Stream Stream { get; }

        public StreamDataSource(string data) : this(data.AsExtendedAsciiBytes()) { }
        public StreamDataSource(byte[] data) : this(new MemoryStream(data)) { }
        public StreamDataSource(Stream stream)
        {
            Stream = stream;
        }
        
        public static implicit operator StreamDataSource(Stream s) => new(s);
        public static implicit operator StreamDataSource(byte[] s) => new(s);
        public static implicit operator StreamDataSource(string s) => new(s);
    }
}