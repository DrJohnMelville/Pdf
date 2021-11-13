using System.Collections.Generic;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Filters.StreamFilters;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Writers.ObjectWriters;

namespace Melville.Pdf.LowLevel.Writers.Builder;

public class ObjectStreamBuilder
{
    private int count = 0;
    private readonly MultiBufferStream refs = new();
    private readonly CountingPipeWriter referenceStreamWriter;
    private readonly MultiBufferStream objects = new();
    private readonly CountingPipeWriter objectStreamWriter;
    private readonly PdfObjectWriter objectWriter;
    private readonly List<PdfIndirectObject> members = new();

    public ObjectStreamBuilder()
    {
        referenceStreamWriter = new CountingPipeWriter(PipeWriter.Create(refs));
        objectStreamWriter = new CountingPipeWriter(PipeWriter.Create(objects));
        objectWriter = new PdfObjectWriter(objectStreamWriter);
    }

    public bool TryAddRef(PdfIndirectObject obj)
    {
        if (!IsLegalWrite(obj, obj.DirectValueAsync().GetAwaiter().GetResult())) return false;
        members.Add(obj);
        return true;
    }

    private async ValueTask TryAddRefAsync(PdfIndirectObject obj)
    {
        var direcetValue = await obj.DirectValueAsync();
        count++;
        WriteObjectPosition(obj);
        await WriteObject(direcetValue);
    }

    private bool IsLegalWrite(PdfIndirectObject pdfIndirectObject, PdfObject direcetValue) => 
        pdfIndirectObject.GenerationNumber == 0 && direcetValue is not PdfStream;


    private void WriteObjectPosition(PdfIndirectObject item)
    {
        IntegerWriter.Write(referenceStreamWriter, item.ObjectNumber);
        referenceStreamWriter.WriteSpace();
        IntegerWriter.Write(referenceStreamWriter, objectStreamWriter.BytesWritten);
        referenceStreamWriter.WriteSpace();
    }

    private async ValueTask WriteObject(PdfObject directValue)
    {
        await directValue.Visit(objectWriter);
        objectStreamWriter.WriteLineFeed();
    }
#warning need to clean up this class --> move fields into a method
    public async ValueTask<PdfStream> CreateStream(DictionaryBuilder builder)
    {
        foreach (var member in members)
        {
            await TryAddRefAsync(member);
        }
        await referenceStreamWriter.FlushAsync();
        await objectStreamWriter.FlushAsync();
        return builder
            .WithItem(KnownNames.Type, KnownNames.ObjStm)
            .WithItem(KnownNames.N, count)
            .WithItem(KnownNames.First, referenceStreamWriter.BytesWritten)
            .AsStream(FinalStreamContent());
    }

    private ConcatStream FinalStreamContent()
    {
        return new ConcatStream(refs.CreateReader(), objects.CreateReader());
    }
}