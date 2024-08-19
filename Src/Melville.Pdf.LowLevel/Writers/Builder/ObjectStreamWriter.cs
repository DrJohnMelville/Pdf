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
    private readonly IWritableMultiplexSource refs;
    private readonly Melville.Parsing.Writers.CountingPipeWriter referenceStreamWriter;
    private readonly IWritableMultiplexSource objects;
    private readonly Melville.Parsing.Writers.CountingPipeWriter objectStreamWriter;
    private readonly PdfObjectWriter objectWriter;

    public ObjectStreamWriter()
    {
        refs = WritableBuffer.Create();
        objects = WritableBuffer.Create();
        referenceStreamWriter = refs.WritingPipe();
        objectStreamWriter = objects.WritingPipe();
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
    private ConcatStream FinalStreamContent() => new(refs.ReadFrom(0), objects.ReadFrom(0));
}