using FluentAssertions;
using Melville.Fonts;
using Melville.Fonts.Type1TextParsers;
using Melville.Fonts.Type1TextParsers.EexecDecoding;
using Melville.Parsing.MultiplexSources;
using Melville.Parsing.ObjectRentals;
using Melville.StreamInterfaces;
using Melville.StreamInterfaces.Memory;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Xunit;

namespace Melville.Pdf.DataModelTests.Fonts.Type1Text;

public class ReadType1TextFont
{
    private static IMultiplexSource GetFontText(
        string name, [CallerFilePath] string callerPath = "")
    {
        string fname = ConstructFileName(name, callerPath);
        return MultiplexSourceFactory.Create(File.OpenRead(fname));
    }

    private static string ConstructFileName(string name, [CallerFilePath] string callerPath = "") =>
        Path.Combine(Path.GetDirectoryName(callerPath)!, name);

    private static async Task<IGenericFont> ReadFontAsync(string name)
    {
        using var multiplexSource = GetFontText(name);
        var font = (await new Type1Parser(multiplexSource)
            .ParseAsync())[0];
        return font;
    }

    [Fact]
    public async Task DecodeEExecAsync() 
    {
        var data = File.ReadAllBytes(ConstructFileName("Type1Text.fon"));

        var reader = new MemoryReader(data.AsMemory(5644));
        var decryptor = new DecodingReader(reader, 55665);

        var buf = new byte[30];
        await decryptor.ReadExactAsync(buf.AsMemory(0,4), TestContext.Current.CancellationToken);
        await decryptor.ReadExactAsync(buf, TestContext.Current.CancellationToken);



        Encoding.ASCII.GetString(buf).Should().Be("dup /Private 23 dict dup begin");
        
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