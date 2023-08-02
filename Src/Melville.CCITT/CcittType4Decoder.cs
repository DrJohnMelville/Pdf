using System.Buffers;
using System.Diagnostics;
using Melville.Parsing.VariableBitEncoding;

namespace Melville.CCITT;

internal class CcittType4Decoder : IJBigMmrFilter
{
  private readonly CcittParameters parameters;
  private LinePair lines;
  private readonly CcittCodeReader reader;
  private readonly ICodeDictionay codeDictionay;
  private readonly BitWriter writer = new();
  private int linesDone = 0;
  
  public CcittType4Decoder(CcittParameters parameters, ICodeDictionay codeDictionay)
  {
    this.parameters = parameters;
    lines = new LinePair(parameters);
    this.codeDictionay = codeDictionay;
    reader = new(codeDictionay);
  }

  public (SequencePosition SourceConsumed, int bytesWritten, bool Done) Convert(
  ref SequenceReader<byte> source, in Span<byte> destination)
  {
    var localDestination = destination;
    var doneReading = InnerConvertLoop(ref source, ref localDestination);
    return (source.Position, destination.Length - localDestination.Length, doneReading);
  }

  private bool InnerConvertLoop(ref SequenceReader<byte> source, ref Span<byte> destination)
  {
    while (destination.Length > 0)
    {
      Debug.Assert(a0IsNextPixelToWrite <= lines.LineLength);
      if (DoneReadingLine() && !WriteCurrentLine(ref destination)) return false;
      if (parameters.HasReadEntireImage(linesDone))
        return true;
      if (!ReadLine(ref source)) return false;
    }
    return false;
  }

  private bool DoneReadingLine() => a0IsNextPixelToWrite >= lines.LineLength;

  public (SequencePosition SourceConsumed, int bytesWritten, bool Done) FinalConvert(
    ref SequenceReader<byte> source, in Span<byte> destination) =>
    (source.Position, writer.FinishWrite(destination), true);

  private int a0IsNextPixelToWrite = -1;
  private bool ReadLine(ref SequenceReader<byte> source)
  {
    while (!codeDictionay.IsAtValidEndOfLine || a0IsNextPixelToWrite < lines.LineLength)
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
      case CcittCodeOperation.Vertical: DoVertical(code.VerticalOffset);  break;
      case CcittCodeOperation.EndOfLine: DoEndOfLine(); break;
      default:
        throw new InvalidDataException($"Code {code.Operation} should not have escaped the CccitCodeReader");
    }
  }

  private void DoEndOfLine() => Debug.Assert(a0IsNextPixelToWrite >= lines.LineLength);

  private void DoPass() => FillRunTo(lines.ComputeB2(a0IsNextPixelToWrite, currentRunColor));

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
    if (codeLength > 0) FillRunTo(Math.Max(a0IsNextPixelToWrite,0)+codeLength);
    SwitchRunColor();
  }
  

  private void DoVertical(int delta)
  {
    FillRunTo(lines.ComputeB1(a0IsNextPixelToWrite, currentRunColor)+delta);
    SwitchRunColor();
  }

  private bool currentRunColor = true;
  private void SwitchRunColor()
  {
    currentRunColor = !currentRunColor;
  }

  private void FillRunTo(int exclusiveLastPoint)
  {
    for (int i = Math.Max(0,a0IsNextPixelToWrite); i < Math.Min(exclusiveLastPoint, lines.LineLength); i++)
    {
      lines.CurrentLine[i] = currentRunColor;
    }

    a0IsNextPixelToWrite = exclusiveLastPoint; 
    Debug.Assert(a0IsNextPixelToWrite > -1 && a0IsNextPixelToWrite <= lines.LineLength);
  }


  private bool WriteCurrentLine(ref Span<byte> destination)
  {
    TryWriteCurrentLineToSpan(ref destination);
    return CheckIfLineWriteComplete();
  }

  private int currentWritePosition = 0; 
  private void TryWriteCurrentLineToSpan(ref Span<byte> destination)
  {
    var (read, written) = writer.WriteBitSpan(lines.CurrentLine.AsSpan(currentWritePosition..),
      destination, new BitToByte(parameters.WhiteByte, parameters.BlackByte));
    destination = destination[written..];
    currentWritePosition += read;

    if (destination.Length > 0)
    {
      var finalLength = writer.FinishWrite(destination);
      destination = destination[finalLength..];
    }
  }

  private bool CheckIfLineWriteComplete()
  {
    if (!LineCompletelyWritten()) return false;
    ResetReaderForNextLine();
    return true;
  }

  private bool LineCompletelyWritten() => currentWritePosition >= lines.LineLength && writer.NoBitsWaitingToBeWritten();

  private void ResetReaderForNextLine()
  {
    if (parameters.EncodedByteAlign) reader.DiscardPartialByte();
    currentWritePosition = 0;
    a0IsNextPixelToWrite = -1;
    lines = lines.SwapLines();
    linesDone++;
    currentRunColor = true;
  }

  public void RequireTerminator(ref SequenceReader<byte> source)
  {
        if (!this.reader.TryReadEndOfFileCode(ref source, out var done) || !done)
        {
            throw new InvalidDataException("An expected CCIT End of Frame code is not found");
        }
    }
}