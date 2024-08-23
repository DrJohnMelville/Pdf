using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Melville.Parsing.MultiplexSources;
using Melville.Parsing.ObjectRentals;
using Melville.Parsing.Streams;
using Xunit;

namespace Melville.Pdf.DataModelTests.ParsingTestUtils;

public class MultiplexedStreamTest: IDisposable
{
    public void Dispose() => sut.Dispose();

    private readonly IMultiplexSource sut;

    public MultiplexedStreamTest()
    {
        var data = Enumerable.Range(0, 256).Select(i => (byte)i).ToArray();
        sut = CreateSut(data);
    }

    protected IMultiplexSource CreateSut(byte[] data) =>
        MultiplexSourceFactory.Create(data);

    private void VerifyRead(Stream reader, params byte[] data)
    {
        var actual = new byte[data.Length];
        Assert.Equal(data.Length, reader.Read(actual, 0, data.Length));
        Assert.Equal(data, actual);
    }
    private async Task VerifyReadAsync(Stream reader, params byte[] data)
    {
        var actual = new byte[data.Length];
        Assert.Equal(data.Length, await reader.ReadAsync(actual, 0, actual.Length));
        Assert.Equal(data, actual);
    }
    [Fact]
    public void SimpleRead()
    {
        using var reader = sut.ReadFrom(0);
        VerifyRead(reader, 0, 1, 2, 3);
        VerifyRead(reader, 4,5,6,7,8,9);
    }
    [Fact]
    public void InterleavedRead()
    {
        using var reader = sut.ReadFrom(0);
        using var reader2 = sut.ReadFrom(0);
        VerifyRead(reader, 0, 1, 2, 3);
        VerifyRead(reader2, 0, 1, 2, 3);
        VerifyRead(reader, 4,5,6,7,8,9);
        VerifyRead(reader2, 4,5,6,7,8,9);
    }
    [Fact]
    public async Task AsyncInterleavedReadAsync()
    {
        await using var reader = sut.ReadFrom(0);
        await using var reader2 = sut.ReadFrom(0);
        VerifyRead(reader, 0, 1, 2, 3);
        await VerifyReadAsync(reader2, 0, 1, 2, 3);
        await VerifyReadAsync(reader, 4,5,6,7,8,9);
        VerifyRead(reader2, 4,5,6,7,8,9);
    }
    [Fact]
    public async Task SimpleReadAsync()
    {
        await using var reader = sut.ReadFrom(0);
        await VerifyReadAsync(reader, 0, 1, 2, 3);
        await VerifyReadAsync(reader, 4,5,6,7,8,9);
    }
    [Fact]
    public void SeekRead()
    {
        using var reader = sut.ReadFrom(0);
        VerifyRead(reader, 0, 1, 2, 3);
        reader.Seek(20, SeekOrigin.Begin);
        VerifyRead(reader, 20, 21, 22);
    }
    [Fact]
    public void SeekFromEnd()
    {
        using var reader = sut.ReadFrom(0);
        VerifyRead(reader, 0, 1, 2, 3);
        reader.Seek(-56, SeekOrigin.End);
        VerifyRead(reader, 200,201,202);
    }
}