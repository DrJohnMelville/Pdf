using System.Threading.Tasks;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Model.Objects;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_3;

public class S_7_3_10_IndirectObjectsDefined
{
    [Fact]
    public async Task ParseReference()
    {

        var src = "24 543 R".AsParsingSource();
        src.IndirectResolver.AddLocationHint(
            new IndirectObjectWithAccessor(24,543, () => new ValueTask<PdfObject>(PdfTokenValues.Null)));
        var result = (PdfIndirectObject)await src.ParseObjectAsync();
        Assert.Equal(24, result.ObjectNumber);
        Assert.Equal(543, result.GenerationNumber);
        Assert.Equal(PdfTokenValues.Null, await result.DirectValueAsync());
            
    }

    [Theory]
    [InlineData("true")]
    [InlineData("false")]
    [InlineData("null")]
    [InlineData("1234/")]
    [InlineData("1234.5678/")]
    [InlineData("(string value)")]
    [InlineData("[1 2 3 4]  ")]
    [InlineData("<1234>  ")]
    [InlineData("<</Foo (bar)>>  ")]
    public async Task DirectObjectValueDefinition(string targetAsPdf)
    {
        var obj = await targetAsPdf.ParseObjectAsync();
            
        Assert.True(ReferenceEquals(obj, await obj.DirectValueAsync()));

        var indirect = new PdfIndirectObject(1, 0, obj);
        Assert.True(ReferenceEquals(obj, await indirect.DirectValueAsync()));
    }
}