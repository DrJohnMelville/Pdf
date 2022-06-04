using System;
using System.Buffers;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Filters.CryptFilters.BitmapSymbols;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.SegmentParsers.SymbolDictonaries;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.Segments;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_4Filters.S7_4_7Jbig2DecodeFilter;

public class SymbolSegmentParserTest
{
    private static SymbolDictionarySegment ParseSymbolDictionary(string data, params Segment[] referredTo)
    {
        return new SymbolDictionaryParser(new SequenceReader<byte>(new ReadOnlySequence<byte>(data.BitsFromHex())),
            referredTo.AsSpan()).Parse();
    }

    [Fact]
    public void Example1FromStandard()
    {
        var data = "00 01 00 00 00 01 00 00 00 01 E9 CB F4 00 26 AF 04 BF F0 78 2F E0 00 40";
        var sut = ParseSymbolDictionary(data);
        Assert.Equal(1, sut.ExportedSymbols.Length);
        Assert.Equal(8, sut.ExportedSymbols.Span[0].Height);
        Assert.Equal("BBBB.\r\nB...B\r\nB...B\r\nB...B\r\nBBBB.\r\nB....\r\nB....\r\nB....", 
            sut.ExportedSymbols.Span[0].BitmapString());
        Assert.Equal(1, sut.ExportedSymbols.Length);
        Assert.Equal(sut.ExportedSymbols.Span[0],  sut.ExportedSymbols.Span[0]);
        
    }

    [Theory]
    [InlineData("0001 00000002 00000002 E5 CD F8 00 79 E0 84 10 81 F0 82 10 86 10 79 F0 00 80")]
    [InlineData("08 00 02 FF 00 00 00 02 00 00 00 02 4F E7 8C 20 0E 1D C7 CF 01 11 C4 B2 6F FF AC")]
    public void Example2FromStandard(string data)
    {
        var sut = ParseSymbolDictionary(data);
        Assert.Equal(2, sut.ExportedSymbols.Length);
        Assert.Equal(6, sut.ExportedSymbols.Span[0].Height);
        VerifyCharacterC(sut.ExportedSymbols.Span[0]);
        VerifyCharacterA(sut.ExportedSymbols.Span[1]);
        Assert.Equal(2, sut.ExportedSymbols.Length);
        Assert.Equal(sut.ExportedSymbols.Span[0],  sut.ExportedSymbols.Span[0]);
        Assert.Equal(sut.ExportedSymbols.Span[1],  sut.ExportedSymbols.Span[1]);
    }

    private static void VerifyCharacterC(IBinaryBitmap symbol) => 
        Assert.Equal(".BBBB.\r\nB....B\r\nB.....\r\nB.....\r\nB....B\r\n.BBBB.", symbol.BitmapString());

    private static void VerifyCharacterA(IBinaryBitmap symbol) => 
        Assert.Equal(".BBBB.\r\n.....B\r\n.BBBBB\r\nB....B\r\nB....B\r\n.BBBBB", symbol.BitmapString());

    [Fact]
    public void RefinementDictionaryTest()
    {
        var d1 = ParseSymbolDictionary("08 00 02 FF 00 00 00 01 00 00 00 01 4F E7 8D 68 1B 14 2F 3F FF AC");
        Assert.Equal(1, d1.ExportedSymbols.Length);
        VerifyCharacterA(d1.ExportedSymbols.Span[0]);

        var d2 = ParseSymbolDictionary(
            "08 02 02 FF FF FF FF FF 00 00 00 03 00 00 00 02 4F E9 D7 D5 90 C3 B5 26 A7 FB 6D 14 98 3F FF AC",
            d1);
        Assert.Equal(3, d2.ExportedSymbols.Length);
    }
}