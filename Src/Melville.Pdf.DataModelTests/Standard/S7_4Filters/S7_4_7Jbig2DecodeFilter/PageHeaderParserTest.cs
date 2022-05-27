using System;
using System.Buffers;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.SegmentParsers;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.Segments;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_4Filters.S7_4_7Jbig2DecodeFilter;

public class PageHeaderParserTest
{
    private static PageInformationSegment Parse(string data) => Parse(data.BitsFromHex());

    private static PageInformationSegment Parse(byte[] bits)
    {
        var reader = new SequenceReader<byte>(
            new ReadOnlySequence<byte>(
                bits
            )
        );
        return PageHeaderParser.Parse(ref reader, 210);
    }

    [Fact]
    public void FirstPageSegmentFromSample()
    {
        var sut = Parse("00 00 00 40 00 00 00 38 00 00 00 00 00 00 00 00 01 00 00");
        Assert.Equal(64u, sut.Width);
        Assert.Equal(56u, sut.Height);
        Assert.Equal(0u, sut.XResolution);
        Assert.Equal(0u, sut.YResolution);
        
        Assert.True(sut.Flags.Lossless);
        Assert.False(sut.Flags.HasRefinements);
        Assert.False(sut.Flags.DefaultValue);
        Assert.False(sut.Flags.AuxiliaryBuffers);
        Assert.False(sut.Flags.OverrideCombinator);
        Assert.Equal(CombinationOperator.Or, sut.Flags.DefaultOperator);
        Assert.False(sut.Striping.IsStriped);
        Assert.Equal(0, sut.Striping.StripeSize);
    }


    [Theory]
    [InlineData(CombinationOperator.Or)]
    [InlineData(CombinationOperator.And)]
    [InlineData(CombinationOperator.Xor)]
    [InlineData(CombinationOperator.Xnor)]
    public void CombOperator(CombinationOperator op)
    {
        var data = "00 00 00 40 00 00 00 38 00 00 00 00 00 00 00 00 00 00 00".BitsFromHex();
        var sut = Parse(data);
        Assert.Equal(CombinationOperator.Or, sut.Flags.DefaultOperator);
        
        data[^3] |= (byte)((byte)op << 3);
        sut = Parse(data);
        Assert.Equal(op, sut.Flags.DefaultOperator);
    }
/*    [Theory]
    [InlineData(PageInformationFlags.Lossless)]
    [InlineData(PageInformationFlags.HasRefinements)]
    [InlineData(PageInformationFlags.AuxiliaryBuffers)]
    [InlineData(PageInformationFlags.OverrideCombinator)]
    public void PageInformationFlagsTest(PageInformationFlags item)
    {
        var data = "00 00 00 40 00 00 00 38 00 00 00 00 00 00 00 00 00 00 00".BitsFromHex();
        var sut = Parse(data);
        Assert.False(sut.Flags.HasFlag(item));
        data[^3] |= (byte)item;
        sut = Parse(data);
        Assert.True(sut.Flags.HasFlag(item));
    }*/

    private void RunFlagTest(byte value, Func<PageInformationFlags, bool> selector)
    {
        var data = "00 00 00 40 00 00 00 38 00 00 00 00 00 00 00 00 00 00 00".BitsFromHex();
        var sut = Parse(data);
        Assert.False(selector(sut.Flags));
        data[^3] |= value;
        sut = Parse(data);
        Assert.True(selector(sut.Flags));
    }

    [Fact] public void LosslessBit() => RunFlagTest(0x1, i => i.Lossless);
    [Fact] public void RefinementBit() => RunFlagTest(0x2, i => i.HasRefinements);
    [Fact] public void DefaultValueBit() => RunFlagTest(0x4, i => i.DefaultValue);
    [Fact] public void AuxBufferBit() => RunFlagTest(0x20, i => i.AuxiliaryBuffers);
    [Fact] public void OverrideCombinatorBit() => RunFlagTest(0x40, i => i.OverrideCombinator);


    [Fact]
    public void StripingOp()
    {
        var data = "00 00 00 40 00 00 00 38 00 00 00 00 00 00 00 00 00 81 23".BitsFromHex();
        var sut = Parse(data);
        Assert.True(sut.Striping.IsStriped);
        Assert.Equal(0x123, sut.Striping.StripeSize);
        
    }
    [Fact]
    public void Resolution()
    {
        var data = "00000040 00000038 01234567 07654321 00 8123".BitsFromHex();
        var sut = Parse(data);
        Assert.Equal(0x01234567u, sut.XResolution);
        Assert.Equal(0x07654321u, sut.YResolution);
    }
}