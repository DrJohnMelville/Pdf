using System;
using System.Buffers;
using System.IO;
using System.Threading.Tasks;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Parsing.ObjectParsers;
using Melville.Pdf.LowLevel.Parsing.ParserContext;
using Moq;
using Xunit;

namespace Melville.Pdf.DataModelTests.PdfStreamHolderTest;

public class ParsingSourceTest
{
    private static MemoryStream IndexedStream()
    {
        var ret = new byte[256];
        for (int i = 0; i < ret.Length; i++)
        {
            ret[i] = (byte)i;
        }

        return new MemoryStream(ret);
    }

    private readonly ParsingFileOwner owner = 
        new(IndexedStream(), Mock.Of<IPasswordSource?>(), Mock.Of<IPdfObjectParser?>(), Mock.Of<IIndirectObjectResolver?>());

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
    public async Task CannotMakeTwoReaders()
    {
        await owner.RentReader(12);
        await Assert.ThrowsAsync<InvalidOperationException>(() => owner.RentReader(33).AsTask());
    }
    [Fact]
    public async Task CannotMakeReaderAndStream()
    {
        await owner.RentReader(12);
        await Assert.ThrowsAsync<InvalidOperationException>(() => owner.RentStream(33,22).AsTask());
    }
    [Fact]
    public async Task CannotMakeStreamAndReader()
    {
        await owner.RentStream(12,33);
        await Assert.ThrowsAsync<InvalidOperationException>(() => owner.RentReader(33).AsTask());
    }
    [Fact]
    public async Task CannotMakeTwoStreams()
    {
        await owner.RentStream(12,22);
        await Assert.ThrowsAsync<InvalidOperationException>(() => owner.RentStream(33,22).AsTask());
    }
    [Fact]
    public async Task ReadFiveBytes()
    {
        var sut = (await owner.RentReader(0)).Reader;
        var result = await sut.ReadAsync();
        var sp =ConfirmBytes(result.Buffer, 0, 1, 2, 3, 4);
        Assert.Equal(0, sut.GlobalPosition);
        sut.AdvanceTo( sp);
        Assert.Equal(5, sut.GlobalPosition);
    }
    [Fact]
    public async Task ReadThenJump()
    {
        using (var sut = await owner.RentReader(0))
        {
            var result = await sut.Reader.ReadAsync();
            var sp = ConfirmBytes(result.Buffer, 0, 1, 2, 3, 4);
            Assert.Equal(0, sut.Reader.GlobalPosition);
            sut.Reader.AdvanceTo(sp);
            Assert.Equal(5, sut.Reader.GlobalPosition);
        }

        using (var sut = await owner.RentReader(45))
        {
            var result = await sut.Reader.ReadAsync();
            var sp = ConfirmBytes(result.Buffer, 45, 46, 47, 48);
            sut.Reader.AdvanceTo( sp);
            Assert.Equal(49, sut.Reader.GlobalPosition);
        }
    }
}