using System.Buffers;
using Melville.Icc.Parser;
using Melville.Parsing.SequenceReaders;

namespace Melville.Icc.Model.Tags;

/// <summary>
/// Describes the measurment units for a ResponseCurve
/// </summary>
public enum CurveMeasurement : uint
{
    /// <summary>
    /// Measurment  StatusA
    /// </summary>
    StatusA = 0x53746141,

    /// <summary>
    /// Measurment  StatusE
    /// </summary>
    StatusE = 0x53746145,

    /// <summary>
    /// Measurment  StatusI
    /// </summary>
    StatusI = 0x53746149,

    /// <summary>
    /// Measurment  StatusT
    /// </summary>
    StatusT = 0x53746154,

    /// <summary>
    /// Measurment  StatusM
    /// </summary>
    StatusM = 0x53745154,

    /// <summary>
    /// Measurment  Din
    /// </summary>
    Din = 0x434e2020,

    /// <summary>
    /// Measurment  DinPolarized
    /// </summary>
    DinPolarized = 0x434e2050,

    /// <summary>
    /// Measurment  DinNarrow
    /// </summary>
    DinNarrow = 0x434e4e20,

    /// <summary>
    /// Measurment  DinNarrowPolarized
    /// </summary>
    DinNarrowPolarized = 0x434e4e50
}

/// <summary>
/// A single element of a response curve
/// </summary>
/// <param name="DeviceValue">Value in device units</param>
/// <param name="MeasurementValue">Corresponding measuremment in the indicated unitis for this curve.</param>
public record struct Response16Number(ushort DeviceValue, float MeasurementValue);

/// <summary>
/// A channel in a ICC response curve set
/// </summary>
/// <param name="MaximumColorantValue">XYZ value of the maximum colorant value for this channel</param>
/// <param name="Response">Response data for a single channel</param>
public record struct ResponseCurveChannel(
    XyzNumber MaximumColorantValue,
    IReadOnlyList<Response16Number> Response);

/// <summary>
/// ICC response curve set
/// </summary>
/// <param name="Unit">The measurement unit for this response curve.</param>
/// <param name="Channels">The response curve for each channel of the transform</param>
public record struct ResponseCurve(
    CurveMeasurement Unit,
    IReadOnlyList<ResponseCurveChannel> Channels);

/// <summary>
/// Represents a set of 16 bit response curves.
/// </summary>
public class ResponseCurveSet16Tag
{
    /// <summary>
    /// The response curves that make up this set.
    /// </summary>
    public IReadOnlyList<ResponseCurve> Curves { get; }

    internal ResponseCurveSet16Tag(ref SequenceReader<byte> reader)
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