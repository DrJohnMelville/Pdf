using System.Collections.Generic;
using System.IO;
using Melville.Pdf.LowLevel.Filters.Ascii85Filter;
using Melville.Pdf.LowLevel.Filters.AsciiHexFilters;
using Melville.Pdf.LowLevel.Filters.FlateFilters;
using Melville.Pdf.LowLevel.Filters.LzwFilter;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Filters
{
    public interface IEncoder
    {
        public byte[] Encode(byte[] data, PdfObject? parameters);
    }
    public interface IStreamEncoder{
        public Stream Encode(Stream data, PdfObject? parameters);
    }
    public class IseWrapper: IEncoder
    {
        private readonly IStreamEncoder inner;

        public IseWrapper(IStreamEncoder inner)
        {
            this.inner = inner;
        }

        public byte[] Encode(byte[] data, PdfObject? parameters)
        {
            var input = new MemoryStream(data);
            var ret = new MemoryStream();
            using (var encoded = inner.Encode(input, parameters))
            {
                encoded.CopyToAsync(ret).GetAwaiter().GetResult();
            }

            return ret.ToArray();
        }
    }

    public static class Encode
    {
        public static byte[] Compress(byte[] data, PdfObject algorithm, PdfObject? parameters)
        {
            var algorithms = algorithm.AsList();
            return DoCompress(data, algorithms, parameters.AsList(), algorithms.Count - 1);
        }

        private static byte[] DoCompress(byte[] data, IReadOnlyList<PdfObject> algorithms, 
            IReadOnlyList<PdfObject> parameters, int which)
        {
            if (which < 0) return data;
            return DoCompress(compressors[(PdfName) algorithms[which]].Encode(data,
                    which < parameters.Count ? parameters[which] : PdfTokenValues.Null),
                algorithms, parameters, which - 1);
        }

        private static readonly Dictionary<PdfName, IEncoder> compressors = new()
        {
            {KnownNames.ASCIIHexDecode, new IseWrapper(new AsciiHexEncoder())},
            {KnownNames.ASCII85Decode, new IseWrapper(new Ascii85Encoder())},
            {KnownNames.Fl, new IseWrapper(new FlateEncoder())},
            {KnownNames.FlateDecode, new IseWrapper(new FlateEncoder())},
            {KnownNames.LZWDecode, new LzwEncoder()},
        };
    }
}