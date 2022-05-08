using System.Runtime.InteropServices;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.HuffmanTables;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.Segments;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_4Filters.S7_4_7Jbig2DecodeFilter;

public class SymbolDictionaryFlagsTest
{
    [Theory]
    [InlineData(0,false)]
    [InlineData(1, true)]
    public void Huffman(ushort flags, bool value) => 
        Assert.Equal(value, new SymbolDictionaryFlags(flags).UseHuffmanEncoding);
    [Theory]
    [InlineData(0,false)]
    [InlineData(2, true)]
    public void AggregateRefinement(ushort flags, bool value) => 
        Assert.Equal(value, new SymbolDictionaryFlags(flags).AggregateRefinement);

    
    [Theory]
    [InlineData(0<<2, HuffmanTableSelection.B4)]
    [InlineData(1<<2, HuffmanTableSelection.B5)]
    [InlineData(3<<2, HuffmanTableSelection.UserSupplied)]
    public void HuffmanDH(ushort flags, HuffmanTableSelection huffmanSel) =>
        Assert.Equal(huffmanSel, new SymbolDictionaryFlags(flags).HuffmanSelectionForHeight);
    [Theory]
    [InlineData(0<<4, HuffmanTableSelection.B2)]
    [InlineData(1<<4, HuffmanTableSelection.B3)]
    [InlineData(3<<4, HuffmanTableSelection.UserSupplied)]
    public void HuffmanDW(ushort flags, HuffmanTableSelection huffmanSel) =>
        Assert.Equal(huffmanSel, new SymbolDictionaryFlags(flags).HuffmanSelectionForWidth);
    [Theory]
    [InlineData(0<<6, HuffmanTableSelection.B1)]
    [InlineData(1<<6, HuffmanTableSelection.UserSupplied)]
    public void HuffmanBMSize(ushort flags, HuffmanTableSelection huffmanSel) =>
        Assert.Equal(huffmanSel, new SymbolDictionaryFlags(flags).HuffmanSelectionBitmapSize);
    [Theory]
    [InlineData(0<<7, HuffmanTableSelection.B1)]
    [InlineData(1<<7, HuffmanTableSelection.UserSupplied)]
    public void HuffmanAggInst(ushort flags, HuffmanTableSelection huffmanSel) =>
        Assert.Equal(huffmanSel, new SymbolDictionaryFlags(flags).HuffmanTableSelectionAggInst);

    [Theory]
    [InlineData(0<<8,false)]
    [InlineData(1<<8, true)]
    public void BitmapContextUsed(ushort flags, bool value) => 
        Assert.Equal(value, new SymbolDictionaryFlags(flags).BitmapContextUsed);
    
    [Theory]
    [InlineData(0<<9,false)]
    [InlineData(1<<9, true)]
    public void BitmapContextRetained(ushort flags, bool value) => 
        Assert.Equal(value, new SymbolDictionaryFlags(flags).BitmapContextRetained);

    [Theory]
    [InlineData(0 << 10, SymbolDictionaryTemplate.V0)]
    [InlineData(1 << 10, SymbolDictionaryTemplate.V1)]
    [InlineData(2 << 10, SymbolDictionaryTemplate.V2)]
    [InlineData(3 << 10, SymbolDictionaryTemplate.V3)]
    public void TemplateSelection (ushort flags, SymbolDictionaryTemplate huffmanSel) =>
        Assert.Equal(huffmanSel, new SymbolDictionaryFlags(flags).SymbolDictionaryTemplate);

    [Theory]
    [InlineData(0<<12,false)]
    [InlineData(1<<12, true)]
    public void RefAggTemplate(ushort flags, bool value) => 
        Assert.Equal(value, new SymbolDictionaryFlags(flags).RefAggTeqmplate);
}