using System.Collections.Generic;
using Melville.Pdf.LowLevel.Filters.Ascii85Filter;
using Melville.Pdf.LowLevel.Filters.AshiiHexFilters;
using Melville.Pdf.LowLevel.Filters.FlateFilters;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Filters
{
    public interface IEncoder
    {
        public byte[] Encode(byte[] data, PdfObject? parameters);
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
            {KnownNames.ASCIIHexDecode, new AsciiHexEncoder()},
            {KnownNames.ASCII85Decode, new Ascii85Encoder()},
            {KnownNames.Fl, new FlateEncoder()},
            {KnownNames.FlateDecode, new FlateEncoder()}
        };
    }
}