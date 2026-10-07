using System.Buffers;
using Melville.Icc.Parser;
using Melville.Parsing.SequenceReaders;
using Melville.StreamInterfaces.ParsingBuffers;

namespace Melville.Icc.Model.Tags;

/// <summary>
/// Tjhe ICC? measurment type tag from the ICC spec.  This tag is informational only in this implementation.
/// </summary>
public class MeasurementTypeTag
{
    /// <summary>
    /// The observer specification used in defining this profile.
    /// </summary>
    public StandardObserver Observer { get; }
    /// <summary>
    /// Backing color for the measurement
    /// </summary>
    public XyzNumber MeasurementBacking { get; }
    /// <summary>
    /// Measurement geometry for the profile creation
    /// </summary>
    public MeasurmentGeomenty Geometry { get; }
    /// <summary>
    /// Measurement flare used in creating the profile
    /// </summary>
    public MeasurmentFlare Flare { get; }
    /// <summary>
    /// Illumination model used in creating the profile.
    /// </summary>
    public StandardIllumination Illumination { get; }
    
    internal MeasurementTypeTag(ref SpanParsingBuffer reader)
    {

        reader.Skip32BitPad();
        Observer = (StandardObserver)reader.GetUInt32BigEndian();
        MeasurementBacking = reader.ReadXyzNumber();
        Geometry = (MeasurmentGeomenty)reader.GetUInt32BigEndian();
        Flare = (MeasurmentFlare) reader.GetUInt32BigEndian();
        Illumination = (StandardIllumination)reader.GetUInt32BigEndian();
    }
}