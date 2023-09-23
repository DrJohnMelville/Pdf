using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Objects;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_3;

public class S7_3_1HelpersForObjects
{
    [Fact]
    public void ObjectIsAnArray()
    {
        var ret = ((PdfDirectObject)true).ObjectAsUnresolvedList();
        Assert.Single(ret);
        Assert.Equal(true, ret[0]);
    }
    [Fact]
    public void NullIsEmptyArray()
    {
        var ret = PdfDirectObject.CreateNull().ObjectAsUnresolvedList();
        Assert.Empty(ret);
    }
    [Fact]
    public async Task ArrayIsArrayAsync()
    {
        var ret = await new PdfArray(true, false).CastAsync<bool>();
        Assert.Equal(2, ret.Count);
        Assert.True(ret[0]);
        Assert.False(ret[1]);
    }
}