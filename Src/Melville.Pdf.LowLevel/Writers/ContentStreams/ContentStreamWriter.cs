using System;
using System.IO.Pipelines;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Writers.ObjectWriters;

namespace Melville.Pdf.LowLevel.Writers.ContentStreams;

public class ContentStreamWriter: IContentStreamOperations
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
    private void WriteDouble(double d) => destPipe.Advance(DoubleWriter.Write(d, destPipe.GetSpan(25)));
    private void WriteNewLine() => destPipe.WriteByte((byte)'\n');
    private void WriteSpace() => destPipe.WriteByte((byte)' ');

    public void SaveGraphicsState() => WriteOperator(ContentStreamOperatorNames.q);

    public void RestoreGraphicsState() => WriteOperator(ContentStreamOperatorNames.Q);

    public void ModifyTransformMatrix(double a, double b, double c, double d, double e, double f)
    {
        WriteDouble(a);
        WriteSpace();
        WriteDouble(b);
        WriteSpace();
        WriteDouble(c);
        WriteSpace();
        WriteDouble(d);
        WriteSpace();
        WriteDouble(e);
        WriteSpace();
        WriteDouble(f);
        WriteSpace();
        WriteOperator(ContentStreamOperatorNames.cm);
    }
    
    public void SetLineWidth(double width)
    {
        WriteDouble(width);
        WriteSpace();
        WriteOperator(ContentStreamOperatorNames.w);
    }

    public void SetLineCap(LineCap cap)
    {
        WriteDouble((double)cap);
        WriteSpace();
        WriteOperator(ContentStreamOperatorNames.J);
    }
}