using System.IO;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.HuffmanTables;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.Segments;

public readonly struct SymbolDictionaryFlags
{
    private readonly ushort flags;

    public SymbolDictionaryFlags(ushort flags)
    {
        this.flags = flags;
    }

    /// <summary>
    /// In Standard SDHHUFF
    /// </summary>
    public bool UseHuffmanEncoding => CheckBit(1<<0);
    
    /// <summary>
    /// In standard SDREFAGG
    /// </summary>
    public bool AggregateRefinement => CheckBit(1<<1);

    private bool CheckBit(ushort template) => (flags & template) == template;
    
    /// <summary>
    /// In standard SDHUFFDH
    /// </summary>
    public HuffmanTableSelection HuffmanSelectionForHeight =>
        (flags & 3 << 2) switch
        {
            0x00<<2 => HuffmanTableSelection.B4,
            0x01<<2 => HuffmanTableSelection.B5,
            0x03<<2 => HuffmanTableSelection.UserSupplied,
            _=> throw new InvalidDataException("Invalid Huffman Table Selector")
        };

    /// <summary>
    /// In standard SDHUFFDW
    /// </summary>
    public HuffmanTableSelection HuffmanSelectionForWidth =>
        (flags & 3 << 4) switch
        {
            0x00<<4 => HuffmanTableSelection.B2,
            0x01<<4 => HuffmanTableSelection.B3,
            0x03<<4 => HuffmanTableSelection.UserSupplied,
            _=> throw new InvalidDataException("Invalid Huffman Table Selector")
        };

    /// <summary>
    /// In standard SDHUFFBMSIZE
    /// </summary>
    public HuffmanTableSelection HuffmanSelectionBitmapSize =>
        CheckBit(1 << 6) ? HuffmanTableSelection.UserSupplied : HuffmanTableSelection.B1;
    
    /// <summary>
    /// In standard SDHUFFAGHGINST
    /// </summary>
    public HuffmanTableSelection HuffmanTableSelectionAggInst =>
        CheckBit(1 << 7) ? HuffmanTableSelection.UserSupplied : HuffmanTableSelection.B1;

    public bool BitmapContextUsed => CheckBit(1 << 8);
    public bool BitmapContextRetained => CheckBit(1 << 9);

    /// <summary>
    /// In Standard SDTEMPLATE
    /// </summary>
    public SymbolDictionaryTemplate SymbolDictionaryTemplate => 
        (SymbolDictionaryTemplate)((flags >> 10) & 0x03);

    /// <summary>
    /// In Standard SDRTEMPLATE
    /// </summary>
    public bool RefAggTeqmplate => CheckBit(1 << 12);
}