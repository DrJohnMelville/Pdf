using System.Collections.Generic;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects2;

namespace Melville.Pdf.LowLevel.Writers.Builder;

internal class ObjectStreamBuilder
{
    private readonly ValueDictionaryBuilder streamDictionaryItems;
    private readonly Dictionary<int,PdfDirectValue> members = new();

    public ObjectStreamBuilder(ValueDictionaryBuilder? streamDictionaryItems = null)
    {
        this.streamDictionaryItems = streamDictionaryItems ?? DefaultBuilder();
    }

    private ValueDictionaryBuilder DefaultBuilder() => new ValueDictionaryBuilder()
        .WithItem(KnownNames.FilterTName, KnownNames.FlateDecodeTName);

    public bool TryAddRef(int number, PdfDirectValue obj)
    {
        if (!IsLegalWrite(obj)) return false;
        members[number] = obj;
        return true;
    }
    private bool IsLegalWrite(PdfDirectValue value) => ! value.TryGet(out PdfValueStream _);

    public async ValueTask<PdfDirectValue> CreateStreamAsync()
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