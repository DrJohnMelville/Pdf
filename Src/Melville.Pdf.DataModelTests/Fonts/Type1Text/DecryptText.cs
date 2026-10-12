using System;
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
using Melville.Parsing.ObjectRentals;
using Melville.Pdf.ReferenceDocuments.Utility;
using Melville.StreamInterfaces;
using Melville.StreamInterfaces.Memory;
using Melville.StreamInterfaces.Streams;
using Xunit;

namespace Melville.Pdf.DataModelTests.Fonts.Type1Text;

public class HexDecodeStreamTests
{
    [Theory]
    [InlineData("1234567890ABCDEF")]
    [InlineData("1234567890\r\nABCDEF")]
    [InlineData("1gk234567890\r\nABCDEF")]
    public async Task DecodeHexReaderAsync(string input)
    {
        var source = new HexDecodingReader(new MemoryReader(Encoding.ASCII.GetBytes(input)));
        var target = new MemoryStream();
        await ((IAsyncReader)source).CopyToAsync(target.AsStreamWriter(), TestContext.Current.CancellationToken);
        target.ToArray().Should().BeEquivalentTo("1234567890ABCDEF".BitsFromHex());
    }
}

public class DecryptText
{

    [Fact]
    public async Task DecryptEexec()
    {
        var reader = new DecodingReader(new MemoryReader("""
            10BF31704FAB5B1F03F9B68B1F39A66521B1841F14
            81697F8E12B7F7DDD6E3D7248D965B1CD45E2114
            """.BitsFromHex()), 4330);
        var result = new byte[37];
        await reader.ReadExactAsync(result.AsMemory(0, 4), TestContext.Current.CancellationToken);
        await reader.ReadExactAsync(result, TestContext.Current.CancellationToken);
        result.Should().BeEquivalentTo("""
            BDF9B40D8BEF038BEF01F8ECEF018B16F9
            5006EF07FCEC06F88807F8EC06EF07FD5006090E 
            """.BitsFromHex());

    }
}