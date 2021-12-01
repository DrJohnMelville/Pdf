using System.Buffers;
using Melville.Icc.Parser;

namespace Melville.Icc.Model.Tags;

public enum CurveMeasurement : uint
{
    StatusA = 0x53746141,
    StatusE = 0x53746145,
    StatusI = 0x53746149,
    StatusT = 0x53746154,
    StatusM = 0x53745154,
    Din = 0x434e2020,
    DinPolarized = 0x434e2050,
    DinNarrow = 0x434e4e20,
    DinNarrowPolarized = 0x434e4e50
}

public record struct Response16Number(ushort DeviceValue, float MeasurementValue);

public record struct ResponseCurveChannel(
    XyzNumber MaximumColorantValue,
    IReadOnlyList<Response16Number> response);

public record struct ResponseCurve(
    CurveMeasurement Unit,
    IReadOnlyList<ResponseCurveChannel> Channels);

public class ResponseCurveSet16Tag : ProfileData
{
    public IReadOnlyList<ResponseCurve> Curves { get; }
    public ResponseCurveSet16Tag(ref SequenceReader<byte> reader)
    {
        reader.VerifyInCorrectPositionForTagRelativeOffsets();
        reader.Skip32BitPad();
        var channels = reader.ReadBigEndianUint16();
        var curves = new ResponseCurve[reader.ReadBigEndianUint16()];
        for (int i = 0; i < curves.Length; i++)
        {
            curves[i] = ParseResponseCurve(reader.ReaderAt(reader.ReadBigEndianUint32()), channels);
        }
        Curves = curves;
    }

    private ResponseCurve ParseResponseCurve(SequenceReader<byte> reader, ushort chanels)
    {
        var unit = (CurveMeasurement)reader.ReadBigEndianUint32();
        var xyzReader = reader.ReaderAt((uint)(4 + (4 * chanels)));
        var dataReader = reader.ReaderAt((uint)(4 + (16 * chanels)));

        var chanelArray = new ResponseCurveChannel[chanels];
        for (int i = 0; i < chanels; i++)
        {
            var xyzNumber = xyzReader.ReadXyzNumber();
            chanelArray[i] = new ResponseCurveChannel(xyzNumber,
                ReadResponseNumbers(reader.ReadBigEndianUint32(), ref dataReader));
        }

        return new ResponseCurve(unit, chanelArray);
    }

    private IReadOnlyList<Response16Number> ReadResponseNumbers(
        uint count, ref SequenceReader<byte> reader)
    {
        var ret = new Response16Number[count];
        for (int i = 0; i < ret.Length; i++)
        {
            ret[i] = ReadSingleResponseNumber(ref reader);
        }

        return ret;
    }

    private static Response16Number ReadSingleResponseNumber(ref SequenceReader<byte> reader)
    {
        var device = reader.ReadBigEndianUint16();
        reader.Skip16BitPad();
        return new Response16Number(device, reader.Reads15Fixed16());
    }
}