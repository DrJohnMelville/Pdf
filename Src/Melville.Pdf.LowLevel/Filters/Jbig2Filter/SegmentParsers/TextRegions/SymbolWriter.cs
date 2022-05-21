using System;
using Melville.Pdf.LowLevel.Filters.CryptFilters.BitmapSymbols;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.HuffmanTables;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.Segments;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.SegmentParsers.TextRegions;

public ref struct SymbolWriter
{
    private readonly BinaryBitmapWriter target;
    private BitSource source;
    /// <summary>
    /// SBHUFFFS
    /// </summary>
    private readonly TextRegionFlags regionFlags;
    private readonly IIntegerDecoder firstSDecoder;
    /// <summary>
    /// SBHUFFFDS
    /// </summary>
    private readonly IIntegerDecoder deltaSDecoder;
    /// <summary>
    /// SBHUFFDT
    /// </summary>
    private readonly IIntegerDecoder deltaTDecoder;

    private readonly IIntegerDecoder symbolTDecoder;
    /// <summary>
    /// SBHUFFRDW
    /// </summary>
    private readonly IIntegerDecoder deltaRefinementWidthDecoder;
    /// <summary>
    /// SBHUFFRDH
    /// </summary>
    private readonly IIntegerDecoder deltaRefinementHeightDecoder;
    /// <summary>
    /// SBHUFFRDX
    /// </summary>
    private readonly IIntegerDecoder deltaRefinementXDecoder;
    /// <summary>
    /// SBHUFFRDY
    /// </summary>
    private readonly IIntegerDecoder deltaRefinementYDecoder;
    /// <summary>
    /// SBHUFFRDY
    /// </summary>
    private readonly IIntegerDecoder deltaRefinementSizeDecoder;
    private readonly CharacterDecoder characterDecoder;

    // these variables are the current decoding state
    private int remainingSymbolsToDecode;
    private int strIpT = 0;
    private int firstS = 0;
    private int curS = 0;

    public SymbolWriter(BinaryBitmapWriter target, in BitSource source, TextRegionFlags regionFlags,
        IIntegerDecoder firstSDecoder, IIntegerDecoder deltaSDecoder, IIntegerDecoder deltaTDecoder,
        IIntegerDecoder symbolTDecoder,
        IIntegerDecoder deltaRefinementWidthDecoder, IIntegerDecoder deltaRefinementHeightDecoder, 
        IIntegerDecoder deltaRefinementXDecoder, IIntegerDecoder deltaRefinementYDecoder, 
        IIntegerDecoder deltaRefinementSizeDecoder, CharacterDecoder characterDecoder, int symbolCount)
    {
        this.target = target;
        this.source = source;
        this.regionFlags = regionFlags;
        this.firstSDecoder = firstSDecoder;
        this.deltaSDecoder = deltaSDecoder;
        this.deltaTDecoder = deltaTDecoder;
        this.symbolTDecoder = symbolTDecoder;
        this.deltaRefinementWidthDecoder = deltaRefinementWidthDecoder;
        this.deltaRefinementHeightDecoder = deltaRefinementHeightDecoder;
        this.deltaRefinementXDecoder = deltaRefinementXDecoder;
        this.deltaRefinementYDecoder = deltaRefinementYDecoder;
        this.deltaRefinementSizeDecoder = deltaRefinementSizeDecoder;
        this.characterDecoder = characterDecoder;
        remainingSymbolsToDecode = symbolCount;
    }

    public void Decode()
    {
        var deltaT = deltaTDecoder.GetInteger(ref source) * regionFlags.StripSize;
        strIpT = -deltaT;
        while (remainingSymbolsToDecode > 0) DecodeStrip();
    }

    private void DecodeStrip()
    {
        strIpT += deltaTDecoder.GetInteger(ref source) * regionFlags.StripSize;

        firstS += firstSDecoder.GetInteger(ref source);
        curS = firstS;
        DecodeSymbol();
        while (deltaSDecoder.GetInteger(ref source) is { } deltaS and < int.MaxValue)
        {
            curS += deltaS;
            DecodeSymbol();
        }
        // decode subsequent symbols
    }

    private void DecodeSymbol()
    {
        var charT = symbolTDecoder.GetInteger(ref source) + strIpT;
        var symbol = characterDecoder.GetBitmap(ref source);
        target.WriteBitmap(charT, ref curS, symbol);
        curS += regionFlags.DefaultCharacteSpacing;
        remainingSymbolsToDecode--;
    }
}