using System.Buffers;
using Melville.Icc.Parser;

namespace Melville.Icc.Model.Tags;

public class CurveTag: ICurveTag
{
     private readonly IReadOnlyList<ushort> values;
     public IReadOnlyList<ushort> Values => values;

     public CurveTag(ref SequenceReader<byte> reader)
     {
          reader.Skip32BitPad();
          values = reader.ReadUshortArray((int)reader.ReadBigEndianUint32());
     }

     public float Evaluate(float input)
     {
          throw new NotImplementedException();
     }
}