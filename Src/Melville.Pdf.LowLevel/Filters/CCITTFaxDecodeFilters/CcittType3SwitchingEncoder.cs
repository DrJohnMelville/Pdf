using System;
using System.Diagnostics;

namespace Melville.Pdf.LowLevel.Filters.CCITTFaxDecodeFilters;

public class CcittType31dEncoder : CcittEncoderBase
{
    public CcittType31dEncoder(CcittParameters parameters) : base(parameters)
    {
    }

    protected override ILineEncoder CurrentLineEncoder() => LineEncoder1D.Instance;

    protected override void WriteLineSuffix(ref CcittBitWriter writer)
    {
        var result = writer.WriteEndOfLineCode();
        Debug.Assert(result);
    }
}
public class CcittType3SwitchingEncoder : CcittEncoderBase
{
    private int positionInRowGroup = 0;
    public CcittType3SwitchingEncoder(in CcittParameters parameters) : base(parameters)
    {
    }

    protected override ILineEncoder CurrentLineEncoder() =>
        ShouldEncodeLine1D() ? LineEncoder1D.Instance : LineEncoder2D.Instance;

    private bool ShouldEncodeLine1D() => positionInRowGroup == 0;

    protected override void WriteLineSuffix(ref CcittBitWriter writer)
    {
        AdvanceLineGroupIndicator();
        var result = writer.WriteEndOfLineCode() && 
                     writer.WhiteNextLineEncodingBit(ShouldEncodeLine1D());
        Debug.Assert(result);
    }

    private void AdvanceLineGroupIndicator() => 
        positionInRowGroup = (positionInRowGroup + 1) % Parameters.K;
}