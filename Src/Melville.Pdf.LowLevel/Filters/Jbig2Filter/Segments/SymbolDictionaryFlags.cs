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
    public bool UseHuffmanEncoding => BitOperations.CheckBit(flags, 1<<0);
    
    /// <summary>
    /// In standard SDREFAGG
    /// </summary>
    public bool AggregateRefinement => BitOperations.CheckBit(flags, 1<<1);

    /// <summary>
    /// In standard SDHUFFDH
    /// </summary>
    public HuffmanTableSelection HuffmanSelectionForHeight =>
        HuffmanTableSelector.Select(flags, 2, HuffmanTableSelection.B4, HuffmanTableSelection.B5);

    /// <summary>
    /// In standard SDHUFFDW
    /// </summary>
    public HuffmanTableSelection HuffmanSelectionForWidth =>
        HuffmanTableSelector.Select(flags, 4, HuffmanTableSelection.B2, HuffmanTableSelection.B3);
        
    /// <summary>
    /// In standard SDHUFFBMSIZE
    /// </summary>
    public HuffmanTableSelection HuffmanSelectionBitmapSize =>
        BitOperations.CheckBit(flags, 1 << 6) ? HuffmanTableSelection.UserSupplied : HuffmanTableSelection.B1;
    
    /// <summary>
    /// In standard SDHUFFAGHGINST
    /// </summary>
    public HuffmanTableSelection HuffmanTableSelectionAggInst =>
        BitOperations.CheckBit(flags, 1 << 7) ? HuffmanTableSelection.UserSupplied : HuffmanTableSelection.B1;

    public bool BitmapContextUsed => BitOperations.CheckBit(flags, 1 << 8);
    public bool BitmapContextRetained => BitOperations.CheckBit(flags, 1 << 9);

    /// <summary>
    /// In Standard SDTEMPLATE
    /// </summary>
    public GenericRegionTemplate GenericRegionTemplate => 
        (GenericRegionTemplate)((flags >> 10) & 0x03);

    /// <summary>
    /// In Standard SDRTEMPLATE
    /// </summary>
    public bool RefAggTeqmplate => BitOperations.CheckBit(flags, 1 << 12);
}