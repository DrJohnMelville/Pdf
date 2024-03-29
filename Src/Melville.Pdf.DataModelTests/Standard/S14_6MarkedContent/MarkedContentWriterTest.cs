﻿using System.Threading.Tasks;
using Melville.Pdf.DataModelTests.Standard.S8_4GraphicState;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S14_6MarkedContent;

public class MarkedContentWriterTest: WriterTest
{
    [Fact]
    public async Task MarkedContentPointTestAsync()
    {
        sut.MarkedContentPoint("M1");
        Assert.Equal("/M1 MP\n", await WrittenTextAsync() );
        
    }

    [Fact]
    public async Task MarkedPointWithPropertyNameAsync()
    {
        await sut.MarkedContentPointAsync("M1", "M2");
        Assert.Equal("/M1 /M2 DP\n", await WrittenTextAsync());
        
    }

    [Fact]
    public async Task MarkedPointWithInlineDictionaryAsync()
    {
        await sut.MarkedContentPointAsync("M1", new DictionaryBuilder()
            .WithItem(KnownNames.Type, KnownNames.Catalog)
            .AsDictionary());
        Assert.Equal("/M1 <</Type/Catalog>>DP\n", await WrittenTextAsync());
        
    }

    [Fact]
    public async Task NamedMarkRangeAsync()
    {
        using (sut.BeginMarkedRange("M2"))
        {
            sut.MarkedContentPoint("M1");
        }
        Assert.Equal("/M2 BMC\n/M1 MP\nEMC\n", await WrittenTextAsync());
    }
 
    [Fact]
    public async Task NamedMarkRangeWithDictNameAsync()
    {
        using (await sut.BeginMarkedRangeAsync("M2", KnownNames.All))
        {
            sut.MarkedContentPoint("M1");
        }
        Assert.Equal("/M2 /All BDC\n/M1 MP\nEMC\n", await WrittenTextAsync());
    }
    
    [Fact]
    public async Task NamedMarkRangeWithInlineDictAsync()
    {
        using (await sut.BeginMarkedRangeAsync("M2", new DictionaryBuilder()
                   .WithItem(KnownNames.Type, KnownNames.Type)
                   .AsDictionary()))
        {
            sut.MarkedContentPoint("M1");
        }
        Assert.Equal("/M2 <</Type/Type>>BDC\n/M1 MP\nEMC\n", await WrittenTextAsync());
    }
    
    
}