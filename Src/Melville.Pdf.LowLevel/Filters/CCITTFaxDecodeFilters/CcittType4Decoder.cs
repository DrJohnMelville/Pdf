
using System;
using System.Buffers;
using System.Diagnostics;
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
  private int linesDone = 0;
  
  public CcittType4Decoder(CcittParameters parameters)
  {
    this.parameters = parameters;
    lines = new CcittLinePair(parameters);
  }

  public (SequencePosition SourceConsumed, int bytesWritten, bool Done) Convert(
  ref SequenceReader<byte> source, ref Span<byte> destination)
  {
    var localDestination = destination;
    var doneReading = InnerConvertLoop(ref source, ref localDestination);
    return (source.Position, destination.Length - localDestination.Length, doneReading);
  }

  private bool InnerConvertLoop(ref SequenceReader<byte> source, ref Span<byte> destination)
  {
    while (destination.Length > 0)
    {
      if (DoneReadingLine() && !WriteCurrentLine(ref destination)) return false;
      if (parameters.HasReadEntireImage(linesDone))
        return true;
      if (!ReadLine(ref source)) return false;
    }
    return false;
  }

  private bool DoneReadingLine() => a0 >= lines.LineLength-1;

  public (SequencePosition SourceConsumed, int bytesWritten, bool Done) FinalConvert(
    ref SequenceReader<byte> source, ref Span<byte> destination) =>
    (source.Position, writer.FinishWrite(destination), true);

  private int a0 = -1;
  private bool ReadLine(ref SequenceReader<byte> source)
  {
    while (a0 < lines.LineLength-1)
    {
      if (!reader.TryReadCode(ref source, currentRunColor, out var code)) return false;
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
  }

  private void DoPass() => FillRunTo(lines.ComputeB2(a0));

  private void DoBlack(ushort codeLength)
  {
    Debug.Assert(!currentRunColor);
    DoHorizontalRun(codeLength);
  }

  private void DoWhite(ushort codeLength)
  {
    Debug.Assert(currentRunColor);
   DoHorizontalRun(codeLength);
  }

  private void DoHorizontalRun(ushort codeLength)
  {
    if (codeLength > 0) FillRunTo(a0+1+codeLength);
    SwitchRunColor();
  }
  

  private void DoVertical(int delta)
  {
    var b1 = lines.ComputeB1(a0, currentRunColor);
    FillRunTo(b1+delta);
    SwitchRunColor();
  }

  private bool currentRunColor = true;
  private void SwitchRunColor()
  {
    currentRunColor = !currentRunColor;
  }

  private void FillRunTo(int exclusiveLastPoint)
  {
    for (int i = a0+1; i < Math.Min(exclusiveLastPoint, lines.LineLength); i++)
    {
      lines.CurrentLine[i] = currentRunColor;
    }

    a0 = exclusiveLastPoint-1;
    Debug.Assert(a0 >= -1 && a0 <= lines.LineLength);
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
    currentWritePosition = 0;
    a0 = -1;
    lines = lines.SwapLines();
    linesDone++;
    currentRunColor = true;
  }

  private byte CurrentPixelBit() => parameters.ByteForColor(lines.CurrentLine[currentWritePosition]);
}