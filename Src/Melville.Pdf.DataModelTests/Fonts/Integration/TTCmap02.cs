using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using FluentAssertions;
using Melville.Fonts;
using Melville.Parsing.MultiplexSources;
using Xunit;

namespace Melville.Pdf.DataModelTests.Fonts.Integration;

public class TTCmap02
{
    private string FontFileName([CallerFilePath] string path = null!) =>
        path.Replace(".cs", ".ttf");

    [Fact]
    public async Task Type0Cmap()
    {
        var src = MultiplexSourceFactory.Create(
            File.Open(FontFileName(), FileMode.Open, FileAccess.Read, FileShare.Read));
        var font = (await RootFontParser.ParseAsync(src))[0];
        var map = await (await font.ParseCMapsAsync()).GetByIndexAsync(1);

        map.Map(0).Should().Be(0);
        map.Map(0x1F).Should().Be(0);
        map.Map(0x20).Should().Be(3);
        map.Map(0x21).Should().Be(4);
        map.Map(0x7E).Should().Be(97);
        map.Map(0xA1).Should().Be(131);
        map.Map(0xB5).Should().Be(151);
        map.Map(0xCA).Should().Be(3);
    }

}