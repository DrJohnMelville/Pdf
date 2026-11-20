using System;
using System.IO;
using System.Threading.Tasks;
using Melville.Parsing.MultiplexSources;
using Melville.Parsing.ObjectRentals;
using Melville.Parsing.Streams;
using Melville.Pdf.LowLevel.Model.Conventions;

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

    [Test]
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

    [Test]
    public void ReadSingleBuffer()
    {
        using var sut = CreateStream("ABCDE");
        var ret = new byte[5];
        using var reader = sut.ReadFrom(0);
        Assert.Equal(5, reader.Read(ret, 0, 5));
        Assert.Equal("ABCDE", ExtendedAsciiEncoding.ExtendedAsciiString(ret));
    }


    [Test]
    [Arguments((int)'A', 0)]
    [Arguments((int)'C', 2)]
    [Arguments((int)'E', 4)]
    [Arguments(-1, 5)]
    public void ReadByte(int value, long position)
    {
        using var sut = CreateStream("ABCDE");
        using var reader = sut.ReadFrom(0);
        reader.Seek(position, SeekOrigin.Begin);
        Assert.Equal(value, reader.ReadByte());
    }
    [Test]
    [Arguments(0)]
    [Arguments(2)]
    [Arguments(4)]
    [Arguments(5)]
    public void WriteByte(long position)
    {
        using var sut = CreateEmptyStream(10);
        using var writer = sut.WritingStream();
        writer.Seek(position, SeekOrigin.Begin);
        writer.WriteByte((byte)'Z');
        using var reader = sut.ReadFrom(position);
        Assert.Equal('Z', reader.ReadByte());
    }
    [Test]
    public void SimpleStreamLength()
    {
        using var sut = CreateStream("ABCDE");
        Assert.Equal(5, sut.Length);
    }
    [Test]
    public void ReadInTwoParts()
    {
        using var sut = CreateStream("ABCDE");
        Span<byte> ret = stackalloc byte[3];
        using var reader = sut.ReadFrom(0);
        Assert.Equal(3, reader.Read(ret));
        Assert.Equal("ABC", ExtendedAsciiEncoding.ExtendedAsciiString(ret));
        Span<byte> ret2 = stackalloc byte[2];
        Assert.Equal(2, reader.Read(ret2));
        Assert.Equal("DE", ExtendedAsciiEncoding.ExtendedAsciiString(ret2));
    }
    [Test]
    [Arguments(3, SeekOrigin.Begin)]
    [Arguments(1, SeekOrigin.Current)]
    [Arguments(-2, SeekOrigin.End)]
    public void SeekTest(int location, SeekOrigin seekOrigin)
    {
        using var sut = CreateStream("ABCDE");
        using var reader = sut.ReadFrom(0);
        reader.Seek(2, SeekOrigin.Begin);
        reader.Seek(location, seekOrigin);
        Span<byte> ret2 = stackalloc byte[2];
        Assert.Equal(2, reader.Read(ret2));
        Assert.Equal("DE", ExtendedAsciiEncoding.ExtendedAsciiString(ret2));
    }

    [Test]
    [Arguments(-5, false)]
    [Arguments(-1, false)]
    [Arguments(0, true)]
    [Arguments(1, true)]
    [Arguments(3, true)]
    [Arguments(5, true)]
    [Arguments(6, false)]
    [Arguments(7, false)]
    [Arguments(17, false)]
    public void ValidAndInvalidSeekTest(int location, bool valid)
    {
        using var sut = MultiplexSourceFactory.Create("ABCDE");
        using var reader = sut.ReadFrom(0);
        if (valid)
        {
            reader.Position = location;
        }
        else
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => reader.Position = location);
        }
    }

        

    [Test]
    public async Task ReadAsyncFromByteArrayAsync()
    {
        using var sut = CreateStream("ABCDE");
        var ret = new byte[5];
        using var reader = sut.ReadFrom(0);
        Assert.Equal(5, await reader.ReadAsync(ret, 0, 5));
        Assert.Equal("ABCDE", ExtendedAsciiEncoding.ExtendedAsciiString(ret));
    }
    [Test]
    public async Task ReadAsyncFromMemoryAsync()
    {
        using var sut = CreateStream("ABCDE");
        using var reader = sut.ReadFrom(0);

        var ret = new byte[5];
        Assert.Equal(5, await reader.ReadAsync(ret.AsMemory()));
        Assert.Equal("ABCDE", ExtendedAsciiEncoding.ExtendedAsciiString(ret));
    }
    [Test]
    public void ReadUpdatesPosition()
    {
        using var sut = CreateStream("ABCDE");
        using var reader = sut.ReadFrom(0);
        var ret = new byte[5];
        Assert.Equal(0, reader.Position);
        Assert.Equal(5, reader.Read(ret, 0, 5));
        Assert.Equal(5, reader.Position);
    }
    [Test]
    public void WriteAndReadSingleBuffer()
    {
        using var sut = CreateEmptyStream(10);
        using var writer = sut.WritingStream();
        Assert.Equal(0, sut.Length);
        writer.Write("ABCDE".AsExtendedAsciiBytes(), 0, 5);
        Assert.Equal(5, sut.Length);
        var ret = new byte[10];
        using var readFrom = sut.ReadFrom(0);
        Assert.Equal(5, readFrom.Read(ret, 0, 10));
        Assert.Equal("ABCDE", ExtendedAsciiEncoding.ExtendedAsciiString(ret.AsSpan(0,5)));
    }
    [Test]
    public async Task WriteAndReadSingleBufferAsync()
    {
        using var sut = CreateEmptyStream(10);
        using var writer = sut.WritingStream();
        Assert.Equal(0, sut.Length);
        await writer.WriteAsync("ABCDE".AsExtendedAsciiBytes(), 0, 5);
        Assert.Equal(5, sut.Length);
        var ret = new byte[10];
        using var readFrom = sut.ReadFrom(0);
        Assert.Equal(5, await readFrom.ReadAsync(ret, 0, 10));
        Assert.Equal("ABCDE", ExtendedAsciiEncoding.ExtendedAsciiString(ret.AsSpan(0,5)));
    }
        
    [Test]
    public void WriteIntoMultipleBuffers()
    {
        using var sut = CreateEmptyStream(3);
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