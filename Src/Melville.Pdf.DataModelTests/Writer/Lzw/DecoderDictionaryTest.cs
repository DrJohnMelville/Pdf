using System;
using Melville.Pdf.LowLevel.Filters.LzwFilter;

namespace Melville.Pdf.DataModelTests.Writer.Lzw;

public class DecoderDictionaryTest
{
    private DecoderDictionary dict = new DecoderDictionary();
    [Test]
    [Arguments(0x20,0, new byte[] {0x20, 0,0,0,0})]
    [Arguments(0x65,0, new byte[] {0x65, 0,0,0,0})]
    public void WriteOneChar(int data, int position, byte[] result)
    {
        var dest = new byte[5];
        var span = dest.AsSpan().Slice(position);
        var end = span;
        Assert.Equal(data,dict.WriteChars(data, ref end, -1));
        Assert.SeqEqual(result, dest);
    }

    [Test]
    public void DefineAValue()
    {
        Assert.False(dict.IsDefined(258));
        Assert.Equal(258,dict.AddChild(65, 66));
        Assert.True(dict.IsDefined(258));

    }
    [Test]
    public void FirstByte()
    {
        Assert.Equal(258,dict.AddChild(65, 66));
        Assert.Equal(65, dict.FirstChar(65));
        Assert.Equal(65, dict.FirstChar(258));
        Assert.Equal(66, dict.FirstChar(66));
            

    }
    [Test]
    [Arguments(258, new byte[]{0, 0, 0,0,0}, 0)]
    [Arguments(-1, new byte[]{65, 66, 0,0,0}, 2)]
    [Arguments(65, new byte[]{66, 0, 0,0,0}, 1)]
    public void WriteTwoChars(short lastWritten, byte[] result, int bytesWritten)
    {
        var dest = new byte[5];
        var end = dest.AsSpan();
        dict.AddChild(65, 66);
        Assert.Equal(258,dict.WriteChars(258, ref end, lastWritten));
        Assert.SeqEqual(result, dest);
        Assert.Equal(dest.Length - end.Length, bytesWritten);
    }
}