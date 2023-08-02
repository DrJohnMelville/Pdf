using Melville.Pdf.LowLevel.Filters.FlateFilters;
using Melville.Pdf.LowLevel.Model.Conventions;
using Xunit;

namespace Melville.Pdf.DataModelTests.Writer.Lzw;

public class AddlerTests
{
    [Fact]
    public void AddlerTest()
    {
        var sut = new Adler32Computer(1);
        sut.AddData(ExtendedAsciiEncoding.AsExtendedAsciiBytes("Wikipedia"));
        Assert.Equal(0x11e60398u, sut.GetHash());
                    
    }
        
    [Fact]
    public void AdlerHandlesMultipleOuterLoops()
    {
        var data = new byte[20000];
        for (int i = 0; i < 20000; i++)
        {
            data[i] = (byte) i;
        }
        
        var af = new Adler32Computer(1);
        af.AddData(data);
        Assert.Equal(2851790123u, af.GetHash());
                    
    }

}