using Melville.JBig2.HuffmanTables;
using Melville.JBig2.Segments;

namespace Melville.Pdf.DataModelTests.Standard.S7_4Filters.S7_4_7Jbig2DecodeFilter;

public class SymbolDictionaryFlagsTest
{
    [Test]
    [Arguments(0,false)]
    [Arguments(1, true)]
    public void Huffman(ushort flags, bool value) => 
        Assert.Equal(value, new SymbolDictionaryFlags(flags).UseHuffmanEncoding);
    [Test]
    [Arguments(0,false)]
    [Arguments(2, true)]
    public void AggregateRefinement(ushort flags, bool value) => 
        Assert.Equal(value, new SymbolDictionaryFlags(flags).AggregateRefinement);

    
    [Test]
    [Arguments(0<<2, HuffmanTableSelection.B4)]
    [Arguments(1<<2, HuffmanTableSelection.B5)]
    [Arguments(3<<2, HuffmanTableSelection.UserSupplied)]
    public void HuffmanDH(ushort flags, object huffmanSel) =>
        Assert.Equal(huffmanSel, new SymbolDictionaryFlags(flags).HuffmanSelectionForHeight);
    [Test]
    [Arguments(0<<4, HuffmanTableSelection.B2)]
    [Arguments(1<<4, HuffmanTableSelection.B3)]
    [Arguments(3<<4, HuffmanTableSelection.UserSupplied)]
    public void HuffmanDW(ushort flags, object huffmanSel) =>
        Assert.Equal(huffmanSel, new SymbolDictionaryFlags(flags).HuffmanSelectionForWidth);
    [Test]
    [Arguments(0<<6, HuffmanTableSelection.B1)]
    [Arguments(1<<6, HuffmanTableSelection.UserSupplied)]
    public void HuffmanBMSize(ushort flags, object huffmanSel) =>
        Assert.Equal(huffmanSel, new SymbolDictionaryFlags(flags).HuffmanSelectionBitmapSize);
    [Test]
    [Arguments(0<<7, HuffmanTableSelection.B1)]
    [Arguments(1<<7, HuffmanTableSelection.UserSupplied)]
    public void HuffmanAggInst(ushort flags, object huffmanSel) =>
        Assert.Equal(huffmanSel, new SymbolDictionaryFlags(flags).HuffmanTableSelectionAggInst);

    [Test]
    [Arguments(0<<8,false)]
    [Arguments(1<<8, true)]
    public void BitmapContextUsed(ushort flags, bool value) => 
        Assert.Equal(value, new SymbolDictionaryFlags(flags).BitmapContextUsed);
    
    [Test]
    [Arguments(0<<9,false)]
    [Arguments(1<<9, true)]
    public void BitmapContextRetained(ushort flags, bool value) => 
        Assert.Equal(value, new SymbolDictionaryFlags(flags).BitmapContextRetained);

    [Test]
    [Arguments(0 << 10, GenericRegionTemplate.GB0)]
    [Arguments(1 << 10, GenericRegionTemplate.GB1)]
    [Arguments(2 << 10, GenericRegionTemplate.GB2)]
    [Arguments(3 << 10, GenericRegionTemplate.GB3)]
    public void TemplateSelection (ushort flags, object huffmanSel) =>
        Assert.Equal(huffmanSel, new SymbolDictionaryFlags(flags).GenericRegionTemplate);

    [Test]
    [Arguments(0<<12,false)]
    [Arguments(1<<12, true)]
    public void RefAggTemplate(ushort flags, bool value) => 
        Assert.Equal(value, new SymbolDictionaryFlags(flags).RefAggTeqmplate);
}