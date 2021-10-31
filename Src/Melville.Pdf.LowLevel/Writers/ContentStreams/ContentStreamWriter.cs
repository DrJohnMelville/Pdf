using System;
using System.IO.Pipelines;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Writers.ObjectWriters;

namespace Melville.Pdf.LowLevel.Writers.ContentStreams;

public class ContentStreamWriter : IContentStreamOperations
{
    private readonly PipeWriter destPipe;

    public ContentStreamWriter(PipeWriter destPipe)
    {
        this.destPipe = destPipe;
    }

    private void WriteOperator(in ReadOnlySpan<byte> name)
    {
        destPipe.WriteBytes(name);
        WriteNewLine();
    }

    private void WriteOperator(double width, byte[] operation)
    {
        WriteDoubleAndSpace(width);
        WriteOperator(operation);
    }

    private void WriteDoubleAndSpace(double d)
    {
        WriteDouble(d);
        WriteSpace();
    }

    private void WriteDouble(double d) => destPipe.Advance(DoubleWriter.Write(d, destPipe.GetSpan(25)));

    private void WriteNewLine() => destPipe.WriteByte((byte)'\n');
    private void WriteSpace() => destPipe.WriteByte((byte)' ');

    public void SaveGraphicsState() => WriteOperator(ContentStreamOperatorNames.q);
    public void RestoreGraphicsState() => WriteOperator(ContentStreamOperatorNames.Q);

    public void ModifyTransformMatrix(double a, double b, double c, double d, double e, double f)
    {
        WriteDoubleAndSpace(a);
        WriteDoubleAndSpace(b);
        WriteDoubleAndSpace(c);
        WriteDoubleAndSpace(d);
        WriteDoubleAndSpace(e);
        WriteDoubleAndSpace(f);
        WriteOperator(ContentStreamOperatorNames.cm);
    }

    public void SetLineWidth(double width) => WriteOperator(width, ContentStreamOperatorNames.w);
    public void SetLineCap(LineCap cap) => WriteOperator((double)cap, ContentStreamOperatorNames.J);
    public void SetLineJoinStyle(LineJoinStyle cap) => 
        WriteOperator((double)cap, ContentStreamOperatorNames.j);
    public void SetMiterLimit(double miter) => WriteOperator(miter, ContentStreamOperatorNames.M);

    public void SetLineDashPattern(double dashPhase, ReadOnlySpan<double> dashArray)
    {
        WriteDoubleArray(dashArray);
        WriteSpace();
        WriteOperator(dashPhase, ContentStreamOperatorNames.d);
    }

    private void WriteDoubleArray(ReadOnlySpan<double> dashArray)
    {
        destPipe.WriteByte((byte)'[');
        if (dashArray.Length > 0)
        { 
            WriteDoublesWithSpaces(dashArray[..^1]);
            WriteDouble(dashArray[^1]);
        }
        destPipe.WriteByte((byte)']');
    }

    private void WriteDoublesWithSpaces(ReadOnlySpan<double> values)
    {
        foreach (var value in values)
        {
            WriteDoubleAndSpace(value);
        }
    }

    public void SetRenderIntent(RenderingIntentName intent)
    {
        NameWriter.WriteWithoutlush(destPipe, intent);
        WriteSpace();
        WriteOperator(ContentStreamOperatorNames.ri);
    }
}