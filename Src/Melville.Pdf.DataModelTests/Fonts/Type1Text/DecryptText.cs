using System.Buffers;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Melville.Fonts.Type1TextParsers.EexecDecoding;
using Melville.Hacks;
using Melville.Parsing.CountingReaders;
using Melville.Parsing.MultiplexSources;
using Melville.Pdf.ReferenceDocuments.Utility;
using Xunit;

namespace Melville.Pdf.DataModelTests.Fonts.Type1Text;

public class HexDecodeStreamTests
{
    [Theory]
    [InlineData("1234567890ABCDEF")]
    [InlineData("1234567890\r\nABCDEF")]
    [InlineData("1gk234567890\r\nABCDEF")]
    public async Task DecodeHexStreamAsync(string input)
    {
        var source = new MemoryStream(Encoding.ASCII.GetBytes(input));
        var target = new MemoryStream();
        var stream = new DecodeHexStream(source);
        await stream.CopyToAsync(target);
        target.ToArray().Should().BeEquivalentTo("1234567890ABCDEF".BitsFromHex());
    }
}

public class DecryptText
{
    [Fact]
    public async Task DecryptCharstringAsync()
    {
        var source = MultiplexSourceFactory.Create("""
            20202020
            10BF31704FAB5B1F03F9B68B1F39A66521B1841F14
            81697F8E12B7F7DDD6E3D7248D965B1CD45E2114
            """.BitsFromHex());

        var result = new byte[source.Length - 8];

        IByteSource stream;
        var pipe = source.ReadPipeFrom(0);
        stream = new EexeDecisionSource(
            pipe, source,
            i=>stream = i, 4330);

        var rr = await stream.ReadAtLeastAsync(result.Length);
        new SequenceReader<byte>(rr.Buffer).TryCopyTo(result);


        result.Should().BeEquivalentTo("""
            BDF9B40D8BEF038BEF01F8ECEF018B16F9
            5006EF07FCEC06F88807F8EC06EF07FD5006090E 
            """.BitsFromHex());
    }

    [Fact]
    public async Task DecryptTextCharstringAsync()
    {
        var source = MultiplexSourceFactory.Create("""
            10BF31704FAB5B1F03F9B68B1F39A66521B1841F14
            81697F8E12B7F7DDD6E3D7248D965B1CD45E2114
            """u8.ToArray());
    
        var result = new byte[37];
    
        IByteSource stream;
        var pipe = source.ReadPipeFrom(0);
        stream = new EexeDecisionSource(pipe, source, i=>stream = i, 4330);
    
        var rr = await stream.ReadAtLeastAsync(result.Length);
        new SequenceReader<byte>(rr.Buffer).TryCopyTo(result);
    
    
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