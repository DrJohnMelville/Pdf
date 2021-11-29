using System.Buffers;
using System.Data.Common;
using Melville.Icc.Parser;

namespace Melville.Icc.Model.Tags;

public enum DataType : uint
{
    String = 0,
    Binary = 1,
}
public class DataTag: ProfileData{
    public DataType Type { get; }
    private byte[] data;
    public IReadOnlyList<byte> Data => data;

     public DataTag(ref SequenceReader<byte> reader)
     {
         reader.Skip32BitPad();
         Type = (DataType)reader.ReadBigEndianUint32();
         data = new byte[reader.Length - 12];
         for (int i = 0; i < data.Length; i++)
         {
             data[i] = reader.ReadBigEndianUint8();
         }
     }

     public String AsString() => String.Create(data.Length-1, data, CopyBytesToCharSpan );
     private void CopyBytesToCharSpan(Span<char> span, byte[] bytes)
     {
         for (int i = 0; i < span.Length; i++)
         {
             span[i] = (char)bytes[i];
         }
     }
}