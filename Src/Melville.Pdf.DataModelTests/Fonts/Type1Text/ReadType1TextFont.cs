using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using FluentAssertions;
using Melville.Fonts;
using Melville.Fonts.Type1TextParsers;
using Melville.Parsing.MultiplexSources;
using Melville.Parsing.ObjectRentals;
using Xunit;

namespace Melville.Pdf.DataModelTests.Fonts.Type1Text;

public class ReadType1TextFont
{
    private static IMultiplexSource GetFontText(
        string name, [CallerFilePath] string callerPath = "")
    {
        var fname = Path.Combine(Path.GetDirectoryName(callerPath)!, name);
        return MultiplexSourceFactory.Create(File.OpenRead(fname));
    }
    private static async Task<IGenericFont> ReadFontAsync(string name)
    {
        using var multiplexSource = GetFontText(name);
        var font = (await new Type1Parser(multiplexSource)
            .ParseAsync())[0];
        return font;
    }

    [Fact]
    public async Task ReadTrimmedAsync()
    {
        var font = await ReadFontAsync("Type1Text.fon");

        (await font.GetGlyphSourceAsync()).GlyphCount.Should().Be(4);
        (await font.GlyphNamesAsync()).Should().BeEquivalentTo(
            "n",".notdef","one","j");
        (await font.GetCmapSourceAsync()).Count.Should().Be(0);
        (await font.FontFamilyNameAsync()).Should().Be("GODCEN+Dingbats");
    }
    [Fact]
    public async Task REadFullType1Async()
    {
        var font = await ReadFontAsync("putr.pfa");

        (await font.GetGlyphSourceAsync()).GlyphCount.Should().Be(229);
        (await font.GetCmapSourceAsync()).Count.Should().Be(0);
    }

}