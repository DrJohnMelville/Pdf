using System;
using System.IO.Pipelines;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Writers.ObjectWriters;

namespace Melville.Pdf.LowLevel.Writers.ContentStreams;

public readonly struct ContentStreamPipeWriter
{
    private readonly PipeWriter destPipe;

    public ContentStreamPipeWriter(PipeWriter destPipe)
    {
        this.destPipe = destPipe;
    }

    public void WriteOperator(in ReadOnlySpan<byte> name)
    {
        destPipe.WriteBytes(name);
        WriteNewLine();
    }

    public void WriteOperator(byte[] @operator, PdfName name)
    {
        NameWriter.WriteWithoutlush(destPipe, name);
        WriteSpace();
        WriteOperator(@operator);
    }

    public void WriteOperator(in ReadOnlySpan<byte> operation, double value)
    {
        WriteDoubleAndSpace(value);
        WriteOperator(operation);
    }

    public void WriteOperator(in ReadOnlySpan<byte> operation, params double[] values)
    {
        foreach (var value in values)
        {
            WriteDoubleAndSpace(value);
        }
        WriteOperator(operation);
    }

    public void WriteDoubleAndSpace(double d)
    {
        WriteDouble(d);
        WriteSpace();
    }

    public void WriteDouble(double d) => destPipe.Advance(DoubleWriter.Write(d, destPipe.GetSpan(25)));

    public void WriteNewLine() => destPipe.WriteByte((byte)'\n');
    public void WriteSpace() => destPipe.WriteByte((byte)' ');

    public void WriteDoubleArray(ReadOnlySpan<double> dashArray)
    {
        destPipe.WriteByte((byte)'[');
        if (dashArray.Length > 0)
        { 
            WriteDoublesWithSpaces(dashArray[..^1]);
            WriteDouble(dashArray[^1]);
        }
        destPipe.WriteByte((byte)']');
        WriteSpace();
    }
    private void WriteDoublesWithSpaces(ReadOnlySpan<double> values)
    {
        foreach (var value in values)
        {
            WriteDoubleAndSpace(value);
        }
    }
}