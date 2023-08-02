using System.Buffers;
using Melville.Parsing.SequenceReaders;

namespace Melville.Icc.Model.Tags;

/// <summary>
/// Represents the format of data in an ICC Data Tag
/// </summary>
public enum DataType : uint
{
    /// <summary>
    /// Data type contains 7 bit ascii data in 8 bit bytes
    /// </summary>
    String = 0,
    /// <summary>
    /// Data type is an unconstrained stream of 8 bit bytes.
    /// </summary>
    Binary = 1,
}

/// <summary>
/// This ICC tag allows additional, unconstrained data to be included in an ICC profile.
/// </summary>
public class DataTag{
    /// <summary>
    /// Describes the kind of data contained in this tag.
    /// </summary>
    public DataType Type { get; }
    private readonly byte[] data;
    /// <summary>
    /// The data contained in the tag.
    /// </summary>
    public IReadOnlyList<byte> Data => data;

     internal DataTag(ref SequenceReader<byte> reader)
     {
         reader.Skip32BitPad();
         Type = (DataType)reader.ReadBigEndianUint32();
         data = new byte[reader.Length - 12];
         for (int i = 0; i < data.Length; i++)
         {
             data[i] = reader.ReadBigEndianUint8();
         }
     }

     /// <summary>
     /// Display the data as an ascii formatted string.
     /// </summary>
     public String AsString() => String.Create(data.Length-1, data, CopyBytesToCharSpan );
     private void CopyBytesToCharSpan(Span<char> span, byte[] bytes)
     {
         for (int i = 0; i < span.Length; i++)
         {
             span[i] = (char)bytes[i];
         }
     }
}