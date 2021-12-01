using System.Buffers;
using Melville.Icc.Model;
using Melville.Icc.Model.Tags;

namespace Melville.Icc.Parser;

public static class TagParser
{
    public static ProfileData Parse(ReadOnlySequence<byte> input)
    {
        var reader = new SequenceReader<byte>(input);
        return reader.ReadBigEndianUint32() switch
        {
            IccTags.chrm => new ChromacityTag(ref reader),
            IccTags.clro => new ColorOrderTag(ref reader),
            IccTags.clrt => new ColorantTableTag(ref reader),
            IccTags.curv => new CurveTag(ref reader),
            IccTags.data => new DataTag(ref reader),
            IccTags.dtim => new DateTimeTag(ref reader),
            IccTags.mft2 => new LutXTag(ref reader, 2),
            IccTags.mft1 => new LutXTag(ref reader, 1),
            IccTags.meas => new MeasurementTypeTag(ref reader),
            IccTags.mluc => new MultiLocalizedUnicodeTag(ref reader),
            IccTags.ncl2 => new NamedColorTag(ref reader),
            IccTags.para => new ParametricCurveTag(ref reader),
            IccTags.pseq => new ProfileSequenceDescriptionTag(ref reader),
            IccTags.psid => new ProfileSequenceIdentifierTag(ref reader),
            IccTags.rcs2 => new ResponseCurveSet16Tag(ref reader),
            IccTags.sf32 => new S15Fixed16Array(ref reader),
            IccTags.sig  => new SignatureTag(ref reader),
            _ => throw new InvalidDataException("Unknown ICC object type")
        };
    }
}