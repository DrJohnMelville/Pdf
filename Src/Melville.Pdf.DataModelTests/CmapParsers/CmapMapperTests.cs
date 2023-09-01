using System;
using Melville.Pdf.Model.Renderers.FontRenderings.CMaps;
using Melville.SharpFont;
using Xunit;

namespace Melville.Pdf.DataModelTests.CmapParsers;

public class CmapMapperTests
{
    [Theory]
    [InlineData(0x120, 1)]
    [InlineData(0x1201245, 1)]
    public void ConstantMapping(uint input, uint character)
    {
        var sut = new ConstantCMapper(new VariableBitChar(0x100), new VariableBitChar(0x1FF), 
            character);
        Span<uint> output = stackalloc uint[2];
        Assert.Equal(1, sut.WriteMapping(new VariableBitChar(input), output));
        Assert.Equal(character, output[0]);
    }

    [Fact]
    public void ConstantMappingFail()
    {
        var sut = new ConstantCMapper(new VariableBitChar(0x100), new VariableBitChar(0x1FF),1);
        Assert.Equal(-1, sut.WriteMapping(SingleByteChar(12), Array.Empty<uint>().AsSpan()));
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(1,2)]
    [InlineData(11,12)]
    public void LinearMapping(byte input, uint character)
    {
        var sut = new LinearCMapper(new VariableBitChar(0x100), new VariableBitChar(0x1FF),
            character);
        Span<uint> output = stackalloc uint[2];
        Assert.Equal(1, sut.WriteMapping(SingleByteChar(input), output));
        Assert.Equal(character+input, output[0]);
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(5, false)]
    [InlineData(9, false)]
    [InlineData(10, true)]
    [InlineData(11, true)]
    [InlineData(15, true)]
    [InlineData(19, true)]
    [InlineData(20, true)]
    [InlineData(21, false)]
    [InlineData(22, false)]
    [InlineData(220, false)]
    public void RangeTest(byte character, bool result)
    {
        Assert.Equal(result, new LinearCMapper(new VariableBitChar("\x0A"u8), 
            new VariableBitChar(
                "\x14"u8), 2).AppliesTo(SingleByteChar(character)));
    }

    private static VariableBitChar SingleByteChar(byte character)
    {
        return new VariableBitChar((uint)(1<<8)|character);
    }
}