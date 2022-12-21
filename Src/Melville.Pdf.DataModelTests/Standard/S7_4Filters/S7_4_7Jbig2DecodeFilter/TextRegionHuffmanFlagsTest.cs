using System.Runtime.InteropServices;
using Melville.JBig2.HuffmanTables;
using Melville.JBig2.Segments;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_4Filters.S7_4_7Jbig2DecodeFilter;

public class TextRegionHuffmanFlagsTest
{
    [Theory]
    [InlineData(0,HuffmanTableSelection.B6)]
    [InlineData(1,HuffmanTableSelection.B7)]
    [InlineData(3,HuffmanTableSelection.UserSupplied)]
    public void HuffFSTest(ushort value, object result) =>
        Assert.Equal(result, new TextRegionHuffmanFlags(value).SbhuffFs);

    [Theory]
    [InlineData(0,HuffmanTableSelection.B8)]
    [InlineData(1<<2,HuffmanTableSelection.B9)]
    [InlineData(3<<2,HuffmanTableSelection.UserSupplied)]
    public void HuffDSTest(ushort value, object result) =>
        Assert.Equal(result, new TextRegionHuffmanFlags(value).SbhuffDs);

    [Theory]
    [InlineData(0,HuffmanTableSelection.B11)]
    [InlineData(1<<4,HuffmanTableSelection.B12)]
    [InlineData(3<<4,HuffmanTableSelection.UserSupplied)]
    public void HuffDtTest(ushort value, object result) =>
        Assert.Equal(result, new TextRegionHuffmanFlags(value).SbhuffDt);

    [Theory]
    [InlineData(0,HuffmanTableSelection.B14)]
    [InlineData(1<<6,HuffmanTableSelection.B15)]
    [InlineData(3<<6,HuffmanTableSelection.UserSupplied)]
    public void HuffRDWTest(ushort value, object result) =>
        Assert.Equal(result, new TextRegionHuffmanFlags(value).SbhuffRdw);
    
    [Theory]
    [InlineData(0,HuffmanTableSelection.B14)]
    [InlineData(1<<8,HuffmanTableSelection.B15)]
    [InlineData(3<<8,HuffmanTableSelection.UserSupplied)]
    public void HuffRDHTest(ushort value, object result) =>
        Assert.Equal(result, new TextRegionHuffmanFlags(value).SbhuffRdh);
    
    [Theory]
    [InlineData(0,HuffmanTableSelection.B14)]
    [InlineData(1<<10,HuffmanTableSelection.B15)]
    [InlineData(3<<10,HuffmanTableSelection.UserSupplied)]
    public void HuffRDXTest(ushort value, object result) =>
        Assert.Equal(result, new TextRegionHuffmanFlags(value).SbhuffRdx);

    [Theory]
    [InlineData(0,HuffmanTableSelection.B14)]
    [InlineData(1<<12,HuffmanTableSelection.B15)]
    [InlineData(3<<12,HuffmanTableSelection.UserSupplied)]
    public void HuffRDYTest(ushort value, object result) =>
        Assert.Equal(result, new TextRegionHuffmanFlags(value).SbhuffRdy);
    
    [Theory]
    [InlineData(0, HuffmanTableSelection.B1)]
    [InlineData(1<<14, HuffmanTableSelection.UserSupplied)]
    public void HuffRSizeText(ushort value, object result) =>
        Assert.Equal(result, new TextRegionHuffmanFlags(value).SbHuffRSize);

    
} 