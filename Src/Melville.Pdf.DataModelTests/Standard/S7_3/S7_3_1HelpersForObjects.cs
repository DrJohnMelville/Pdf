using Melville.Pdf.LowLevel.Model.Objects;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_3;

public class S7_3_1HelpersForObjects
{
    [Fact]
    public void ObjectIsAnArray()
    {
        var ret = PdfBoolean.True.AsList();
        Assert.Equal(1, ret.Count);
        Assert.Equal(PdfBoolean.True, ret[0]);
    }
    [Fact]
    public void NullIsEmptyArray()
    {
        var ret = PdfTokenValues.Null.AsList();
        Assert.Equal(0, ret.Count);
    }
    [Fact]
    public void ArrayIsArray()
    {
        var ret = new PdfArray(PdfBoolean.True, PdfBoolean.False).AsList();
        Assert.Equal(2, ret.Count);
        Assert.Equal(PdfBoolean.True, ret[0]);
        Assert.Equal(PdfBoolean.False, ret[1]);
    }
}