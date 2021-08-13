using System.Collections.Generic;
using System.IO;
using Melville.Pdf.LowLevel.Filters.Ascii85Filter;
using Melville.Pdf.LowLevel.Filters.AsciiHexFilters;
using Melville.Pdf.LowLevel.Filters.FlateFilters;
using Melville.Pdf.LowLevel.Filters.LzwFilter;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Writers;

namespace Melville.Pdf.LowLevel.Filters
{
    public interface IStreamEncoder{
        public Stream Encode(Stream data, PdfObject? parameters);
    }

    public static class Encode
    {
        public static Stream Compress(in StreamDataSource data, PdfObject algorithm, PdfObject? parameters)
        {
            var algorithms = algorithm.AsList();
            return DoCompress(data.Stream, algorithms, parameters.AsList(), algorithms.Count - 1);
        }

        private static Stream DoCompress(Stream data, IReadOnlyList<PdfObject> algorithms, 
            IReadOnlyList<PdfObject> parameters, int which)
        {
            if (which < 0) return data;
            return DoCompress(compressors[(PdfName) algorithms[which]].Encode(data,
                    which < parameters.Count ? parameters[which] : PdfTokenValues.Null),
                algorithms, parameters, which - 1);
        }

        private static readonly Dictionary<PdfName, IStreamEncoder> compressors = new()
        {
            {KnownNames.ASCIIHexDecode, new AsciiHexEncoder()},
            {KnownNames.ASCII85Decode, new Ascii85Encoder()},
            {KnownNames.Fl, new FlateEncoder()},
            {KnownNames.FlateDecode, new FlateEncoder()},
            {KnownNames.LZWDecode, new LzwEncoder()},
        };
    }
}