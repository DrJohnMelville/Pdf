namespace Melville.Pdf.LowLevel.Filters.CCITTFaxDecodeFilters;

public class CcittType4Encoder : CcittEncoderBase
{
   
    public CcittType4Encoder(in CcittParameters parameters): base(parameters)
    {
    }

    protected override ILineEncoder CurrentLineEncoder() => LineEncoder2D.Instance;
    protected override void WriteLineSuffix(ref CcittBitWriter writer) { }
}