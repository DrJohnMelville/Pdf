using System.Buffers;
using System.Security.Cryptography.X509Certificates;
using Melville.Icc.Parser;

namespace Melville.Icc.Model.Tags;

public class ViewingConditionsTag : ProfileData
{ 
    public XyzNumber IlluminantValue { get; }
    public XyzNumber SurroundValue { get; }
    public StandardIllumination IlluminantType { get; }
    public ViewingConditionsTag(ref SequenceReader<byte> reader)
    {
        reader.Skip32BitPad();
        IlluminantValue = reader.ReadXyzNumber();
        SurroundValue = reader.ReadXyzNumber();
        IlluminantType = (StandardIllumination)reader.ReadBigEndianUint32();
    }
}