
using System;
using System.Buffers;
using Melville.Pdf.LowLevel.Filters.StreamFilters;
using Melville.Pdf.LowLevel.Model.Objects.StringEncodings;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Model.Primitives.VariableBitEncoding;

namespace Melville.Pdf.LowLevel.Filters.CCITTFaxDecodeFilters;

public class CcittType4Decoder : IStreamFilterDefinition
{
  private readonly CcittParameters parameters;
  private CcittLinePair lines;
  private readonly CcittCodeReader reader = new ();
  private readonly BitWriter writer = new();
  
  public CcittType4Decoder(CcittParameters parameters)
  {
    this.parameters = parameters;
    lines = new CcittLinePair(parameters);
  }

  public (SequencePosition SourceConsumed, int bytesWritten, bool Done) Convert(
  ref SequenceReader<byte> source, ref Span<byte> destination)
  {
    var localDestination = destination;
    while (localDestination.Length > 0)
    {
      if (currentRunPosition >= lines.LineLength && !WriteCurrentLine(ref localDestination)) continue;
      if (!ReadLine(ref source)) break;
    } 
    return (source.Position, destination.Length - localDestination.Length, false);
  }

  public (SequencePosition SourceConsumed, int bytesWritten, bool Done) FinalConvert(
    ref SequenceReader<byte> source, ref Span<byte> destination) =>
    (source.Position, writer.FinishWrite(destination), true);

  private int currentRunPosition = 0;
  private bool currentRunIsWhite = true;
  private bool ReadLine(ref SequenceReader<byte> source)
  {
    while (currentRunPosition < lines.LineLength)
    {
      if (!reader.TryReadCode(ref source, currentRunIsWhite, out var code)) return false;
      ProcessCode(code);
    }
    return true;
  }

  private void ProcessCode(CcittCode code)
  {
    switch (code.Operation)
    {
      case CcittCodeOperation.Pass: DoPass(); break;
      case CcittCodeOperation.HorizontalBlack: DoBlack(code.Length); break;
      case CcittCodeOperation.HorizontalWhite: DoWhite(code.Length); break;
      case CcittCodeOperation.Vertical: DoVertical(code.Length - 3); break;
      case CcittCodeOperation.MakeUp:
      case CcittCodeOperation.SwitchToHorizontalMode:
      default:
        throw new PdfParseException($"Code {code.Operation} should not have escaped the CccitCodeReader");
    }

    currentRunIsWhite = !currentRunIsWhite;
  }

  private void DoPass()
  {
    throw new NotImplementedException();
  }

  private void DoBlack(ushort codeLength)
  {
    throw new NotImplementedException();
  }

  private void DoWhite(ushort codeLength)
  {
    throw new NotImplementedException();
  }

  private void DoVertical(int delta)
  {
    var b1 = lines.ComputeB1(currentRunPosition - 1);
    var runLength = b1 + delta;
    FillRun(runLength);
  }

  private void FillRun(int runLength)
  {
    for (int i = 0; i < runLength; i++)
    {
      lines.CurrentLine[currentRunPosition + i] = currentRunIsWhite;
    }
    currentRunPosition += runLength;
  }


  int currentWritePosition = 0; 
  private bool WriteCurrentLine(ref Span<byte> destination)
  {
    TryWriteCurrentLineToSpan(ref destination);
    return CheckIfLineWriteComplete();
  }

  private void TryWriteCurrentLineToSpan(ref Span<byte> destination)
  {
    for (; currentWritePosition < lines.LineLength && destination.Length > 0; currentWritePosition++)
    {
      var bytesWritten = writer.WriteBits(CurrentPixelBit(), 1, destination);
      destination = destination[bytesWritten..];
    }
  }

  private bool CheckIfLineWriteComplete()
  {
    if (!LineCompletelyWritten()) return false;
    ResetReaderForNextLine();
    return true;
  }

  private bool LineCompletelyWritten() => currentWritePosition >= lines.LineLength;

  private void ResetReaderForNextLine()
  {
    if (parameters.EncodedByteAlign) reader.DiscardPartialByte();
    currentRunIsWhite = true;
    currentWritePosition = 0;
    currentRunPosition = 0;
    lines = lines.SwapLines();
  }

  private byte CurrentPixelBit() => parameters.ByteForColor(lines.CurrentLine[currentWritePosition]);
}