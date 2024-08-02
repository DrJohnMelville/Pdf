using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Melville.Hacks;
using Melville.Parsing.MultiplexSources;
using Melville.Parsing.Streams;
using Xunit;

namespace Melville.Pdf.DataModelTests.MultiplexSource;

public class MultiBufferStreamTest
{
    [Fact]
    public void RoundTripStream()
    {
        var numberedBuffer = Enumerable.Range(0, 256).Select(i => (byte)i).ToArray();
        var sut = new MultiBufferStream(16);
        sut.Write(numberedBuffer, 0, 256);
        var buf2 = new byte[256];
        var reader = sut.CreateReader();
        buf2.FillBuffer(0, 256, reader.Read);
        buf2.Should().BeEquivalentTo(numberedBuffer);
    }
    [Fact]
    public void SerialRead()
    {
        var numberedBuffer = Enumerable.Range(0, 256).Select(i => (byte)i).ToArray();
        var sut = new MultiBufferStream(16);
        sut.Write(numberedBuffer, 0, 256);
        var buf2 = new byte[16];
        var reader = sut.CreateReader();
        buf2.FillBuffer(0, 16, reader.Read);
        buf2.Should().BeEquivalentTo(numberedBuffer[..16]);
        buf2.FillBuffer(0, 16, reader.Read);
        buf2.Should().BeEquivalentTo(numberedBuffer[16..32]);
    }
    [Fact]
    public async Task RoundTripStreamAsync()
    {
        var numberedBuffer = Enumerable.Range(0, 256).Select(i => (byte)i).ToArray();
        var sut = new MultiBufferStream(16);
        await sut.WriteAsync(numberedBuffer, 0, 256);
        var buf2 = new byte[256];
        var reader = sut.CreateReader();
        await buf2.FillBufferAsync(0, 256, reader);
        buf2.Should().BeEquivalentTo(numberedBuffer);
    }
    [Fact]
    public async Task RoundTripStreamWithSeekAsync()
    {
        var numberedBuffer = Enumerable.Range(0, 256).Select(i => (byte)i).ToArray();
        var sut = new MultiBufferStream(16);
        await sut.WriteAsync(numberedBuffer, 0, 256);
        var buf2 = new byte[240];
        var reader = ((IMultiplexSource)sut).ReadFrom(16);
        await buf2.FillBufferAsync(0, 240, reader);
        buf2.Should().BeEquivalentTo(numberedBuffer[16..]);
    }
}