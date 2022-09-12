using System.Collections.Generic;
using System.Threading.Tasks;
using Melville.Parsing.StreamFilters;
using Melville.Pdf.LowLevel.Filters.Ascii85Filter;
using Melville.Pdf.LowLevel.Filters.AsciiHexFilters;
using Melville.Pdf.LowLevel.Filters.CCITTFaxDecodeFilters;
using Melville.Pdf.LowLevel.Filters.ExternalFilters;
using Melville.Pdf.LowLevel.Filters.FlateFilters;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter;
using Melville.Pdf.LowLevel.Filters.JpxDecodeFilters;
using Melville.Pdf.LowLevel.Filters.LzwFilter;
using Melville.Pdf.LowLevel.Filters.RunLengthEncodeFilters;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Filters;

public static class StaticCodecFactory
{
    public static ICodecDefinition CodecFor(PdfName name) => codecs[name];

    private static Dictionary<PdfName, ICodecDefinition> codecs = CreateDictionary();

    private static Dictionary<PdfName, ICodecDefinition> CreateDictionary() =>
        new()
        {
            { KnownNames.ASCIIHexDecode, ConstantCodec(new AsciiHexEncoder(), new AsciiHexDecoder()) },
            { KnownNames.ASCII85Decode, ConstantCodec(new Ascii85Encoder(), new Ascii85Decoder()) },
            { KnownNames.RunLengthDecode, ConstantCodec(new RunLengthEncoder(), new RunLengthDecoder()) },
            { KnownNames.LZWDecode, new CodecDefinition(LzwFilterFactory.Encoder, LzwFilterFactory.Decoder)},
            { KnownNames.FlateDecode, new FlateCodecDefinition() },
            { KnownNames.DCTDecode, new DctDecoder() },
            { KnownNames.JBIG2Decode, new JbigToPdfAdapter()},
            { KnownNames.JPXDecode, new JpxToPdfAdapter()},
            { KnownNames.CCITTFaxDecode, new CodecDefinition(CcittFilterFactory.Encoder, CcittFilterFactory.Decoder) }
        };

    private static CodecDefinition ConstantCodec(
        IStreamFilterDefinition encoder, IStreamFilterDefinition decoder) =>
        new(_ => new ValueTask<IStreamFilterDefinition>(encoder),
            _ => new ValueTask<IStreamFilterDefinition>(decoder));
}