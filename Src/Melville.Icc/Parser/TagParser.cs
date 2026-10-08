using System.Buffers;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Melville.Icc.Model.Tags;
using Melville.Parsing.CountingReaders;
using Melville.Parsing.SequenceReaders;
using Melville.StreamInterfaces.ParsingBuffers;

namespace Melville.Icc.Parser;

internal static class TagParser
{
    public static object Parse(ReadOnlySequence<byte> input)
    {
        var reader = new SequenceReader<byte>(input);
        return Parse(ref reader);
    }

    public static object Parse(ref SpanParsingBuffer reader)
    { // this is a terrible transition hack
        var buffer = reader.Peek().ToArray();
        var inner = new SequenceReader<byte>(new ReadOnlySequence<byte>(buffer));
        var ret = Parse(ref inner);
        var consumed = buffer.Length - inner.Length;
        reader.Advance((int)consumed);
        return ret;
    }
   
    public static object Parse(ref SequenceReader<byte> reader)
    {
        var reader2 = reader.AsSpanParsingBuffer();
        reader2.Skip32BitPad();
        try
        {

            return reader.GetUInt32BigEndian() switch
            {
                IccTags.bACS or IccTags.eACS => NullColorTransform.Parse(ref reader2),
                IccTags.chrm => new ChromacityTag(ref reader2),
                IccTags.clro => new ColorantOrderTag(ref reader2),
                IccTags.clrt => new ColorantTableTag(ref reader2),
                IccTags.clut => new MultidimensionalLookupTable(ref reader2),
                IccTags.curv => CurveTagParser.Parse(ref reader2),
                IccTags.curf => new MultiProcessCurve(ref reader2),
                IccTags.cvst => new MultiProcessCurveSet(ref reader2),
                IccTags.data => new DataTag(ref reader2),
                IccTags.dtim => new DateTimeTag(ref reader2),
                IccTags.matf => MultiProcessMatrix.Parse(ref reader2),
                IccTags.mft2 => new LutXTag(ref reader2, 2),
                IccTags.mft1 => new LutXTag(ref reader2, 1),
                IccTags.meas => new MeasurementTypeTag(ref reader2),
                IccTags.mAB => new LutAToBTag(ref reader2),
                IccTags.mBA => new LutBToATag(ref reader2),
                IccTags.mpet => new MultiProcessTag(ref reader),
                IccTags.mluc => new MultiLocalizedUnicodeTag(ref reader2),
                IccTags.ncl2 => new NamedColorTag(ref reader2),
                IccTags.para => new ParametricCurveTag(ref reader2),
                IccTags.parf => ParseCurveSegment(ref reader2),
                IccTags.pseq => new ProfileSequenceDescriptionTag(ref reader2),
                IccTags.psid => new ProfileSequenceIdentifierTag(ref reader2),
                IccTags.rcs2 => new ResponseCurveSet16Tag(ref reader2),
                IccTags.samf => new SampledCurveSegment(ref reader2),
                IccTags.sf32 => new S15Fixed16Array(ref reader2),
                IccTags.sig => new SignatureTag(ref reader2),
                IccTags.text => new TextTag(ref reader2),
                IccTags.uf32 => new U16Fixed16Array(ref reader2),
                IccTags.ui16 => new UInt16Array(ref reader2),
                IccTags.ui32 => new UInt32Array(ref reader2),
                IccTags.ui64 => new UInt64Array(ref reader2),
                IccTags.XYZ => new XyzArray(ref reader2),
                IccTags.view => new ViewingConditionsTag(ref reader2),
                var x => UnRecognizedTransform(x) // Some profiles have invalid tags that we do not care about.
            };
        }
        finally
        {
            if (reader2.RelativePosition > 4)
                reader.Advance(reader2.RelativePosition -4);
        }
    }

    private static NullColorTransform UnRecognizedTransform(uint u)
    {
        var strm = Encoding.UTF8.GetString(new byte[]
        {
            (byte)(u >> 24),
            (byte)(u >> 16),
            (byte)(u >> 8),
            (byte)(u),
        });
        return NullColorTransform.Instance(3);
    }

    private static object ParseCurveSegment(ref SpanParsingBuffer reader)
    {
        reader.Skip32BitPad();
        var fType = reader.GetUInt16BigEndian();
        reader.Skip16BitPad();
        return fType switch
        {
            0 => new FormulaSegmentType0(ref reader),
            1 => new FormulaSegmentType1(ref reader),
            2 => new FormulaSegmentType2(ref reader),
            _=> throw new InvalidDataException("Unknown curve segment type")
        };
    }
}