using System.IO;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.MultiplexSources;
using Melville.Parsing.StreamFilters;
using Melville.Parsing.Streams;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Writers.ObjectWriters;

namespace Melville.Pdf.LowLevel.Writers.Builder;

internal readonly struct ObjectStreamWriter
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

    public ValueTask<FlushResult> TryAddRefAsync(int objectNumber, PdfDirectObject obj)
    {
        WriteObjectPosition(objectNumber);
        return WriteObjectAsync(obj);
    }

    private void WriteObjectPosition(int objectNumber)
    {
        IntegerWriter.Write(referenceStreamWriter, (long)objectNumber);
        referenceStreamWriter.WriteSpace();
        IntegerWriter.Write(referenceStreamWriter, objectStreamWriter.BytesWritten);
        referenceStreamWriter.WriteSpace();
    }

    private ValueTask<FlushResult> WriteObjectAsync(PdfDirectObject directValue)
    {           
        objectWriter.Write(directValue);
        objectStreamWriter.WriteLineFeed();
        return objectStreamWriter.FlushAsync();
    }

    public async ValueTask<PdfStream> BuildAsync(DictionaryBuilder builder, int count)
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
        new ConcatStream(((IMultiplexSource)refs).ReadFrom(0), ((IMultiplexSource)objects).ReadFrom(0));
}