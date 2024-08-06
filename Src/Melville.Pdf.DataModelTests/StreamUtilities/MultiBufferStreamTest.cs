using System;
using System.IO;
using System.Threading.Tasks;
using Melville.Parsing.MultiplexSources;
using Melville.Parsing.Streams;
using Melville.Pdf.LowLevel.Model.Conventions;
using Xunit;

namespace Melville.Pdf.DataModelTests.StreamUtilities;

public class MultiBufferStreamTest
{
    protected virtual IWritableMultiplexSource CreateEmptyStream(int blockLength) => 
        WritableBuffer.Create(blockLength);

    protected virtual IWritableMultiplexSource CreateStream(string content)
    {
        var ret = WritableBuffer.Create();
        using var writer = ret.WritingStream();
        writer.Write(content.AsExtendedAsciiBytes());
        return ret;
    }

    [Fact]
    public void AllowedOperations()
    {
        using var source = CreateEmptyStream(1);
        using var sut = source.WritingStream();
        Assert.False(sut.CanRead);
        Assert.True(sut.CanWrite);
        Assert.True(sut.CanSeek);
        Assert.False(sut.CanTimeout);

        using var reader = source.ReadFrom(0);
        Assert.True(reader.CanRead);
        Assert.True(reader.CanSeek);
        Assert.False(reader.CanWrite);
        Assert.False(reader.CanTimeout);
        Assert.Throws<NotSupportedException>(() => reader.Write(new byte[1], 0, 1));
    }

    [Fact]
    public void ReadSingleBuffer()
    {
        var sut = CreateStream("ABCDE");
        var ret = new byte[5];
        using var reader = sut.ReadFrom(0);
        Assert.Equal(5, reader.Read(ret, 0, 5));
        Assert.Equal("ABCDE", ExtendedAsciiEncoding.ExtendedAsciiString(ret));
    }


    [Theory]
    [InlineData((int)'A', 0)]
    [InlineData((int)'C', 2)]
    [InlineData((int)'E', 4)]
    [InlineData(-1, 5)]
    public void ReadByte(int value, long position)
    {
        using var sut = CreateStream("ABCDE");
        using var reader = sut.ReadFrom(0);
        reader.Seek(position, SeekOrigin.Begin);
        Assert.Equal(value, reader.ReadByte());
    }
    [Theory]
    [InlineData(0)]
    [InlineData(2)]
    [InlineData(4)]
    [InlineData(5)]
    public void WriteByte(long position)
    {
        var sut = CreateEmptyStream(10);
        var writer = sut.WritingStream();
        writer.Seek(position, SeekOrigin.Begin);
        writer.WriteByte((byte)'Z');
        using var reader = sut.ReadFrom(position);
        Assert.Equal('Z', reader.ReadByte());
    }
    [Fact]
    public void SimpleStreamLength()
    {
        var sut = CreateStream("ABCDE");
        Assert.Equal(5, sut.Length);
    }
    [Fact]
    public void ReadInTwoParts()
    {
        var sut = CreateStream("ABCDE");
        Span<byte> ret = stackalloc byte[3];
        using var reader = sut.ReadFrom(0);
        Assert.Equal(3, reader.Read(ret));
        Assert.Equal("ABC", ExtendedAsciiEncoding.ExtendedAsciiString(ret));
        Span<byte> ret2 = stackalloc byte[2];
        Assert.Equal(2, reader.Read(ret2));
        Assert.Equal("DE", ExtendedAsciiEncoding.ExtendedAsciiString(ret2));
    }
    [Theory]
    [InlineData(3, SeekOrigin.Begin)]
    [InlineData(1, SeekOrigin.Current)]
    [InlineData(-2, SeekOrigin.End)]
    public void SeekTest(int location, SeekOrigin seekOrigin)
    {
        var sut = CreateStream("ABCDE");
        using var reader = sut.ReadFrom(0);
        reader.Seek(2, SeekOrigin.Begin);
        reader.Seek(location, seekOrigin);
        Span<byte> ret2 = stackalloc byte[2];
        Assert.Equal(2, reader.Read(ret2));
        Assert.Equal("DE", ExtendedAsciiEncoding.ExtendedAsciiString(ret2));
    }

    [Theory]
    [InlineData(-5, false)]
    [InlineData(-1, false)]
    [InlineData(0, true)]
    [InlineData(1, true)]
    [InlineData(3, true)]
    [InlineData(5, true)]
    [InlineData(6, false)]
    [InlineData(7, false)]
    [InlineData(17, false)]
    public void ValidAndInvalidSeekTest(int location, bool valid)
    {
        using var sut = MultiplexSourceFactory.Create("ABCDE");
        var reader = sut.ReadFrom(0);
        if (valid)
        {
            reader.Position = location;
        }
        else
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => reader.Position = location);
        }
    }

        

    [Fact]
    public async Task ReadAsyncFromByteArrayAsync()
    {
        using var sut = CreateStream("ABCDE");
        var ret = new byte[5];
        await using var reader = sut.ReadFrom(0);
        Assert.Equal(5, await reader.ReadAsync(ret, 0, 5));
        Assert.Equal("ABCDE", ExtendedAsciiEncoding.ExtendedAsciiString(ret));
    }
    [Fact]
    public async Task ReadAsyncFromMemoryAsync()
    {
        using var sut = CreateStream("ABCDE");
        await using var reader = sut.ReadFrom(0);

        var ret = new byte[5];
        Assert.Equal(5, await reader.ReadAsync(ret.AsMemory()));
        Assert.Equal("ABCDE", ExtendedAsciiEncoding.ExtendedAsciiString(ret));
    }
    [Fact]
    public void ReadUpdatesPosition()
    {
        using var sut = CreateStream("ABCDE");
        using var reader = sut.ReadFrom(0);
        var ret = new byte[5];
        Assert.Equal(0, reader.Position);
        Assert.Equal(5, reader.Read(ret, 0, 5));
        Assert.Equal(5, reader.Position);
    }
    [Fact]
    public void WriteAndReadSingleBuffer()
    {
        var sut = CreateEmptyStream(10);
        using var writer = sut.WritingStream();
        Assert.Equal(0, sut.Length);
        writer.Write("ABCDE".AsExtendedAsciiBytes(), 0, 5);
        Assert.Equal(5, sut.Length);
        var ret = new byte[10];
        using var readFrom = sut.ReadFrom(0);
        Assert.Equal(5, readFrom.Read(ret, 0, 10));
        Assert.Equal("ABCDE", ExtendedAsciiEncoding.ExtendedAsciiString(ret.AsSpan(0,5)));
    }
    [Fact]
    public async Task WriteAndReadSingleBufferAsync()
    {
        var sut = CreateEmptyStream(10);
        await using var writer = sut.WritingStream();
        Assert.Equal(0, sut.Length);
        await writer.WriteAsync("ABCDE".AsExtendedAsciiBytes(), 0, 5);
        Assert.Equal(5, sut.Length);
        var ret = new byte[10];
        await using var readFrom = sut.ReadFrom(0);
        Assert.Equal(5, await readFrom.ReadAsync(ret, 0, 10));
        Assert.Equal("ABCDE", ExtendedAsciiEncoding.ExtendedAsciiString(ret.AsSpan(0,5)));
    }
        
    [Fact]
    public void WriteIntoMultipleBuffers()
    {
        var sut = CreateEmptyStream(3);
        using var writer = sut.WritingStream();
        for (int i = 0; i < 3; i++)
        {
            writer.Write("ABCDEFG".AsExtendedAsciiBytes().AsSpan());
        }
        Span<byte> ret = stackalloc byte[21];
        using var readFrom = sut.ReadFrom(0);
        Assert.Equal(21, readFrom.Read(ret));
        Assert.Equal("ABCDEFGABCDEFGABCDEFG", ExtendedAsciiEncoding.ExtendedAsciiString(ret));
    }
        
}