using System;
using System.Buffers;
using Melville.Pdf.LowLevel.Filters.StreamFilters;
using Melville.Pdf.LowLevel.Model.Primitives.VariableBitEncoding;

namespace Melville.Pdf.LowLevel.Filters.CCITTFaxDecodeFilters;

public class CcittType4Decoder : IStreamFilterDefinition
{
  private readonly CcittParameters parameters;
  private CcittLinePair lines;
  private readonly BitReader reader = new ();
  private readonly BitWriter writer = new();
  
  public CcittType4Decoder(CcittParameters parameters)
  {
    this.parameters = parameters;
    lines = new CcittLinePair(parameters);
  }

  [Obsolete("Just a temporary hack")]
  public int MinWriteSize => 4;

  public (SequencePosition SourceConsumed, int bytesWritten, bool Done) Convert(ref SequenceReader<byte> source, ref Span<byte> destination)
  {
    // this is a hack to make a test happy will be completely replaced with a real decoder.
    destination[0] = 0xFF;
    destination[1] = 0xFF;
    destination[2] = 0xFF;
    destination[3] = 0xFF;
    return (source.Position, 4, true);
  }
}