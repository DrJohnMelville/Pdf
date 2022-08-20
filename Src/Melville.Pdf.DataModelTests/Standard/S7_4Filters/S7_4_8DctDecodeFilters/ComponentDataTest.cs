using Melville.Pdf.LowLevel.Filters.Jpeg;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_4Filters.S7_4_8DctDecodeFilters;

public class ComponentDataTest
{
    [Theory]
    [InlineData(1,0,0)]
    [InlineData(1,1,1)]
    public void DecodeNumberTest(int len, int code, int value) =>
        Assert.Equal(value, ComponentData.DecodeNumber(len, code));
}