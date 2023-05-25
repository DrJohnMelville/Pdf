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
    private async Task<PdfDictionary> CreateDictAsync(string definition) =>
        (PdfDictionary) (await definition.ParseObjectAsync());


    private Task<PdfDictionary> IndirectTestDictAsync => 
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

        Assert.True(d.ContainsKey(KnownNames.Height));
        Assert.False(d.ContainsKey(KnownNames.FormType));
    }
    [Fact]
    public async Task EnumerateHandlesIndirectReferencesAsync()
    {
        var d = await IndirectTestDictAsync;

        Assert.Equal(new []{PdfBoolean.True, PdfBoolean.False}, 
            ((IEnumerable)d).OfType<KeyValuePair<PdfName,ValueTask<PdfObject>>>()
            .Select(i=>i.Value.Result));
        Assert.Equal(new []{KnownNames.Height, KnownNames.AC},
            ((IEnumerable)d).OfType<KeyValuePair<PdfName,ValueTask<PdfObject>>>().Select(i=>i.Key));
            
    }
    [Fact]
    public async Task EnumerateKeysAsync()
    {
        var d = await IndirectTestDictAsync;
        Assert.Equal(new []{KnownNames.Height, KnownNames.AC},d.Keys);
            
    }
    [Fact]
    public async Task IndexerHandlesIndirectionAsync()
    {
        var d = await IndirectTestDictAsync;
        Assert.Equal(PdfBoolean.True, await d[KnownNames.Height]);
    }
    [Fact]
    public async Task TryGetValueSucceedAsync()
    {
        var d = await IndirectTestDictAsync;
        AAssert.True(d.TryGetValue(KnownNames.Height, out var returned));
        Assert.Equal(PdfBoolean.True, await returned);
    }
    [Fact]
    public async Task TryGetValueFailsAsync()
    {
        var d = await IndirectTestDictAsync;
        AAssert.False(d.TryGetValue(KnownNames.Activation, out var returned));
        Assert.Equal(default(ValueTask<PdfObject>), returned);
    }
    [Fact]
    public async Task EnumerateValuesHandlesIndirectReferencesAsync()
    {
        var d = await IndirectTestDictAsync;

        Assert.Equal(new []{PdfBoolean.True, PdfBoolean.False}, 
            d.Values.Select(i=>i.Result));
    }

    [Theory]
    [InlineData("<</Subtype /Font>>")]
    [InlineData("<</S /Font>>")]
    public async Task SisSubtypeEquivilenceAsync(string text)
    {
        var item = await CreateDictAsync(text);
        Assert.Equal(KnownNames.Font, item.SubTypeOrNull());
        Assert.True(item.TryGetSubType(out var st));
        Assert.Equal(KnownNames.Font, st);
        
    }
}