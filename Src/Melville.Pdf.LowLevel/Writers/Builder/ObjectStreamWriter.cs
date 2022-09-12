using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.StreamFilters;
using Melville.Parsing.Streams;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Writers.ObjectWriters;

namespace Melville.Pdf.LowLevel.Writers.Builder;

public readonly struct ObjectStreamWriter
{
    private readonly MultiBufferStream refs;
    private readonly CountingPipeWriter referenceStreamWriter;
    private readonly MultiBufferStream objects;
    private readonly CountingPipeWriter objectStreamWriter;
    private readonly PdfObjectWriter objectWriter;

    public ObjectStreamWriter()
    {
        refs = new();
        objects = new();
        referenceStreamWriter = new CountingPipeWriter(PipeWriter.Create(refs));
        objectStreamWriter = new CountingPipeWriter(PipeWriter.Create(objects));
        objectWriter = new PdfObjectWriter(objectStreamWriter);
    }

    public async ValueTask TryAddRefAsync(PdfIndirectObject obj)
    {
        var direcetValue = await obj.DirectValueAsync().CA();
        WriteObjectPosition(obj);
        await WriteObject(direcetValue).CA();
    }

    private void WriteObjectPosition(PdfIndirectObject item)
    {
        IntegerWriter.Write(referenceStreamWriter, item.ObjectNumber);
        referenceStreamWriter.WriteSpace();
        IntegerWriter.Write(referenceStreamWriter, objectStreamWriter.BytesWritten);
        referenceStreamWriter.WriteSpace();
    }

    private async ValueTask WriteObject(PdfObject directValue)
    {
        await directValue.Visit(objectWriter).CA();
        objectStreamWriter.WriteLineFeed();
    }

    public async ValueTask<PdfStream> Build(DictionaryBuilder builder, int count)
    {
        await referenceStreamWriter.FlushAsync().CA();
        await objectStreamWriter.FlushAsync().CA();
        return builder
            .WithItem(KnownNames.Type, KnownNames.ObjStm)
            .WithItem(KnownNames.N, count)
            .WithItem(KnownNames.First, referenceStreamWriter.BytesWritten)
            .AsStream(FinalStreamContent());        
    }
    private ConcatStream FinalStreamContent() => 
        new ConcatStream(refs.CreateReader(), objects.CreateReader());
}