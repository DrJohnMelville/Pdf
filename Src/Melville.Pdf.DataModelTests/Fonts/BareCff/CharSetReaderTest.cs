using System;
using System.Threading.Tasks;
using FluentAssertions;
using Melville.Fonts.SfntParsers.TableDeclarations.CffGlyphs;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.MultiplexSources;
using Melville.Pdf.ReferenceDocuments.Utility;
using Xunit;

namespace Melville.Pdf.DataModelTests.Fonts.BareCff;

public class CharSetReaderTest
{
    private const string ThreeNameIndex = """
        0003 01 01 02 03 06 61 62 63 64 65
        """;

    private async ValueTask<CffIndex> NamesIndexAsync()
    {
        var source = MultiplexSourceFactory.Create(ThreeNameIndex.BitsFromHex());
        using var pipe = source.ReadPipeFrom(0);
        var ret = await new CFFIndexParser(source, pipe).ParseCff1Async();
        return ret;
    }

    private async ValueTask<string[]> ReadTableAsync(string hexBytes, int length)
    {
        var source = MultiplexSourceFactory.Create(hexBytes.BitsFromHex());
        using var pipe = source.ReadPipeFrom(0);

        var target = new StringTarget(new CffStringIndex(await NamesIndexAsync()), 
            length);
        return (await new CharSetReader<StringTarget>(pipe, target)
            .ReadCharSetAsync().CA()).Result;

    }

    [Fact]
    public async Task ReadType0Async()
    {
        (await ReadTableAsync("00 0001 0002 0100", 4))
            .Should().BeEquivalentTo([".notdef", "space","exclam", "dsuperior"]);
    }
    [Fact]
    public async Task ReadType0toInts()
    {
        var source = MultiplexSourceFactory.Create("00 0001 0002 0100".BitsFromHex());
        using var pipe = source.ReadPipeFrom(0);

        var target = new MemoryTarget(new ushort[4]);
        (await new CharSetReader<MemoryTarget>(pipe, target)
            .ReadCharSetAsync().CA()).Target.ToArray().Should().BeEquivalentTo(
            [0, 01, 02, 256]);
    }
    [Fact]
    public async Task ReadType1Async()
    {
        (await ReadTableAsync("01 0001 01 0100 00", 4))
            .Should().BeEquivalentTo([".notdef", "space","exclam", "dsuperior"]);
    }
    [Fact]
    public async Task ReadType2Async()
    {
        (await ReadTableAsync("02 0001 0001 0100 0000", 4))
            .Should().BeEquivalentTo([".notdef", "space","exclam", "dsuperior"]);
    }
}