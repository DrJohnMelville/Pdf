using System;
using System.Buffers;
using Melville.Pdf.LowLevel.Filters.FilterProcessing;
using Xunit;

namespace Melville.Pdf.DataModelTests.StreamUtilities;

public class EveryThirdByteStreamTest
{
    [Fact]
    public void Read3From9()
    {
        var src = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        var dest = new byte[10];
        var reader = new SequenceReader<byte>(new ReadOnlySequence<byte>(src));
        var (pos, written, done) = EveryThirdByteFilter.Instance.Convert(ref reader, dest.AsSpan());
        
        Assert.Equal(reader.Sequence.End, pos);
        Assert.Equal(3, written);
        Assert.False(done);
        Assert.Equal(3, dest[0]);
        Assert.Equal(6, dest[1]);
        Assert.Equal(9, dest[2]);
    }
    [Fact]
    public void Read2From8()
    {
        var src = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8};
        var dest = new byte[10];
        var reader = new SequenceReader<byte>(new ReadOnlySequence<byte>(src));
        var (pos, written, done) = EveryThirdByteFilter.Instance.Convert(ref reader, dest.AsSpan());
        
        Assert.Equal(reader.Sequence.GetPosition(6), pos);
        Assert.Equal(2, written);
        Assert.False(done);
        Assert.Equal(3, dest[0]);
        Assert.Equal(6, dest[1]);
    }
}