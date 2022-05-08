using System.Buffers;
using Melville.Hacks.Reflection;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
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
        var flags = (SymbolDictionaryFlags)(sut.GetField("flags")!);
        Assert.True(flags.UseHuffmanEncoding);
        Assert.False(flags.AggregateRefinement);
        Assert.Equal(HuffmanTableSelection.B4, flags.HuffmanSelectionForHeight);
        Assert.Equal(HuffmanTableSelection.B2, flags.HuffmanSelectionForWidth);
        Assert.Equal(1u, (uint)(sut.GetField("exportedSymbols")!));
        Assert.Equal(1u, (uint)(sut.GetField("newSymbols")!));
        
    }
    
    
}