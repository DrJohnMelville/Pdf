using System.Buffers;
using Melville.Icc.Parser;
using Melville.Parsing.SequenceReaders;
using Melville.StreamInterfaces.ParsingBuffers;

namespace Melville.Icc.Model.Tags;

internal abstract class ProfileArray<T> 
{
    public IReadOnlyList<T> Values { get; }
    protected ProfileArray(ref SpanParsingBuffer reader)
    {
        reader.Skip32BitPad();
        var values = new T[(reader.RemainingLength) / ItemSizeInBytes()];
        for (int i = 0; i < values.Length; i++)
        {
            values[i] = GetItem(ref reader);
        }
        Values = values;
    }

    protected abstract int ItemSizeInBytes();
    protected abstract T GetItem(ref SpanParsingBuffer reader);
}

internal class U16Fixed16Array(ref SpanParsingBuffer reader) : ProfileArray<float>(ref reader)
{
    protected override int ItemSizeInBytes() => 4;
    protected override float GetItem(ref SpanParsingBuffer reader) => reader.Readu16Fixed16();
}

internal class UInt16Array(ref SpanParsingBuffer reader) : ProfileArray<ushort>(ref reader)
{
    protected override int ItemSizeInBytes() => 2;
    protected override ushort GetItem(ref SpanParsingBuffer reader) => reader.GetUInt16BigEndian();
}

internal class UInt32Array(ref SpanParsingBuffer reader) : ProfileArray<uint>(ref reader)
{
    protected override int ItemSizeInBytes() => 4;
    protected override uint GetItem(ref SpanParsingBuffer reader) => reader.GetUInt32BigEndian();
}

internal class UInt64Array(ref SpanParsingBuffer reader) : ProfileArray<ulong>(ref reader)
{
    protected override int ItemSizeInBytes() => 8;
    protected override ulong GetItem(ref SpanParsingBuffer reader) => reader.GetUInt64BigEndian();
}
internal class XyzArray(ref SpanParsingBuffer reader) : ProfileArray<XyzNumber>(ref reader)
{
    protected override int ItemSizeInBytes() => 12;
    protected override XyzNumber GetItem(ref SpanParsingBuffer reader) => reader.ReadXyzNumber();
}