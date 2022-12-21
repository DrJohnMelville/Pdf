using System.Buffers;
using Melville.Icc.Parser;
using Melville.Parsing.SequenceReaders;

namespace Melville.Icc.Model.Tags;

/// <summary>
/// Describes th=e observer used in color measurements
/// </summary>
public enum StandardObserver: uint
{
    Unknown = 0,
    Cie1931 = 1,
    Cie1964 = 2
}

/// <summary>
/// Describes the geometry used in measured color values
/// </summary>
public enum MeasurmentGeomenty : uint
{
    Unkown = 0,
    a45 = 1,
    a0 = 2
}

/// <summary>
/// Describes flare in color measurement values
/// </summary>
public enum MeasurmentFlare: uint
{
    f0 = 0,
    f100 = 0x10000
}

/// <summary>
/// Describes illumination used during a color measurement.
/// </summary>
public enum StandardIllumination : uint
{
    Unknown = 0,
    D50 = 1,
    D65 = 2,
    D93 = 3,
    F2 = 4,
    D55 = 5,
    A = 6,
    EquiPower = 7,
    F8 = 8
}

/// <summary>
/// Tjhe ICC? measurment type tag from the ICC spec.  This tag is informational only in this implementation.
/// </summary>
public class MeasurementTypeTag
{
    public StandardObserver Observer { get; }
    public XyzNumber MeasurementBacking { get; }
    public MeasurmentGeomenty Geometry { get; }
    public MeasurmentFlare Flare { get; }
    public StandardIllumination Illumination { get; }
    
    public MeasurementTypeTag(ref SequenceReader<byte> reader)
    {

        reader.Skip32BitPad();
        Observer = (StandardObserver)reader.ReadBigEndianUint32();
        MeasurementBacking = reader.ReadXyzNumber();
        Geometry = (MeasurmentGeomenty)reader.ReadBigEndianUint32();
        Flare = (MeasurmentFlare) reader.ReadBigEndianUint32();
        Illumination = (StandardIllumination)reader.ReadBigEndianUint32();
    }
}