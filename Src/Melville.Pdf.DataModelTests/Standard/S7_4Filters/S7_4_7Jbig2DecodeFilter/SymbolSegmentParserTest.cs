using System;
using System.Buffers;
using Melville.Hacks.Reflection;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Filters.CryptFilters.BitmapSymbols;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.HuffmanTables;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.SegmentParsers;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.SegmentParsers.SymbolDictonaries;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.Segments;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_4Filters.S7_4_7Jbig2DecodeFilter;

public class SymbolSegmentParserTest
{
    [Fact]
    public void Example1FromStandard()
    {
        var data = "00 01 00 00 00 01 00 00 00 01 E9 CB F4 00 26 AF 04 BF F0 78 2F E0 00 40".BitsFromHex();
        var sut = new SymbolDictionaryParser(new SequenceReader<byte>(new ReadOnlySequence<byte>(data)),
            ReadOnlySpan<Segment>.Empty).Parse(210);
        Assert.Single(sut.AllSymbols);
        Assert.Equal(8, sut.AllSymbols[0].Height);
        Assert.Equal("BBBB.\r\nB...B\r\nB...B\r\nB...B\r\nBBBB.\r\nB....\r\nB....\r\nB....", 
            sut.AllSymbols[0].BitmapString());
        Assert.Equal(1, sut.ExportedSymbols.Length);
        Assert.Equal(sut.AllSymbols[0],  sut.ExportedSymbols.Span[0]);
        
    }
    [Theory]
    [InlineData("0001 00000002 00000002 E5 CD F8 00 79 E0 84 10 81 F0 82 10 86 10 79 F0 00 80")]
  //  [InlineData("08 00 02 FF 00 00 00 02 00 00 00 02 4F E7 8C 20 0E 1D C7 CF 01 11 C4 B2 6F FF AC")]
    public void Example2FromStandard(string data)
    {
        var sut = new SymbolDictionaryParser(new SequenceReader<byte>(new ReadOnlySequence<byte>(data.BitsFromHex())),
            ReadOnlySpan<Segment>.Empty).Parse(11);
        Assert.Equal(2, sut.AllSymbols.Length);
        Assert.Equal(6, sut.AllSymbols[0].Height);
        Assert.Equal(".BBBB.\r\nB....B\r\nB.....\r\nB.....\r\nB....B\r\n.BBBB.", sut.AllSymbols[0].BitmapString());
        Assert.Equal(".BBBB.\r\n.....B\r\n.BBBBB\r\nB....B\r\nB....B\r\n.BBBBB", sut.AllSymbols[1].BitmapString());
        Assert.Equal(2, sut.ExportedSymbols.Length);
        Assert.Equal(sut.AllSymbols[0],  sut.ExportedSymbols.Span[0]);
        Assert.Equal(sut.AllSymbols[1],  sut.ExportedSymbols.Span[1]);
        
    }
    
    
}