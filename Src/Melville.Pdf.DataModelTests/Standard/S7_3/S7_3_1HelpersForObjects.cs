using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Objects2;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_3;

public class S7_3_1HelpersForObjects
{
    [Fact]
    public void ObjectIsAnArray()
    {
        var ret = ((PdfDirectValue)true).ObjectAsUnresolvedList();
        Assert.Equal(1, ret.Count);
        Assert.Equal(true, ret[0]);
    }
    [Fact]
    public void NullIsEmptyArray()
    {
        var ret = PdfDirectValue.CreateNull().ObjectAsUnresolvedList();
        Assert.Equal(0, ret.Count);
    }
    [Fact]
    public async Task ArrayIsArray()
    {
        var ret = await new PdfValueArray(true, false).CastAsync<bool>();
        Assert.Equal(2, ret.Length);
        Assert.Equal(true, ret[0]);
        Assert.False(ret[1]);
    }
}