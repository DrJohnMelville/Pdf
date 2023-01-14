using System;

namespace Melville.Pdf.LowLevel.Writers.ContentStreams;

public unsafe struct DeferedClosingTask : IDisposable
{
    private readonly ContentStreamPipeWriter writer;
    private fixed byte closingOperator[4];
    private readonly int closingOperatorLength;
    private Span<byte> ClosingOperatorSpan(byte* basePtr) =>
        new Span<byte>(basePtr, Math.Min(4, closingOperatorLength));

    internal DeferedClosingTask(ContentStreamPipeWriter writer, in ReadOnlySpan<byte> closingOperator)
    {
        this.writer = writer;
        closingOperatorLength = closingOperator.Length;
        fixed (byte* opPtr = this.closingOperator)
        {
            closingOperator.CopyTo(ClosingOperatorSpan(opPtr));
        }
    }

    /// <summary>
    /// Writes out the closing operator for this block
    /// </summary>
    public void Dispose()
    {
        fixed (byte* opPtr = closingOperator)
        {
            writer.WriteOperator(ClosingOperatorSpan(opPtr));
        }
    }
}