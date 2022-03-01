using System;
using Melville.Pdf.LowLevel.Filters.LzwFilter;
using Xunit;

namespace Melville.Pdf.DataModelTests.Writer.Lzw;

public class DecoderDictionaryTest
{
    private DecoderDictionary dict = new DecoderDictionary();
    [Theory]
    [InlineData(0x20,0, new byte[] {0x20, 0,0,0,0})]
    [InlineData(0x65,0, new byte[] {0x65, 0,0,0,0})]
    [InlineData(0x65,3, new byte[] {0, 0, 0,0x65,0})]
    public void WriteOneChar(int data, int position, byte[] result)
    {
        var dest = new byte[5];
        var span = dest.AsSpan().Slice(position);
        Assert.Equal(1,dict.WriteChars(data, 0, span));
        Assert.Equal(result, dest);
    }

    [Fact]
    public void DefineAValue()
    {
        Assert.False(dict.IsDefined(258));
        Assert.Equal(258,dict.AddChild(65, 66));
        Assert.True(dict.IsDefined(258));

    }
    [Fact]
    public void FirstByte()
    {
        Assert.Equal(258,dict.AddChild(65, 66));
        Assert.Equal(65, dict.FirstChar(65));
        Assert.Equal(65, dict.FirstChar(258));
        Assert.Equal(66, dict.FirstChar(66));
            

    }
    [Theory]
    [InlineData(0, 2, new byte[]{0, 0, 0,0,0}, 0)]
    [InlineData(0, 0, new byte[]{65, 66, 0,0,0}, 2)]
    [InlineData(0, 1, new byte[]{66, 0, 0,0,0}, 1)]
    [InlineData(4, 0, new byte[]{0, 0,0,0, 65}, 1)]
    [InlineData(3, 0, new byte[]{0, 0,0, 65, 66}, 2)]
    [InlineData(5, 0, new byte[]{0, 0,0,0, 0}, 0)]
    public void WriteTwoChars(int position, short sourceStart, byte[] result, int bytesWritten)
    {
        var dest = new byte[5];
        var span = dest.AsSpan().Slice(position);
        dict.AddChild(65, 66);
        Assert.Equal(bytesWritten,dict.WriteChars(258, sourceStart, span));
        Assert.Equal(result, dest);
    }
}