using System.Collections.Generic;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Writers.Builder;

internal class ObjectStreamBuilder
{
    private readonly DictionaryBuilder streamDictionaryItems;
    private readonly Dictionary<int,PdfDirectObject> members = new();

    public ObjectStreamBuilder(DictionaryBuilder? streamDictionaryItems = null)
    {
        this.streamDictionaryItems = streamDictionaryItems ?? DefaultBuilder();
    }

    private DictionaryBuilder DefaultBuilder() => new DictionaryBuilder()
        .WithItem(KnownNames.Filter, KnownNames.FlateDecode);

    public bool TryAddRef(int number, PdfDirectObject obj)
    {
        if (!IsLegalWrite(obj)) return false;
        members[number] = obj;
        return true;
    }
    private bool IsLegalWrite(PdfDirectObject value) => ! value.TryGet(out PdfStream? _);

    public async ValueTask<PdfDirectObject> CreateStreamAsync()
    {
        var writer = new ObjectStreamWriter();
        foreach (var member in members)
        {
            await  writer.TryAddRefAsync(member.Key, member.Value).CA();
        }
        return await writer.BuildAsync(streamDictionaryItems, members.Count).CA();
    }

    public bool HasValues() => members.Count > 0;
}