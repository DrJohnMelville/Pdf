using System.Collections.Generic;
using System.Threading.Tasks;
using Melville.Parsing.StreamFilters;
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
using PdfDirectValue = Melville.Pdf.LowLevel.Model.Objects.PdfDirectValue;

namespace Melville.Pdf.LowLevel.Filters;

internal static class StaticCodecFactory
{
    public static ICodecDefinition CodecFor(PdfDirectValue name) => codecs[name];

    private static Dictionary<PdfDirectValue, ICodecDefinition> codecs = CreateDictionary();

    private static Dictionary<PdfDirectValue, ICodecDefinition> CreateDictionary() =>
        new()
        {
            { KnownNames.ASCIIHexDecodeTName, ConstantCodec(new AsciiHexEncoder(), new AsciiHexDecoder()) },
            { KnownNames.ASCII85DecodeTName, ConstantCodec(new Ascii85Encoder(), new Ascii85Decoder()) },
            { KnownNames.RunLengthDecodeTName, ConstantCodec(new RunLengthEncoder(), new RunLengthDecoder()) },
            { KnownNames.LZWDecodeTName, new CodecDefinition(LzwFilterFactory.EncoderAsync, LzwFilterFactory.DecoderAsync)},
            { KnownNames.FlateDecodeTName, new FlateCodecDefinition() },
            { KnownNames.DCTDecodeTName, new DctCodec() },
            { KnownNames.JBIG2DecodeTName, new JbigToPdfAdapter()},
            { KnownNames.JPXDecodeTName, new JpxToPdfAdapter()},
            { KnownNames.CCITTFaxDecodeTName, new CodecDefinition(CcittFilterFactory.EncoderAsync, CcittFilterFactory.DecoderAsync) }
        };

    private static CodecDefinition ConstantCodec(
        IStreamFilterDefinition encoder, IStreamFilterDefinition decoder) =>
        new(_ => new ValueTask<IStreamFilterDefinition>(encoder),
            _ => new ValueTask<IStreamFilterDefinition>(decoder));
}