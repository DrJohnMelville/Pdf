using System;
using System.Diagnostics;

namespace Melville.Pdf.LowLevel.Filters.CCITTFaxDecodeFilters;

public class CcittType3Encoder : CcittEncoderBase
{
    public CcittType3Encoder(in CcittParameters parameters) : base(parameters)
    {
    }
    
    protected override void WriteLineSuffix(ref CcittBitWriter writer)
    {
        bool success = writer.WriteEndOfLine(1, Parameters.K);
        Debug.Assert(success);
    }

    private bool WriteEndOfLine(ref CcittBitWriter writer) =>
        writer.HasRoomToWrite(3) && writer.WriteEndOfLine(1, Parameters.K);

    protected override bool TryWriteRun(ref CcittBitWriter writer)
    {
        var a1 = Lines.StartOfNextRun(a0);
        if (!writer.WriteHorizontal(Lines.CurrentLine[a0], a1 - a0)) return false;
        a0 = a1;
        return true;
    }

    protected override bool TryWriteLinePrefix(ref CcittBitWriter writer)
    {
        if (!EncodingImaginaryFirstPixel()) return true;
        if (! (Lines.CurrentLine[0] || WriteEmptyWhiteRun(ref writer))) return false;
        a0 = 0;
        return true;
    }

    private bool EncodingImaginaryFirstPixel() => a0 <0;
    private bool WriteEmptyWhiteRun(ref CcittBitWriter writer) => writer.WriteHorizontal(true, 0);
}