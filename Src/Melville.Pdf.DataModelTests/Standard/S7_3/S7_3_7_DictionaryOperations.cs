using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_3;

public class S7_3_7_DictionaryOperations
{
    private async Task<PdfDictionary> CreateDictAsync(string definition) =>
        await (await definition.ParseValueObjectAsync()).LoadValueAsync<PdfDictionary>();


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

        Assert.Equal(new []{true, false}, await AllValuesAsync<bool>(d));
        Assert.Equal(new []{KnownNames.Height, KnownNames.AC},
            ((IEnumerable)d).OfType<KeyValuePair<PdfDirectObject,ValueTask<PdfDirectObject>>>().Select(i=>i.Key));
            
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
        Assert.Equal(true, await d[KnownNames.Height]);
    }
    [Fact]
    public async Task TryGetValueSucceedAsync()
    {
        var d = await IndirectTestDictAsync;
        AAssert.True(d.TryGetValue(KnownNames.Height, out var returned));
        Assert.Equal(true, await returned);
    }
    [Fact]
    public async Task TryGetValueFailsAsync()
    {
        var d = await IndirectTestDictAsync;
        AAssert.False(d.TryGetValue(KnownNames.Activation, out var returned));
        Assert.Equal(default(ValueTask<PdfDirectObject>), returned);
    }
    [Fact]
    public async Task EnumerateValuesHandlesIndirectReferencesAsync()
    {
        var d = await IndirectTestDictAsync;

        Assert.Equal(new[] { true, false }, await AllValuesAsync<bool>(d));
    }

    private async ValueTask<IEnumerable<T>> AllValuesAsync<T>(PdfDictionary d)
    {
        var ret = new List<T>();
        foreach (var item in d.Values)
        {
            ret.Add((await item).Get<T>());
        }

        return ret;
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