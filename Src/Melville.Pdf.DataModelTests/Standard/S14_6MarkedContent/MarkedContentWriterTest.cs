using System.Threading.Tasks;
using Melville.Pdf.DataModelTests.Standard.S8_4GraphicState;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Writers;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S14_6MarkedContent;

public class MarkedContentWriterTest: WriterTest
{
    [Fact]
    public async Task MarkedContentPointTest()
    {
        sut.MarkedContentPoint("M1");
        Assert.Equal("/M1 MP\n", await WrittenText() );
        
    }

    [Fact]
    public async Task MarkedPointWithPropertyName()
    {
        await sut.MarkedContentPointAsync("M1", "M2");
        Assert.Equal("/M1 /M2 DP\n", await WrittenText());
        
    }

    [Fact]
    public async Task MarkedPointWithInlineDictionary()
    {
        await sut.MarkedContentPointAsync("M1", new DictionaryBuilder()
            .WithItem(KnownNames.Type, KnownNames.Catalog)
            .AsDictionary());
        Assert.Equal("/M1 <</Type/Catalog>>DP\n", await WrittenText());
        
    }

    [Fact]
    public async Task NamedMarkRange()
    {
        using (sut.BeginMarkedRange("M2"))
        {
            sut.MarkedContentPoint("M1");
        }
        Assert.Equal("/M2 BMC\n/M1 MP\nEMC\n", await WrittenText());
    }
 
    [Fact]
    public async Task NamedMarkRangeWithDictName()
    {
        using (await sut.BeginMarkedRangeAsync("M2", KnownNames.All))
        {
            sut.MarkedContentPoint("M1");
        }
        Assert.Equal("/M2 /All BDC\n/M1 MP\nEMC\n", await WrittenText());
    }
    
    [Fact]
    public async Task NamedMarkRangeWithInlineDict()
    {
        using (await sut.BeginMarkedRangeAsync("M2", new DictionaryBuilder()
                   .WithItem(KnownNames.Type, KnownNames.Type)
                   .AsDictionary()))
        {
            sut.MarkedContentPoint("M1");
        }
        Assert.Equal("/M2 <</Type/Type>>BDC\n/M1 MP\nEMC\n", await WrittenText());
    }
    
    
}