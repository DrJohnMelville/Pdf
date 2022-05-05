using System.Buffers;
using Melville.Icc.Parser;
using Melville.Parsing.SequenceReaders;

namespace Melville.Icc.Model.Tags;

public abstract class ProfileArray<T> 
{
    public IReadOnlyList<T> Values { get; }
    protected ProfileArray(ref SequenceReader<byte> reader)
    {
        reader.Skip32BitPad();
        var values = new T[(reader.Length - 8) / ItemSizeInBytes()];
        for (int i = 0; i < values.Length; i++)
        {
            values[i] = GetItem(ref reader);
        }
        Values = values;
    }

    protected abstract int ItemSizeInBytes();
    protected abstract T GetItem(ref SequenceReader<byte> reader);
}

public class U16Fixed16Array : ProfileArray<float>
{
    public U16Fixed16Array(ref SequenceReader<byte> reader): base(ref reader)
    {
    }

    protected override int ItemSizeInBytes() => 4;
    protected override float GetItem(ref SequenceReader<byte> reader) => reader.Readu16Fixed16();
}

public class UInt16Array : ProfileArray<ushort>
{
    public UInt16Array(ref SequenceReader<byte> reader): base(ref reader)
    {
    }

    protected override int ItemSizeInBytes() => 2;
    protected override ushort GetItem(ref SequenceReader<byte> reader) => reader.ReadBigEndianUint16();
}

public class UInt32Array : ProfileArray<uint>
{
    public UInt32Array(ref SequenceReader<byte> reader): base(ref reader)
    {
    }

    protected override int ItemSizeInBytes() => 4;
    protected override uint GetItem(ref SequenceReader<byte> reader) => reader.ReadBigEndianUint32();
}

public class UInt64Array : ProfileArray<ulong>
{
    public UInt64Array(ref SequenceReader<byte> reader): base(ref reader)
    {
    }

    protected override int ItemSizeInBytes() => 8;
    protected override ulong GetItem(ref SequenceReader<byte> reader) => reader.ReadBigEndianUint64();
}
public class XyzArray : ProfileArray<XyzNumber>
{
    public XyzArray(ref SequenceReader<byte> reader): base(ref reader)
    {
    }

    protected override int ItemSizeInBytes() => 12;
    protected override XyzNumber GetItem(ref SequenceReader<byte> reader) => reader.ReadXyzNumber();
}