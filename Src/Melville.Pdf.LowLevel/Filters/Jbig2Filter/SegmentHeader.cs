namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter;

public enum SegmentType : byte
{
    SymbolDictionary = 0,
    
    IntermediateTextRegion = 4,
    ImmediateTextRegion = 6,
    ImmediateLosslessTextRegion = 7,
    
    PatternDictionary = 16,
    
    IntermediateHalftoneRegion=20,
    ImmediateHalftoneRegion=22,
    ImmediateLosslessHalftoneRegion=23,
    
    IntermediateGenericRegion = 36,
    ImmediateGenericRegion = 38,
    ImmediateLosslessGenericRegion = 39,
    
    IntermediateGenericRefinementRegion = 40,
    ImmediateGenericRefinementRegion = 42,
    ImmediateLosslessGenericRefinementRegion = 43,
    
    PageInformation = 48,
    EndOfPage = 49,
    EndOfStripe = 50,
    EndOfFile= 51,
    Profiles = 52,
    Tables = 53,
    Extension = 62
}

public record struct SegmentHeader(uint Number, SegmentType SegmentType, uint Page, uint DataLength,
    uint[] ReferencedSegmentNumbers)
{
    
}