using System;
using System.Buffers;
using System.IO;
using System.Threading.Tasks;
using Melville.Parsing.MultiplexSources;
using Melville.Parsing.ObjectRentals;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Parsing.ParserContext;
using Moq;
using Xunit;

namespace Melville.Pdf.DataModelTests.PdfStreamHolderTest;

public class ParsingSourceTest: IDisposable
{
    public void Dispose() => owner.Dispose();

    private static IMultiplexSource IndexedStream()
    {
        var ret = new byte[256];
        for (int i = 0; i < ret.Length; i++)
        {
            ret[i] = (byte)i;
        }

        return MultiplexSourceFactory.Create(ret);
    }

    private readonly ParsingFileOwner owner = 
        new(IndexedStream(), Mock.Of<IPasswordSource>());

    private SequencePosition ConfirmBytes(ReadOnlySequence<byte> seq, params byte[] values)
    {
        var reader = new SequenceReader<byte>(seq);
        foreach (var value in values)
        {
            AAssert.True(reader.TryRead(out var b));
            Assert.Equal(value, b);
        }

        return reader.Position;
    }
    
    [Fact]
    public async Task ReadFiveBytesAsync()
    {
        using var parsingReader = owner.SubsetReader(0);
        var sut = parsingReader.Reader;
        var result = await sut.ReadAsync();
        var sp =ConfirmBytes(result.Buffer, 0, 1, 2, 3, 4);
        Assert.Equal(0, sut.Position);
        sut.AdvanceTo( sp);
        Assert.Equal(5, sut.Position);
    }
    [Fact]
    public async Task ReadThenJumpAsync()
    {
        {
            using var sut = owner.SubsetReader(0);
            var result = await sut.Reader.ReadAsync();
            var sp = ConfirmBytes(result.Buffer, 0, 1, 2, 3, 4);
            Assert.Equal(0, sut.Reader.Position);
            sut.Reader.AdvanceTo(sp);
            Assert.Equal(5, sut.Reader.Position);
        }

        {
            using var sut = owner.SubsetReader(45);
            var result = await sut.Reader.ReadAsync();
            var sp = ConfirmBytes(result.Buffer, 45, 46, 47, 48);
            sut.Reader.AdvanceTo( sp);
            Assert.Equal(49, sut.Reader.Position);
        }
    }
}