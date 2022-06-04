using System.Diagnostics;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.Segments;

public enum GenericRegionTemplate: byte
{
    /// <summary>
    /// Value = 0
    /// </summary>
    GB0 = 0,
    /// <summary>
    /// Value =1
    /// </summary>
    GB1 = 1,
    /// <summary>
    /// Value = 2
    /// </summary>
    GB2 = 2,
    /// <summary>
    /// Value = 3
    /// </summary>
    GB3 = 3,
    
    RefinementReference0 = 4,
    RefinementReference1 = 5,
    RefinementDestination0 = 6,
    RefinementDestination1 = 7
}

