using System.Collections.Generic;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.LowLevel.Writers.Builder;

public class ObjectStreamBuilder
{
    private readonly List<PdfIndirectObject> members = new();
    
    public bool TryAddRef(PdfIndirectObject obj)
    {
        if (!(obj.TryGetDirectValue(out var directValue) && IsLegalWrite(obj, directValue))) return false;
        if (!members.Contains(obj)) members.Add(obj);
        return true;
    }
    private bool IsLegalWrite(PdfIndirectObject pdfIndirectObject, PdfObject direcetValue) => 
        pdfIndirectObject.GenerationNumber == 0 && direcetValue is not PdfStream;

    public async ValueTask<PdfObject> CreateStream(DictionaryBuilder builder)
    {
        var writer = new ObjectStreamWriter();
        foreach (var member in members)
        {
            await  writer.TryAddRefAsync(member).CA();
        }
        return await writer.Build(builder, members.Count).CA();
    }
}