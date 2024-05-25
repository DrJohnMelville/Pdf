using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Melville.Fonts;
using Melville.Fonts.SfntParsers.TableDeclarations.CMaps;
using Xunit;

namespace Melville.Pdf.DataModelTests.Fonts.Integration;

public class TTCmap02
{
    private static Task<IGenericFont> loadedFont = IntegrationFontLoader.LoadAsync();

    private async Task<ICmapImplementation> LoadCmapByIndexAsync(int index) => 
        (await (await loadedFont).CmapByIndexAsync(index))!;

    [Fact]
    public async Task Type0CmapAsync()
    {
        var map = await LoadCmapByIndexAsync(1);

        map.Map(0).Should().Be(0);
        map.Map(0x1F).Should().Be(0);
        map.Map(0x20).Should().Be(3);
        map.Map(0x21).Should().Be(4);
        map.Map(0x7E).Should().Be(97);
        map.Map(0xA1).Should().Be(131);
        map.Map(0xB5).Should().Be(151);
        map.Map(0xCA).Should().Be(3);
    }

    [Fact]
    public async Task Type2CmapAsync()
    {
        var map = await LoadCmapByIndexAsync(0);

        map.Map(0).Should().Be(0);
        map.Map(0x1F).Should().Be(0);
        map.Map(0x20).Should().Be(3);
        map.Map(0x21).Should().Be(4);
        map.Map(0x7E).Should().Be(97);
        map.Map(0xA0).Should().Be(3);
        map.Map(0xAD).Should().Be(16);
        map.Map(0xB0).Should().Be(131);
        map.Map(0xB2).Should().Be(240);
        map.Map(0xB5).Should().Be(151);
        map.Map(0xD7).Should().Be(238);
        map.Map(0x37E).Should().Be(30);
    }

    [Fact]  
    public async Task EnumerateType2CmapAsync()
    {
       var map = await LoadCmapByIndexAsync(0);

       var mappings = map.AllMappings().Where(i => i.Glyph > 0).ToList();
       mappings.Should().Contain((1, 0x20, 3));
       mappings.Should().Contain((1, 0x21, 4));
       mappings.Should().Contain((1, 0x7E, 97));
       mappings.Should().Contain((1, 0xA0, 3));
       mappings.Should().Contain((1, 0xAD, 16));
       mappings.Should().Contain((1, 0xB0, 131));
       mappings.Should().Contain((1, 0xB2, 240));
       mappings.Should().Contain((1, 0xB5, 151));
       mappings.Should().Contain((1, 0xD7, 238));
       mappings.Should().Contain((2, 0x37E, 30));
    }
}