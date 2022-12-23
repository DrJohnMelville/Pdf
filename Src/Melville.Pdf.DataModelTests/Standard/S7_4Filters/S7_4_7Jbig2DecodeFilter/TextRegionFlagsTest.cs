using Melville.JBig2.Segments;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_4Filters.S7_4_7Jbig2DecodeFilter;

public class TextRegionFlagsTest
{
    [Theory]
    [InlineData(0x01, true)]
    [InlineData(0x00, false)]
    public void HuffmanTest(ushort value, bool useHuff)=>
        Assert.Equal(useHuff, new TextRegionFlags(value).UseHuffman);
    [Theory]
    [InlineData(0x02, true)]
    [InlineData(0x00, false)]
    public void RefineTest(ushort value, bool useRef)=>
        Assert.Equal(useRef, new TextRegionFlags(value).UsesRefinement);
    [Theory]
    [InlineData(0, 1)]
    [InlineData(1<<2, 2)]
    [InlineData(2<<2, 4)]
    [InlineData(3<<2, 8)]
    public void StripSizeTest(ushort value, int stripSize)=>
        Assert.Equal(stripSize, new TextRegionFlags(value).StripSize);
    
    [Theory]
    [InlineData(ReferenceCorner.BottomLeft, 0)]
    [InlineData(ReferenceCorner.TopLeft, 1<<4)]
    [InlineData(ReferenceCorner.BottomRight, 2<<4)]
    [InlineData(ReferenceCorner.TopRight, 3<<4)]
    public void ReferenceCornerTest(object result, ushort value) =>
        Assert.Equal(result, new TextRegionFlags(value).ReferenceCorner);
    [Theory]
    [InlineData(1<<6, true)]
    [InlineData(0x00, false)]
    public void TransposedTest(ushort value, bool useRef)=>
        Assert.Equal(useRef, new TextRegionFlags(value).Transposed);
    [Theory]
    [InlineData(CombinationOperator.Or, 0)]
    [InlineData(CombinationOperator.And, 1<<7)]
    [InlineData(CombinationOperator.Xor, 2<<7)]
    [InlineData(CombinationOperator.Xnor, 3<<7)]
    public void CombinationOperatorTest(object result, ushort value) =>
        Assert.Equal(result, new TextRegionFlags(value).CombinationOperator);

    [Theory]
    [InlineData(1<<9, true)]
    [InlineData(0x00, false)]
    public void DefaultPixelTest(ushort value, bool useRef)=>
        Assert.Equal(useRef, new TextRegionFlags(value).DefaultPixel);
    
    [Theory]
    [InlineData(0, 0)]
    [InlineData(1<<10, 1)]
    [InlineData(2<<10, 2)]
    [InlineData(15<<10, 15)]
    [InlineData(16<<10, -16)]
    [InlineData(31<<10, -1)]
    public void SbdOffset(ushort value, int offset)=>
        Assert.Equal(offset, new TextRegionFlags(value).DefaultCharacteSpacing);
    
    [Theory]
    [InlineData(1<<15, true)]
    [InlineData(0x00, false)]
    public void RefinementTemplate(ushort value, bool useRef)=>
        Assert.Equal(useRef, new TextRegionFlags(value).RefinementTemplate);
}