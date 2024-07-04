using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using FluentAssertions;
using Melville.Fonts;
using Melville.Fonts.Type1TextParsers;
using Melville.Parsing.MultiplexSources;
using Xunit;

namespace Melville.Pdf.DataModelTests.Fonts.Type1Text;

public class ReadType1TextFont()
{
    private static readonly byte[] fontText = GetFontText();

    private static byte[] GetFontText([CallerFilePath] string callerPath = "")
    {
        var fname = Path.Combine(Path.GetDirectoryName(callerPath)!, "Type1Text.fon");
        return File.ReadAllBytes(fname);
    }

    [Fact]
    public async Task GlyphCountAsync()
    {
        var font = await ReadFontAsync();

        (await font.GetGlyphSourceAsync()).GlyphCount.Should().Be(4);
        (await font.GlyphNamesAsync()).Should().BeEquivalentTo(
            "n",".notdef","one","j");
        (await font.GetCmapSourceAsync()).Count.Should().Be(0);
    }

    private static async Task<IGenericFont> ReadFontAsync()
    {
        var font = (await new Type1Parser(MultiplexSourceFactory.Create(fontText))
            .ParseAsync())[0];
        return font;
    }
}