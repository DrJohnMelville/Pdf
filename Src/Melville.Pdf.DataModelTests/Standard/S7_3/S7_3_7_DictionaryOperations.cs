using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Model;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Objects2;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_3;

public class S7_3_7_DictionaryOperations
{
    private async Task<PdfValueDictionary> CreateDictAsync(string definition) =>
        await (await definition.ParseValueObjectAsync()).LoadValueAsync<PdfValueDictionary>();


    private Task<PdfValueDictionary> IndirectTestDictAsync => 
        CreateDictAsync("<</Height true /AC false>>");

    [Fact]
    public async Task CountIsAccurateAsync()
    {
        var d = await IndirectTestDictAsync;
        Assert.Equal(2, d.Count);
    }
    [Fact]
    public async Task ContainsKeyWorksAsync()
    {
        var d = await IndirectTestDictAsync;

        Assert.True(d.ContainsKey(KnownNames.HeightTName));
        Assert.False(d.ContainsKey(KnownNames.FormTypeTName));
    }
    [Fact]
    public async Task EnumerateHandlesIndirectReferencesAsync()
    {
        var d = await IndirectTestDictAsync;

        Assert.Equal(new []{true, false}, 
            ((IEnumerable)d).OfType<KeyValuePair<PdfDirectValue,ValueTask<PdfObject>>>()
            .Select(i=>i.Value.Result));
        Assert.Equal(new []{KnownNames.HeightTName, KnownNames.ACTName},
            ((IEnumerable)d).OfType<KeyValuePair<PdfDirectValue,ValueTask<PdfObject>>>().Select(i=>i.Key));
            
    }
    [Fact]
    public async Task EnumerateKeysAsync()
    {
        var d = await IndirectTestDictAsync;
        Assert.Equal(new []{KnownNames.HeightTName, KnownNames.ACTName},d.Keys);
            
    }
    [Fact]
    public async Task IndexerHandlesIndirectionAsync()
    {
        var d = await IndirectTestDictAsync;
        Assert.Equal(true, await d[KnownNames.HeightTName]);
    }
    [Fact]
    public async Task TryGetValueSucceedAsync()
    {
        var d = await IndirectTestDictAsync;
        AAssert.True(d.TryGetValue(KnownNames.HeightTName, out var returned));
        Assert.Equal(true, await returned);
    }
    [Fact]
    public async Task TryGetValueFailsAsync()
    {
        var d = await IndirectTestDictAsync;
        AAssert.False(d.TryGetValue(KnownNames.ActivationTName, out var returned));
        Assert.Equal(default(ValueTask<PdfObject>), returned);
    }
    [Fact]
    public async Task EnumerateValuesHandlesIndirectReferencesAsync()
    {
        var d = await IndirectTestDictAsync;

        Assert.Equal(new []{true, false}, 
            d.Values.Select(i=>i.Result));
    }

    [Theory]
    [InlineData("<</Subtype /Font>>")]
    [InlineData("<</S /Font>>")]
    public async Task SisSubtypeEquivilenceAsync(string text)
    {
        var item = await CreateDictAsync(text);
        Assert.Equal(KnownNames.FontTName, item.SubTypeOrNull());
        Assert.True(item.TryGetSubType(out var st));
        Assert.Equal(KnownNames.FontTName, st);
        
    }
}