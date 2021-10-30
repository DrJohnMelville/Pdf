using System;
using System.IO.Pipelines;
using Melville.Pdf.LowLevel.Model.ContentStreams;

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
        int length = name.Length + 1;
        var span = destPipe.GetSpan(length);
        name.CopyTo(span);
        span[name.Length] = (byte)'\n';
        destPipe.Advance(length);
    }

    public void SaveGraphicsState() => WriteOperator(ContentStreamOperatorNames.q);

    public void RestoreGraphicsState() => WriteOperator(ContentStreamOperatorNames.Q);

    public void ModifyTransformMatrix(params double[] xFromParameters)
    {
        throw new System.NotImplementedException();
    }

    public void SetLineWidth(double width)
    {
        throw new System.NotImplementedException();
    }
}