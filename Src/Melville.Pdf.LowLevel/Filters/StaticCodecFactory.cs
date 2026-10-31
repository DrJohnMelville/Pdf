using System.Collections.Generic;
using System.Threading.Tasks;
using Melville.Parsing.StreamFilters;
using Melville.Parsing.StreamFilters.AsciiHex;
using Melville.Pdf.LowLevel.Filters.Ascii85Filter;
using Melville.Pdf.LowLevel.Filters.AsciiHexFilters;
using Melville.Pdf.LowLevel.Filters.CCITTFaxDecodeFilters;
using Melville.Pdf.LowLevel.Filters.FlateFilters;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter;
using Melville.Pdf.LowLevel.Filters.JpegFilter;
using Melville.Pdf.LowLevel.Filters.JpxDecodeFilters;
using Melville.Pdf.LowLevel.Filters.LzwFilter;
using Melville.Pdf.LowLevel.Filters.RunLengthEncodeFilters;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Filters;

internal static class StaticCodecFactory
{
    public static ICodecDefinition CodecFor(PdfDirectObject name) => codecs[name];

    private static Dictionary<PdfDirectObject, ICodecDefinition> codecs = CreateDictionary();

    private static Dictionary<PdfDirectObject, ICodecDefinition> CreateDictionary() =>
        new()
        {
            { KnownNames.ASCIIHexDecode, new CodecDefinition(
                new(_=>new AsciiHexEncoder()), new(new AsciiHexDecoder())) },
            { KnownNames.ASCII85Decode, new CodecDefinition(new(_=>new Ascii85Encoder()),
                new( _=>new Ascii85Decoder())) },
            { KnownNames.RunLengthDecode, new CodecDefinition(new(new RunLengthEncoder()), new(new RunLengthDecoder())) },
            { KnownNames.LZWDecode, new CodecDefinition(
                new(LzwFilterFactory.EncoderAsync), new(LzwFilterFactory.DecoderAsync))},
            { KnownNames.FlateDecode, new FlateCodecDefinition() },
            { KnownNames.DCTDecode, new DctCodec() },
            { KnownNames.JBIG2Decode, new JbigToPdfAdapter()},
            { KnownNames.JPXDecode, new JpxToPdfAdapter()},
            { KnownNames.CCITTFaxDecode, new CodecDefinition(
                new(CcittFilterFactory.EncoderAsync), new(CcittFilterFactory.DecoderAsync)) }
        };

    
}