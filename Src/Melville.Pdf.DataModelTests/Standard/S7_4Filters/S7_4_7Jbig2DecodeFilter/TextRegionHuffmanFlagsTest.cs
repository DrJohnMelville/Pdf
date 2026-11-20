using Melville.JBig2.HuffmanTables;
using Melville.JBig2.Segments;

namespace Melville.Pdf.DataModelTests.Standard.S7_4Filters.S7_4_7Jbig2DecodeFilter;

public class TextRegionHuffmanFlagsTest
{
    [Test]
    [Arguments(0,HuffmanTableSelection.B6)]
    [Arguments(1,HuffmanTableSelection.B7)]
    [Arguments(3,HuffmanTableSelection.UserSupplied)]
    public void HuffFSTest(ushort value, object result) =>
        Assert.Equal(result, new TextRegionHuffmanFlags(value).SbhuffFs);

    [Test]
    [Arguments(0,HuffmanTableSelection.B8)]
    [Arguments(1<<2,HuffmanTableSelection.B9)]
    [Arguments(3<<2,HuffmanTableSelection.UserSupplied)]
    public void HuffDSTest(ushort value, object result) =>
        Assert.Equal(result, new TextRegionHuffmanFlags(value).SbhuffDs);

    [Test]
    [Arguments(0,HuffmanTableSelection.B11)]
    [Arguments(1<<4,HuffmanTableSelection.B12)]
    [Arguments(3<<4,HuffmanTableSelection.UserSupplied)]
    public void HuffDtTest(ushort value, object result) =>
        Assert.Equal(result, new TextRegionHuffmanFlags(value).SbhuffDt);

    [Test]
    [Arguments(0,HuffmanTableSelection.B14)]
    [Arguments(1<<6,HuffmanTableSelection.B15)]
    [Arguments(3<<6,HuffmanTableSelection.UserSupplied)]
    public void HuffRDWTest(ushort value, object result) =>
        Assert.Equal(result, new TextRegionHuffmanFlags(value).SbhuffRdw);
    
    [Test]
    [Arguments(0,HuffmanTableSelection.B14)]
    [Arguments(1<<8,HuffmanTableSelection.B15)]
    [Arguments(3<<8,HuffmanTableSelection.UserSupplied)]
    public void HuffRDHTest(ushort value, object result) =>
        Assert.Equal(result, new TextRegionHuffmanFlags(value).SbhuffRdh);
    
    [Test]
    [Arguments(0,HuffmanTableSelection.B14)]
    [Arguments(1<<10,HuffmanTableSelection.B15)]
    [Arguments(3<<10,HuffmanTableSelection.UserSupplied)]
    public void HuffRDXTest(ushort value, object result) =>
        Assert.Equal(result, new TextRegionHuffmanFlags(value).SbhuffRdx);

    [Test]
    [Arguments(0,HuffmanTableSelection.B14)]
    [Arguments(1<<12,HuffmanTableSelection.B15)]
    [Arguments(3<<12,HuffmanTableSelection.UserSupplied)]
    public void HuffRDYTest(ushort value, object result) =>
        Assert.Equal(result, new TextRegionHuffmanFlags(value).SbhuffRdy);
    
    [Test]
    [Arguments(0, HuffmanTableSelection.B1)]
    [Arguments(1<<14, HuffmanTableSelection.UserSupplied)]
    public void HuffRSizeText(ushort value, object result) =>
        Assert.Equal(result, new TextRegionHuffmanFlags(value).SbHuffRSize);

    
} 