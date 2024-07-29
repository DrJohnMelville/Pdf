using System.Threading.Tasks;
using FluentAssertions;
using Melville.Fonts.SfntParsers.TableDeclarations.CffGlyphs;
using Melville.Parsing.MultiplexSources;
using Melville.Pdf.ReferenceDocuments.Utility;
using Xunit;

namespace Melville.Pdf.DataModelTests.Fonts.BareCff;

public class EncodingReaderTest
{
    private ValueTask<byte[]> Parse(string hexBits) =>
        new CffEncodingReader(
                MultiplexSourceFactory.Create(hexBits.BitsFromHex()).ReadPipeFrom(0))
            .ParseAsync();

    [Fact]
    public async Task ReadType0Encoding()
    {
        var ret = new byte[256];
        ret[10] = 1;
        ret[255] = 2;
        ret[32] = 3;
        (await Parse("00 03 0A FF 20"))
            .Should().BeEquivalentTo(ret);
    }
    [Fact]
    public async Task ReadType0ExtendedEncoding()
    {
        var ret = new byte[256];
        ret[10] = 1;
        ret[255] = 2;
        ret[32] = 3;
        (await Parse("80 03 0A FF 20 02 "))
            .Should().BeEquivalentTo(ret);
    }
    [Fact]
    public async Task ReadType1Encoding()
    {
        var ret = new byte[256];
        ret[10] = 1;
        ret[11] = 2;
        ret[32] = 3;
        (await Parse("01 02 0A 02 20 01"))
            .Should().BeEquivalentTo(ret);
    }

}