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
    
    private void WriteNewLine() => destPipe.WriteByte((byte)'\n');
    private void WriteSpace() => destPipe.WriteByte((byte)' ');

    public void SaveGraphicsState() => WriteOperator(ContentStreamOperatorNames.q);

    public void RestoreGraphicsState() => WriteOperator(ContentStreamOperatorNames.Q);

    public void ModifyTransformMatrix(double a, double b, double c, double d, double e, double f)
    {
        WriteOperator(ContentStreamOperatorNames.cm);
    }

    public void SetLineWidth(double width)
    {
        throw new System.NotImplementedException();
    }
}