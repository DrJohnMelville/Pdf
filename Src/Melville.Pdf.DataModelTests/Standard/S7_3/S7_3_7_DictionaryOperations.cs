using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Model;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Xunit;
using PdfDictionary = Melville.Pdf.LowLevel.Model.Objects.PdfDictionary;

namespace Melville.Pdf.DataModelTests.Standard.S7_3;

public class S7_3_7_DictionaryOperations
{
    private async Task<PdfDictionary> CreateDict(string definition) =>
        (PdfDictionary) (await definition.ParseObjectAsync());


    private Task<PdfDictionary> IndirectTestDict => 
        CreateDict("<</Height true /AC false>>");

    [Fact]
    public async Task CountIsAccurate()
    {
        var d = await IndirectTestDict;
        Assert.Equal(2, d.Count);
    }
    [Fact]
    public async Task ContainsKeyWorks()
    {
        var d = await IndirectTestDict;

        Assert.True(d.ContainsKey(KnownNames.Height));
        Assert.False(d.ContainsKey(KnownNames.FormType));
    }
    [Fact]
    public async Task EnumerateHandlesIndirectReferences()
    {
        var d = await IndirectTestDict;

        Assert.Equal(new []{PdfBoolean.True, PdfBoolean.False}, 
            ((IEnumerable)d).OfType<KeyValuePair<PdfName,ValueTask<PdfObject>>>()
            .Select(i=>i.Value.Result));
        Assert.Equal(new []{KnownNames.Height, KnownNames.AC},
            ((IEnumerable)d).OfType<KeyValuePair<PdfName,ValueTask<PdfObject>>>().Select(i=>i.Key));
            
    }
    [Fact]
    public async Task EnumerateKeys()
    {
        var d = await IndirectTestDict;
        Assert.Equal(new []{KnownNames.Height, KnownNames.AC},d.Keys);
            
    }
    [Fact]
    public async Task IndexerHandlesIndirection()
    {
        var d = await IndirectTestDict;
        Assert.Equal(PdfBoolean.True, await d[KnownNames.Height]);
    }
    [Fact]
    public async Task TryGetValueSucceed()
    {
        var d = await IndirectTestDict;
        AAssert.True(d.TryGetValue(KnownNames.Height, out var returned));
        Assert.Equal(PdfBoolean.True, await returned);
    }
    [Fact]
    public async Task TryGetValueFails()
    {
        var d = await IndirectTestDict;
        AAssert.False(d.TryGetValue(KnownNames.Activation, out var returned));
        Assert.Equal(default(ValueTask<PdfObject>), returned);
    }
    [Fact]
    public async Task EnumerateValuesHandlesIndirectReferences()
    {
        var d = await IndirectTestDict;

        Assert.Equal(new []{PdfBoolean.True, PdfBoolean.False}, 
            d.Values.Select(i=>i.Result));
            
    }

    [Fact]
    public async Task DictionaryWithoutType()
    {
        Assert.Null((await IndirectTestDict).Type);
        Assert.Null((await IndirectTestDict).SubType);
    }
    [Fact]
    public async Task DictionaryWithType()
    {
        Assert.Equal(KnownNames.Image, (await CreateDict("<< /Type /Image >>")).Type);
    }
    [Fact]
    public async Task DictionaryWithSubType()
    {
        Assert.Equal(KnownNames.Image, (await CreateDict("<< /Subtype /Image >>")).SubType);
    }
    [Fact]
    public async Task DictionaryWithAbbreviatedType()
    {
        Assert.Equal(KnownNames.Image, (await CreateDict("<< /S /Image >>")).SubType);
    }
}