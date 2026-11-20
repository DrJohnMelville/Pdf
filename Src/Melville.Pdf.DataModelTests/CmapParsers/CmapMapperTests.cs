using System;
using Melville.Pdf.Model.Renderers.FontRenderings.CMaps;

namespace Melville.Pdf.DataModelTests.CmapParsers;

public class CmapMapperTests
{
    [Test]
    [Arguments(0x120, 1)]
    [Arguments(0x1201245, 1)]
    public void ConstantMapping(uint input, uint character)
    {
        var sut = new ConstantCMapper(new VariableBitChar(0x100), new VariableBitChar(0x1FF), 
            character);
        uint[] output = new uint[2];
        Assert.Equal(1, sut.WriteMapping(new VariableBitChar(input), output));
        Assert.Equal(character, output[0]);
    }

    [Test]
    public void ConstantMappingFail()
    {
        var sut = new ConstantCMapper(new VariableBitChar(0x100), new VariableBitChar(0x1FF),1);
        Assert.Equal(-1, sut.WriteMapping(SingleByteChar(12), Array.Empty<uint>()));
    }

    [Test]
    [Arguments(0, 1)]
    [Arguments(1,2)]
    [Arguments(11,12)]
    public void LinearMapping(byte input, uint character)
    {
        var sut = new LinearCMapper(new VariableBitChar(0x100), new VariableBitChar(0x1FF),
            character);
        uint[] output = new uint[2];
        Assert.Equal(1, sut.WriteMapping(SingleByteChar(input), output));
        Assert.Equal(character+input, output[0]);
    }

    [Test]
    [Arguments(0, false)]
    [Arguments(5, false)]
    [Arguments(9, false)]
    [Arguments(10, true)]
    [Arguments(11, true)]
    [Arguments(15, true)]
    [Arguments(19, true)]
    [Arguments(20, true)]
    [Arguments(21, false)]
    [Arguments(22, false)]
    [Arguments(220, false)]
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