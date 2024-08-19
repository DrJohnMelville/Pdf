using System;
using System.Threading.Tasks;
using FluentAssertions;
using Melville.Fonts.SfntParsers.TableDeclarations.CffGlyphs;
using Melville.Parsing.MultiplexSources;
using Melville.Parsing.ObjectRentals;
using Melville.Pdf.ReferenceDocuments.Utility;
using Xunit;

namespace Melville.Pdf.DataModelTests.Fonts.BareCff;

public class EncodingReaderTest
{
    private ValueTask<byte[]> ParseAsync(string hexBits)
    {
        using var multiplexSource = MultiplexSourceFactory.Create(hexBits.BitsFromHex());
        return new CffEncodingReader(
                multiplexSource.ReadPipeFrom(0),
                new GlyphFromSid([
                    0, 256, 257, 258
                ]))
            .ParseAsync();
    }

    [Fact]
    public async Task ReadType0EncodingAsync()
    {
        var ret = new byte[256];
        ret[10] = 1;
        ret[255] = 2;
        ret[32] = 3;
        (await ParseAsync("00 03 0A FF 20"))
            .Should().BeEquivalentTo(ret);
    }
    [Fact]
    public async Task ReadType0EncodingWithExtraAsync()
    {
        var ret = new byte[256];
        ret[10] = 1;
        ret[255] = 2;
        ret[32] = 3;
        ret[4] = 1;
        (await ParseAsync("80 03 0A FF 20" +
                          "01 04 0100"))
            .Should().BeEquivalentTo(ret);
    }
    [Fact]
    public async Task ReadType0ExtendedZeroLengthArrayAsync()
    {
        var ret = new byte[256];
        ret[10] = 1;
        ret[255] = 2;
        ret[32] = 3;
        (await ParseAsync("80 03 0A FF 20 00"))
            .Should().BeEquivalentTo(ret);
    }
    [Fact]
    public async Task ReadType1EncodingAsync()
    {
        var ret = new byte[256];
        ret[10] = 1;
        ret[11] = 2;
        ret[32] = 3;
        (await ParseAsync("01 02 0A 02 20 01"))
            .Should().BeEquivalentTo(ret);
    }

    [Fact] public async Task ReadType1EncodingWithSupplementalAsync()
    {
        var ret = new byte[256];
        ret[10] = 1;
        ret[11] = 2;
        ret[32] = 3;
        ret[4] = 1;
        ret[5] = 2;
        ret[6] = 3;
        (await ParseAsync("81 02 0A 02 20 01" +
                          "03 04 0100 05 0101 06 0102"))
            .Should().BeEquivalentTo(ret);
    }

}