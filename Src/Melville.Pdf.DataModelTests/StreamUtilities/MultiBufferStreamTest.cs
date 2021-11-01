using System;
using System.IO;
using System.Threading.Tasks;
using Melville.FileSystem;
using Melville.Pdf.LowLevel.Filters.StreamFilters;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Primitives;
using Xunit;

namespace Melville.Pdf.DataModelTests.StreamUtilities;

public class MultiBufferStreamTest
{
    protected virtual MultiBufferStream CreateEmptyStream(int blockLength) => 
        new MultiBufferStream(blockLength);

    protected virtual Stream CreateStream(string content) => 
        new MultiBufferStream(content.AsExtendedAsciiBytes());
        
    [Fact]
    public void AllowedOperations()
    {
        var sut = CreateEmptyStream(1);
        Assert.True(sut.CanRead);
        Assert.True(sut.CanWrite);
        Assert.True(sut.CanSeek);
        Assert.False(sut.CanTimeout);

        var reader = sut.CreateReader();
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
        Assert.Equal(5, sut.Read(ret, 0, 5));
        Assert.Equal("ABCDE", ExtendedAsciiEncoding.ExtendedAsciiString(ret));
    }


    [Theory]
    [InlineData((int)'A', 0)]
    [InlineData((int)'C', 2)]
    [InlineData((int)'E', 4)]
    [InlineData(-1, 5)]
    public void ReadByte(int value, long position)
    {
        var sut = CreateStream("ABCDE");
        sut.Seek(position, SeekOrigin.Begin);
        Assert.Equal(value, sut.ReadByte());
    }
    [Theory]
    [InlineData(0)]
    [InlineData(2)]
    [InlineData(4)]
    [InlineData(5)]
    public void WriteByte(long position)
    {
        var sut = CreateStream("ABCDE");
        sut.Seek(position, SeekOrigin.Begin);
        sut.WriteByte((byte)'Z');
        sut.Seek(position, SeekOrigin.Begin);
        Assert.Equal('Z', sut.ReadByte());
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
        Assert.Equal(3, sut.Read(ret));
        Assert.Equal("ABC", ExtendedAsciiEncoding.ExtendedAsciiString(ret));
        Span<byte> ret2 = stackalloc byte[2];
        Assert.Equal(2, sut.Read(ret2));
        Assert.Equal("DE", ExtendedAsciiEncoding.ExtendedAsciiString(ret2));
    }
    [Theory]
    [InlineData(3, SeekOrigin.Begin)]
    [InlineData(1, SeekOrigin.Current)]
    [InlineData(-2, SeekOrigin.End)]
    public void SeekTest(int location, SeekOrigin seekOrigin)
    {
        var sut = CreateStream("ABCDE");
        sut.Seek(2, SeekOrigin.Begin);
        sut.Seek(location, seekOrigin);
        Span<byte> ret2 = stackalloc byte[2];
        Assert.Equal(2, sut.Read(ret2));
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
        var sut = CreateStream("ABCDE");
        if (valid)
        {
            sut.Position = location;
        }
        else
        {
            Assert.Throws<ArgumentException>(() => sut.Position = location);
        }
    }

        

    [Fact]
    public async Task ReadAsyncFromByteArray()
    {
        var sut = CreateStream("ABCDE");
        var ret = new byte[5];
        Assert.Equal(5, await sut.ReadAsync(ret, 0, 5));
        Assert.Equal("ABCDE", ExtendedAsciiEncoding.ExtendedAsciiString(ret));
    }
    [Fact]
    public async Task ReadAsyncFromMemory()
    {
        var sut = CreateStream("ABCDE");
        var ret = new byte[5];
        Assert.Equal(5, await sut.ReadAsync(ret.AsMemory()));
        Assert.Equal("ABCDE", ExtendedAsciiEncoding.ExtendedAsciiString(ret));
    }
    [Fact]
    public void ReadUpdatesPosition()
    {
        var sut = CreateStream("ABCDE");
        var ret = new byte[5];
        Assert.Equal(0, sut.Position);
        Assert.Equal(5, sut.Read(ret, 0, 5));
        Assert.Equal(5, sut.Position);
    }
    [Fact]
    public void WriteAndReadSingleBuffer()
    {
        var sut = CreateEmptyStream(10);
        Assert.Equal(0, sut.Length);
        sut.Write("ABCDE".AsExtendedAsciiBytes(), 0, 5);
        Assert.Equal(5, sut.Length);
        sut.Seek(0, SeekOrigin.Begin);
        var ret = new byte[10];
        Assert.Equal(5, sut.Read(ret, 0, 10));
        Assert.Equal("ABCDE", ExtendedAsciiEncoding.ExtendedAsciiString(ret.AsSpan(0,5)));
    }
    [Fact]
    public async Task WriteAndReadSingleBufferAsync()
    {
        var sut = CreateEmptyStream(10);
        Assert.Equal(0, sut.Length);
        await sut.WriteAsync("ABCDE".AsExtendedAsciiBytes(), 0, 5);
        Assert.Equal(5, sut.Length);
        sut.Seek(0, SeekOrigin.Begin);
        var ret = new byte[10];
        Assert.Equal(5, await sut.ReadAsync(ret, 0, 10));
        Assert.Equal("ABCDE", ExtendedAsciiEncoding.ExtendedAsciiString(ret.AsSpan(0,5)));
    }
        
    [Fact]
    public void WriteIntoMultipleBuffers()
    {
        var sut = CreateEmptyStream(3);
        for (int i = 0; i < 3; i++)
        {
            sut.Write("ABCDEFG".AsExtendedAsciiBytes().AsSpan());
        }
        sut.Seek(0, SeekOrigin.Begin);
        Span<byte> ret = stackalloc byte[21];
        Assert.Equal(21, sut.Read(ret));
        Assert.Equal("ABCDEFGABCDEFGABCDEFG", ExtendedAsciiEncoding.ExtendedAsciiString(ret));
    }
        
}