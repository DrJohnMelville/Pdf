using System;
using System.Buffers;

namespace Melville.Pdf.LowLevel.Filters.CCITTFaxDecodeFilters;

public class CcittType3Encoder : CcittEncoderBase
{
    public CcittType3Encoder(in CcittParameters parameters) : base(parameters)
    {
    }

    protected override int TryWriteCurrentRow(Span<byte> destination)
    {
        throw new NotImplementedException();
    }
}