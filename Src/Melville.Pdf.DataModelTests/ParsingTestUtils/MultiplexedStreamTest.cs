﻿using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Parsing.ParserContext.MultiplexedStreams;
using Xunit;

namespace Melville.Pdf.DataModelTests.ParsingTestUtils;

public class MultiplexedStreamTest
{
    private readonly MultiplexedStream sut;

    public MultiplexedStreamTest()
    {
        var data = Enumerable.Range(0, 256).Select(i => (byte)i).ToArray();
        sut = new MultiplexedStream(new MemoryStream(data));
    }

    private void VerifyRead(Stream reader, params byte[] data)
    {
        var acutal = new byte[data.Length];
        Assert.Equal(data.Length, reader.Read(acutal, 0, data.Length));
        Assert.Equal(data, acutal);
    }
    private async Task VerifyReadAsync(Stream reader, params byte[] data)
    {
        var acutal = new byte[data.Length];
        Assert.Equal(data.Length, await reader.ReadAsync(acutal, 0, acutal.Length));
        Assert.Equal(data, acutal);
    }
    [Fact]
    public void SimpleRead()
    {
        var reader = sut.ReadFrom(0);
        VerifyRead(reader, 0, 1, 2, 3);
        VerifyRead(reader, 4,5,6,7,8,9);
    }
    [Fact]
    public void InterleavedRead()
    {
        var reader = sut.ReadFrom(0);
        var reader2 = sut.ReadFrom(0);
        VerifyRead(reader, 0, 1, 2, 3);
        VerifyRead(reader2, 0, 1, 2, 3);
        VerifyRead(reader, 4,5,6,7,8,9);
        VerifyRead(reader2, 4,5,6,7,8,9);
    }
    [Fact]
    public async Task AsyncInterleavedRead()
    {
        var reader = sut.ReadFrom(0);
        var reader2 = sut.ReadFrom(0);
        VerifyRead(reader, 0, 1, 2, 3);
        await VerifyReadAsync(reader2, 0, 1, 2, 3);
        await VerifyReadAsync(reader, 4,5,6,7,8,9);
        VerifyRead(reader2, 4,5,6,7,8,9);
    }
    [Fact]
    public async Task SimpleReadAsync()
    {
        var reader = sut.ReadFrom(0);
        await VerifyReadAsync(reader, 0, 1, 2, 3);
        await VerifyReadAsync(reader, 4,5,6,7,8,9);
    }
    [Fact]
    public void SeekRead()
    {
        var reader = sut.ReadFrom(0);
        VerifyRead(reader, 0, 1, 2, 3);
        reader.Seek(20, SeekOrigin.Begin);
        VerifyRead(reader, 20, 21, 22);
    }
    [Fact]
    public void SeekFromEnd()
    {
        var reader = sut.ReadFrom(0);
        VerifyRead(reader, 0, 1, 2, 3);
        reader.Seek(-56, SeekOrigin.End);
        VerifyRead(reader, 200,201,202);
    }
}