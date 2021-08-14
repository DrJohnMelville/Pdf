using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Filters.Ascii85Filter;
using Melville.Pdf.LowLevel.Filters.AsciiHexFilters;
using Melville.Pdf.LowLevel.Filters.FlateFilters;
using Melville.Pdf.LowLevel.Filters.LzwFilter;
using Melville.Pdf.LowLevel.Filters.RunLengthEncodeFilters;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Filters
{
    public interface IDecoder
    {
        ValueTask<Stream> WrapStreamAsync(Stream input, PdfObject parameter);
    }
    public static class Decompressor
    {
        public static async ValueTask<Stream> DecodeStream(
            Stream source, IReadOnlyList<PdfObject> filters, 
            IReadOnlyList<PdfObject> parameters, int desiredFormat)
        {
            desiredFormat = Math.Min(desiredFormat, filters.Count);
            for (var i = 0; i < desiredFormat; i++)
            {
                source = await DecodeSingleStream(source, filters[i], ComputeParameter(parameters, i));
            }
            return source;
        }

        private static PdfObject ComputeParameter(IReadOnlyList<PdfObject> parameters, int i) => 
            i < parameters.Count ? parameters[i] : PdfTokenValues.Null;

        private static ValueTask<Stream> DecodeSingleStream(Stream source, PdfObject filter, PdfObject parameter) => 
            decoders[(PdfName) filter].WrapStreamAsync(source, parameter);

        private static readonly Dictionary<PdfName, IDecoder> decoders = new()
        {
            {KnownNames.ASCIIHexDecode, new AsciiHexDecoder()},
            {KnownNames.ASCII85Decode, new Ascii85Decoder()},
            {KnownNames.FlateDecode, new FlateDecoder()},
            {KnownNames.Fl, new FlateDecoder()},
            {KnownNames.LZWDecode, new LzwDecoder()},
            {KnownNames.RunLengthDecode, new RunLengthDecoder()}
        };
    }
}