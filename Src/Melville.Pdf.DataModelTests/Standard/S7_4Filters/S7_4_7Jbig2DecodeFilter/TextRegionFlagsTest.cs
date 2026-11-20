using Melville.JBig2.Segments;

namespace Melville.Pdf.DataModelTests.Standard.S7_4Filters.S7_4_7Jbig2DecodeFilter;

public class TextRegionFlagsTest
{
    [Test]
    [Arguments(0x01, true)]
    [Arguments(0x00, false)]
    public void HuffmanTest(ushort value, bool useHuff)=>
        Assert.Equal(useHuff, new TextRegionFlags(value).UseHuffman);
    [Test]
    [Arguments(0x02, true)]
    [Arguments(0x00, false)]
    public void RefineTest(ushort value, bool useRef)=>
        Assert.Equal(useRef, new TextRegionFlags(value).UsesRefinement);
    [Test]
    [Arguments(0, 1)]
    [Arguments(1<<2, 2)]
    [Arguments(2<<2, 4)]
    [Arguments(3<<2, 8)]
    public void StripSizeTest(ushort value, int stripSize)=>
        Assert.Equal(stripSize, new TextRegionFlags(value).StripSize);
    
    [Test]
    [Arguments(ReferenceCorner.BottomLeft, 0)]
    [Arguments(ReferenceCorner.TopLeft, 1<<4)]
    [Arguments(ReferenceCorner.BottomRight, 2<<4)]
    [Arguments(ReferenceCorner.TopRight, 3<<4)]
    public void ReferenceCornerTest(object result, ushort value) =>
        Assert.Equal(result, new TextRegionFlags(value).ReferenceCorner);
    [Test]
    [Arguments(1<<6, true)]
    [Arguments(0x00, false)]
    public void TransposedTest(ushort value, bool useRef)=>
        Assert.Equal(useRef, new TextRegionFlags(value).Transposed);
    [Test]
    [Arguments(CombinationOperator.Or, 0)]
    [Arguments(CombinationOperator.And, 1<<7)]
    [Arguments(CombinationOperator.Xor, 2<<7)]
    [Arguments(CombinationOperator.Xnor, 3<<7)]
    public void CombinationOperatorTest(object result, ushort value) =>
        Assert.Equal(result, new TextRegionFlags(value).CombinationOperator);

    [Test]
    [Arguments(1<<9, true)]
    [Arguments(0x00, false)]
    public void DefaultPixelTest(ushort value, bool useRef)=>
        Assert.Equal(useRef, new TextRegionFlags(value).DefaultPixel);
    
    [Test]
    [Arguments(0, 0)]
    [Arguments(1<<10, 1)]
    [Arguments(2<<10, 2)]
    [Arguments(15<<10, 15)]
    [Arguments(16<<10, -16)]
    [Arguments(31<<10, -1)]
    public void SbdOffset(ushort value, int offset)=>
        Assert.Equal(offset, new TextRegionFlags(value).DefaultCharacteSpacing);
    
    [Test]
    [Arguments(1<<15, true)]
    [Arguments(0x00, false)]
    public void RefinementTemplate(ushort value, bool useRef)=>
        Assert.Equal(useRef, new TextRegionFlags(value).RefinementTemplate);
}