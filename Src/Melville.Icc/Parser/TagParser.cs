using System.Buffers;
using Melville.Icc.Model;
using Melville.Icc.Model.Tags;

namespace Melville.Icc.Parser;

public static class TagParser
{
    public static ProfileData Parse(ReadOnlySequence<byte> input)
    {
        var reader = new SequenceReader<byte>(input);
        return Parse(ref reader);
    }

    public static ProfileData Parse(ref SequenceReader<byte> reader) => 
        reader.ReadBigEndianUint32() switch
        {
            IccTags.bACS or IccTags.eACS => NullMultiDimensionalLookupTable.Parse(ref reader),
            IccTags.chrm => new ChromacityTag(ref reader),
            IccTags.clro => new ColorOrderTag(ref reader),
            IccTags.clrt => new ColorantTableTag(ref reader),
            IccTags.clut => new MultidimensionalLookupTable(ref reader),
            IccTags.curv => new CurveTag(ref reader),
            IccTags.data => new DataTag(ref reader),
            IccTags.dtim => new DateTimeTag(ref reader),
            IccTags.matf => new MultiProcessMatrix(ref reader),
            IccTags.mft2 => new LutXTag(ref reader, 2),
            IccTags.mft1 => new LutXTag(ref reader, 1),
            IccTags.meas => new MeasurementTypeTag(ref reader),
            IccTags.mAB => new LutAToBTag(ref reader),
            IccTags.mBA => new LutBToATag(ref reader),
            IccTags.mpet => new MultiProcessTag(ref reader),
            IccTags.mluc => new MultiLocalizedUnicodeTag(ref reader),
            IccTags.ncl2 => new NamedColorTag(ref reader),
            IccTags.para => new ParametricCurveTag(ref reader),
            IccTags.pseq => new ProfileSequenceDescriptionTag(ref reader),
            IccTags.psid => new ProfileSequenceIdentifierTag(ref reader),
            IccTags.rcs2 => new ResponseCurveSet16Tag(ref reader),
            IccTags.sf32 => new S15Fixed16Array(ref reader),
            IccTags.sig => new SignatureTag(ref reader),
            IccTags.text => new TextTag(ref reader),
            IccTags.uf32 => new U16Fixed16Array(ref reader),
            IccTags.ui16 => new UInt16Array(ref reader),
            IccTags.ui32 => new UInt32Array(ref reader),
            IccTags.ui64 => new UInt64Array(ref reader),
            IccTags.XYZ => new XyzArray(ref reader),
            IccTags.view => new ViewingConditionsTag(ref reader),
            _ => throw new InvalidDataException("Unknown ICC object type")
        };
}