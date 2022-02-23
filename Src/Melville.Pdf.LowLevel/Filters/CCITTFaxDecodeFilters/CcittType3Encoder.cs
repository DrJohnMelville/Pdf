using System;
using System.Diagnostics;

namespace Melville.Pdf.LowLevel.Filters.CCITTFaxDecodeFilters;

public class CcittType3Encoder : CcittEncoderBase
{
    public CcittType3Encoder(in CcittParameters parameters) : base(parameters)
    {
    }

    protected override int TryWriteCurrentRow(Span<byte> destination)
    {
        var writer = new CcittBitWriter(destination, BitWriter);
        if (!TryWriteLinePrefix(ref writer)) return writer.BytesWritten;
        while (!DoneEncodingLine() && writer.HasRoomToWrite() && TryWriteRun(ref writer))
        {
            // do nothing
        }

        if (DoneEncodingLine()) TryResetForNextLine(ref writer);
        
        return writer.BytesWritten;
    }

    protected override void ResetForNextLine(ref CcittBitWriter writer)
    {
        var result = WriteEndOfLine(ref writer);
        Debug.Assert(result); // TryResetForNextLine makes this work every time.
        base.ResetForNextLine(ref writer);
    }

    private bool WriteEndOfLine(ref CcittBitWriter writer) =>
        writer.HasRoomToWrite(3) && writer.WriteEndOfLine(1, Parameters.K);

    private bool TryWriteRun(ref CcittBitWriter writer)
    {
        var a1 = Lines.StartOfNextRun(a0);
        if (!writer.WriteHorizontal(Lines.CurrentLine[a0], a1 - a0)) return false;
        a0 = a1;
        return true;
    }

    private bool TryWriteLinePrefix(ref CcittBitWriter writer)
    {
        if (!EncodingImaginaryFirstPixel()) return true;
        if (! (Lines.CurrentLine[0] || WriteEmptyWhiteRun(ref writer))) return false;
        a0 = 0;
        return true;
    }

    private bool EncodingImaginaryFirstPixel() => a0 <0;
    private bool WriteEmptyWhiteRun(ref CcittBitWriter writer) => writer.WriteHorizontal(true, 0);
}