using System;
using System.IO;
using System.IO.Pipelines;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Melville.Parsing.MultiplexSources;
using Melville.SharpFont;
using Xunit;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Melville.Pdf.DataModelTests.MultiplexSource;

public class MultiplexedStreamBufferTest
{
    private static readonly byte[] TestData = [0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16];
    private readonly MemoryStream sourceStream = new(TestData);
    private readonly MultiplexedStreamBuffer sut;
    private readonly Stream stream;

    public MultiplexedStreamBufferTest()
    {
        sut = new MultiplexedStreamBuffer(sourceStream, 4);
        stream = sut.ReadFrom(0);
    }

    [Fact]
    public void SimpleRead()
    {

        Span<byte> data = stackalloc byte[3];
        stream.Read(data).Should().Be(3);
        data.SequenceEqual<byte>([0, 1, 2]).Should().BeTrue();
        sourceStream.Position.Should().Be(4);
       
        stream.Read(data).Should().Be(3);
        data.SequenceEqual<byte>([3,4,5]).Should().BeTrue();
        sourceStream.Position.Should().Be(8);
    }
    [Fact]
    public void DoNotExtendUnlessNeeded()
    {
        Span<byte> data = stackalloc byte[2];
        stream.Read(data).Should().Be(2);
        data.SequenceEqual<byte>([0, 1]).Should().BeTrue();
        sourceStream.Position.Should().Be(4);
       
        stream.Read(data).Should().Be(2);
        data.SequenceEqual<byte>([2,3]).Should().BeTrue();
        sourceStream.Position.Should().Be(4);
    }

    [Fact]
    public async Task SimpleReadAsync()
    {
        byte[] data = new byte[3];
        (await stream.ReadAsync(data.AsMemory())).Should().Be(3);
        data.SequenceEqual<byte>([0, 1, 2]).Should().BeTrue();
        sourceStream.Position.Should().Be(4);

        (await stream.ReadAsync(data.AsMemory())).Should().Be(3);
        data.SequenceEqual<byte>([3,4,5]).Should().BeTrue();
        sourceStream.Position.Should().Be(8);
    }

    [Fact]
    public void SeekAndRead()
    {
        stream.Seek(7, SeekOrigin.Begin);
        Span<byte> data = stackalloc byte[3];
        stream.Read(data).Should().Be(3);
        data.SequenceEqual<byte>([7,8,9]).Should().BeTrue();
    }
}