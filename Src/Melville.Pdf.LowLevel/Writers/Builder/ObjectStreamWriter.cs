using System;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.StreamFilters;
using Melville.Parsing.Streams;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Objects2;
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

    public async ValueTask TryAddRefAsync(int objectNumber, PdfDirectValue obj)
    {
        WriteObjectPosition(objectNumber);
        await WriteObjectAsync(obj).CA();
    }

    private void WriteObjectPosition(int objectNumber)
    {
        IntegerWriter.Write(referenceStreamWriter, (long)objectNumber);
        referenceStreamWriter.WriteSpace();
        IntegerWriter.Write(referenceStreamWriter, objectStreamWriter.BytesWritten);
        referenceStreamWriter.WriteSpace();
    }

    private async ValueTask WriteObjectAsync(PdfDirectValue directValue)
    {            throw new NotSupportedException("Obsolete Object");

//        await directValue.Visit(objectWriter).CA();
        objectStreamWriter.WriteLineFeed();
    }

    public async ValueTask<PdfValueStream> BuildAsync(ValueDictionaryBuilder builder, int count)
    {
        await referenceStreamWriter.FlushAsync().CA();
        await objectStreamWriter.FlushAsync().CA();
        return builder
            .WithItem(KnownNames.TypeTName, KnownNames.ObjStmTName)
            .WithItem(KnownNames.NTName, count)
            .WithItem(KnownNames.FirstTName, referenceStreamWriter.BytesWritten)
            .AsStream(FinalStreamContent());        
    }
    private ConcatStream FinalStreamContent() => 
        new ConcatStream(refs.CreateReader(), objects.CreateReader());
}