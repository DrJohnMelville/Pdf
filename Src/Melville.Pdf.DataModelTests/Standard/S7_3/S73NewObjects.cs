using Melville.Pdf.LowLevel.Model.Objects2;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_3;

public class S73NewObjects
{
    [Theory]
    [InlineData(true, "true")]
    [InlineData(false, "false")]
    public void CreateBooleanValue(bool value, string str)
    {
        var pdfValue = ((PdfDirectValue)value);
        Assert.Equal(value, pdfValue.Get<bool>());
        Assert.Equal(str, pdfValue.Get<string>());
        Assert.Equal(str, pdfValue.ToString());

        Assert.True(pdfValue.IsBool);
        Assert.False(pdfValue.IsNull);
    }

    [Fact]
    public void NullTests()
    {
        PdfDirectValue value = default;
        Assert.Equal("null", value.ToString());
        Assert.Equal("null", value.Get<string>());
    }
}