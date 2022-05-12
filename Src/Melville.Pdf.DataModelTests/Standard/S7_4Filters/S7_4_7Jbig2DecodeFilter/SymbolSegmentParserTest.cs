using System.Buffers;
using Melville.Hacks.Reflection;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Filters.CryptFilters.BitmapSymbols;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.HuffmanTables;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.SegmentParsers;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.Segments;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_4Filters.S7_4_7Jbig2DecodeFilter;

public class SymbolSegmentParserTest
{
    [Fact]
    public void Example1FromStandard()
    {
        var data = "00 01 00 00 00 01 00 00 00 01 E9 CB F4 00 26 AF 04 BF F0 78 2F E0 00 40".BitsFromHex();
        var sut = new SymbolDictionaryParser(new SequenceReader<byte>(new ReadOnlySequence<byte>(data))).Parse(210);
        Assert.Equal(210u, sut.Number);
        Assert.Single(sut.AllSymbols);
        Assert.Equal(8, sut.AllSymbols[0].Height);
        Assert.Equal("BBBB.\r\nB...B\r\nB...B\r\nB...B\r\nBBBB.\r\nB....\r\nB....\r\nB....", 
            sut.AllSymbols[0].BitmapString());
        Assert.Equal(1, sut.ExportedSymbols.Length);
        Assert.Equal(sut.AllSymbols[0],  sut.ExportedSymbols.Span[0]);
        
    }
    
    
}