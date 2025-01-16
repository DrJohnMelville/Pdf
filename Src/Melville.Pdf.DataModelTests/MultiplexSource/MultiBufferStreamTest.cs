using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using FluentAssertions;
using Melville.Hacks;
using Melville.Icc.Model.Tags;
using Melville.Parsing.MultiplexSources;
using Melville.Parsing.ObjectRentals;
using Melville.Parsing.Streams;
using Melville.Pdf.DataModelTests.SpanShould;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Xunit;

namespace Melville.Pdf.DataModelTests.MultiplexSource;

public class MultiBufferStreamTest
{
    [Fact]
    public void RoundTripStream()
    {
        var numberedBuffer = Enumerable.Range(0, 256).Select(i => (byte)i).ToArray();
        using var sut = WritableBuffer.Create(16);
        using var writer = sut.WritingStream();
        writer.Write(numberedBuffer, 0, 256);
        var buf2 = new byte[256];
        using var reader = sut.ReadFrom(0);
        buf2.FillBuffer(0, 256, reader.Read);
        buf2.Should().BeEquivalentTo(numberedBuffer);
    }

    [Fact]
    public void SeekOfEndExtendsStream()
    {
        using var sut = WritableBuffer.Create(16);
        using var writer = sut.WritingStream();
        writer.Write([1,2,3]);
        using var reader = sut.ReadFrom(0);
        reader.Seek(256, SeekOrigin.Begin);
        reader.Length.Should().Be(256);
        reader.Seek(0, SeekOrigin.Begin);
        var buffer = new byte[256];
        reader.ReadExactly(buffer);
        buffer.Should().StartWith(new byte[] { 1, 2, 3 });
        // the contents of jumped over sections are undefined.
    }

    [Fact]
    public void SerialRead()
    {
        var numberedBuffer = Enumerable.Range(0, 256).Select(i => (byte)i).ToArray();
        using var sut = WritableBuffer.Create(16);
        using var writer = sut.WritingStream();
        writer.Write(numberedBuffer, 0, 256);
        var buf2 = new byte[16];
        using var reader = sut.ReadFrom(0);
        buf2.FillBuffer(0, 16, reader.Read);
        buf2.AsSpan().Should().Be(numberedBuffer.AsSpan()[..16]);
        buf2.FillBuffer(0, 16, reader.Read);
        buf2.AsSpan().Should().Be(numberedBuffer.AsSpan()[16..32]);
    }
    [Fact]
    public async Task RoundTripStreamAsync()
    {
        var numberedBuffer = Enumerable.Range(0, 256).Select(i => (byte)i).ToArray();
        using var sut = WritableBuffer.Create(16);
        await using var writer = sut.WritingStream();
        await writer.WriteAsync(numberedBuffer, 0, 256);
        var buf2 = new byte[256];
        await using var reader = sut.ReadFrom(0);
        await buf2.FillBufferAsync(0, 256, reader);
        buf2.Should().BeEquivalentTo(numberedBuffer);
    }
    [Fact]
    public async Task RoundTripStreamWithSeekAsync()
    {
        var numberedBuffer = Enumerable.Range(0, 256).Select(i => (byte)i).ToArray();
        using var sut = WritableBuffer.Create(16);
        await using var writer = sut.WritingStream();
        await writer.WriteAsync(numberedBuffer, 0, 256);
        var buf2 = new byte[240];
        using var reader = ((IMultiplexSource)sut).ReadFrom(16);
        await buf2.FillBufferAsync(0, 240, reader);
        buf2.AsSpan().Should().Be(numberedBuffer.AsSpan()[16..]);
    }

    //next we need to test writes that happen in the middle of the stream
    // including writes that overrun the end of the stream
    // then we need to get rid of MultiBufferStream2
}