using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Melville.Fonts.Type1TextParsers;
using Melville.Hacks;
using Melville.Pdf.ReferenceDocuments.Utility;
using Xunit;

namespace Melville.Pdf.DataModelTests.Fonts.Type1Text;

public class DecryptText
{
    [Fact]
    public async Task DecryptCharstringAsync()
    {
        var source = new MemoryStream("""
            10BF31704FAB5B1F03F9B68B1F39A66521B1841F14
            81697F8E12B7F7DDD6E3D7248D965B1CD45E2114
            """.BitsFromHex());

        var result = new byte[source.Length - 4];

        var stream = new ExecDecodeStream(source, 4330);

        await result.FillBufferAsync(0, 10, stream);
        await result.FillBufferAsync(10, result.Length - 10, stream);

        result.Should().BeEquivalentTo("""
            BDF9B40D8BEF038BEF01F8ECEF018B16F9
            5006EF07FCEC06F88807F8EC06EF07FD5006090E 
            """.BitsFromHex());
    }

    [Fact]
    public void DecodeCharstringSpan()
    {
        var source = """
            10BF31704FAB5B1F03F9B68B1F39A66521B1841F14
            81697F8E12B7F7DDD6E3D7248D965B1CD45E2114
            """.BitsFromHex();

        ushort key = 4330;
        DecodeType1Encoding.DecodeSegment(source, ref key);

        source.Should().BeEquivalentTo("""
            00000000BDF9B40D8BEF038BEF01F8ECEF018B16F9
            5006EF07FCEC06F88807F8EC06EF07FD5006090E 
            """.BitsFromHex());
    }

}