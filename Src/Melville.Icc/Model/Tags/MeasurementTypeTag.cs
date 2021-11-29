using System.Buffers;
using Melville.Icc.Parser;

namespace Melville.Icc.Model.Tags;

public enum StandardObserver: uint
{
    Unknown = 0,
    Cie1931 = 1,
    Cie1964 = 2
}

public enum MeasurmentGeomenty : uint
{
    Unkown = 0,
    a45 = 1,
    a0 = 2
}

public enum MeasurmentFlare: uint
{
    f0 = 0,
    f100 = 0x10000
}

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

public class MeasurementTypeTag: ProfileData
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