using System;
using System.Diagnostics.Contracts;
using System.Formats.Asn1;
using Melville.Pdf.LowLevel.Model.Primitives.VariableBitEncoding;

namespace Melville.Pdf.LowLevel.Filters.CCITTFaxDecodeFilters;

public record struct CcittLineComparison(int A1, int A2, int B1, int B2)
{
    public bool CanVerticalEncode => Math.Abs(VerticalEncodingDelta) <= 3;
    public int VerticalEncodingDelta => A1 - B1;
    public bool CanPassEncode => B2 < A1;
}

