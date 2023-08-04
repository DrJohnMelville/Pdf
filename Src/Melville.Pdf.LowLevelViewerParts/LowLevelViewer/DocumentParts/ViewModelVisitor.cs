using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevelViewerParts.LowLevelViewer.DocumentParts.Fonts;
using Melville.Pdf.LowLevelViewerParts.LowLevelViewer.DocumentParts.Pages;
using Melville.Pdf.LowLevelViewerParts.LowLevelViewer.DocumentParts.References;
using Melville.Pdf.LowLevelViewerParts.LowLevelViewer.DocumentParts.Streams;
using Melville.Pdf.Model.Documents;

namespace Melville.Pdf.LowLevelViewerParts.LowLevelViewer.DocumentParts;

public class ViewModelVisitor
{
    private string prefix = "";
    private string ConsumePrefix()
    {
        var ret = prefix;
        prefix = "";
        return ret;
    }

    public DocumentPart GeneratePart(string prefix, PdfIndirectObject dictionary)
    {
        this.prefix = prefix;
        return GenerateForObject(dictionary);
    }

    private DocumentPart GenerateForObject(PdfIndirectObject indir) =>
        indir.TryGetEmbeddedDirectValue(out var dirValue)
            ? GenerateForObject(dirValue)
            : RenderReference(indir.GetObjectReference());


    private DocumentPart GenerateForObject(PdfDirectObject directValue)
    {
        return directValue switch
        {
            {IsString: true} => TerminalNode($"({directValue})"),
            {IsName: true} => TerminalNode($"/{directValue}"),
            var x when x.TryGet(out PdfArray? array) => ParseArray(array),
            var x when x.TryGet(out PdfStream? stream) => ParseStream(stream),
            var x when x.TryGet(out PdfDictionary? dictionary) => ParseDictionary(dictionary),
            _ => TerminalNode(directValue.ToString())
        };
    }

    private DocumentPart ParseStream(PdfStream stream)
    {
        var localPrefix = ConsumePrefix();
        var items = ParseDictionaryFields(stream, out var nodeType, out var subType);
        return SelectSpecialStream(localPrefix, nodeType, subType, stream, items);
    }

    private DocumentPart SelectSpecialStream(
        string localPrefix, PdfDirectObject nodeType, PdfDirectObject nodeSubType, PdfStream stream, 
        DocumentPart[] items)
    {
        if (nodeSubType.Equals(KnownNames.Image))
            return new ImagePartViewModel($"{localPrefix}Image Stream", items, stream);
        if (nodeType.Equals(KnownNames.XRef))
            return new XrefPartViewModel($"{localPrefix}Xref Stream", items, stream);
        return new StreamPartViewModel($"{localPrefix}Stream", items, stream);
    }

    private DocumentPart ParseDictionary(PdfDictionary dictionary)
    {
        var localPrefix = ConsumePrefix();
        var items = ParseDictionaryFields(dictionary, out var nodeType, out _);
        return SelectSpecialDictionaryView(dictionary, nodeType, localPrefix, items);
    }

    private static DocumentPart SelectSpecialDictionaryView(
        PdfDictionary dictionary, PdfDirectObject nodeType, string localPrefix, DocumentPart[] items) =>
        nodeType switch
        {
            var x when x.Equals(KnownNames.Font) =>
                new FontPartViewModel(localPrefix + "Font", dictionary, items),
            var x when x.Equals(KnownNames.Page) =>
                new PagePartViewModel(localPrefix + "Page", items, new PdfPage(dictionary)),
            _ => new DocumentPart($"{localPrefix}Dictionary", items)
        };

    private DocumentPart[] ParseDictionaryFields(PdfDictionary dictionary, out PdfDirectObject nodeType,
        out PdfDirectObject nodeSubType)
    {
        var items = new DocumentPart[dictionary.Count];
        int position = 0;
        nodeType = default;
        nodeSubType = default;
        foreach (var item in dictionary.RawItems)
        {
            CheckForType(item, KnownNames.Type, ref nodeType);
            CheckForType(item, KnownNames.Subtype, ref nodeSubType);
            CheckForType(item, KnownNames.S, ref nodeSubType);
            items[position++] = GeneratePart($"/{item.Key}: ", item.Value);
        }

        return items;
    }

    public void CheckForType(
        in KeyValuePair<PdfDirectObject, PdfIndirectObject> item, PdfDirectObject name,
        ref PdfDirectObject value)
    {
        if (item.Key.Equals(name) &&
            item.Value.TryGetEmbeddedDirectValue(out var typeValue))
            value = typeValue;
    }

    private DocumentPart ParseArray(PdfArray value)
    {
        var localPrefix = ConsumePrefix();
        var items = new DocumentPart[value.Count];
        for (int i = 0; i < items.Length; i++) 
            items[i] = GeneratePart($"[{i}]: ", value.RawItems[i]);
        return new DocumentPart($"{localPrefix}Array", items);
    }

    private DocumentPart TerminalNode(string text) => new(ConsumePrefix() + text);

    private DocumentPart RenderReference((int ObjectNumber, int Generation) item) => 
        new ReferencePartViewModel(ConsumePrefix(), item.ObjectNumber, item.Generation);

    /*
        prefix = newPrefix;
        return item.InvokeVisitor(this);
    }

    private ValueTask<DocumentPart> TerminalAsync(string text) => 
        new ValueTask<DocumentPart>(new DocumentPart(ConsumePrefix() + text));

    public async ValueTask<DocumentPart> Visit(PdfDictionary item)
    {
        //Notice there is an ordering dependency in these two declarationsn.  The second
        //line changes prefix;
        var savedPrefix = ConsumePrefix() ;
        var children = await ParseDictionaryChildrenAsync(item);

        var type = await item.GetOrDefaultAsync(KnownNames.Type, KnownNames.Type);

        return type.GetHashCode() switch
        {
            KnownNames.Font => new FontPartViewModel(savedPrefix + "Font", item, children),
            KnownNames.Page => new PagePartViewModel(savedPrefix+"Page", children, new PdfPage(item)),
            _ => new DocumentPart(savedPrefix + DictionaryTitle(type), children)
        };
    }

    private static string DictionaryTitle(PdfName type) => 
        (type == KnownNames.Type?"Dictionary":type.ToString())??"Dictionary";

    private async Task<DocumentPart[]> ParseDictionaryChildrenAsync(PdfDictionary item)
    {
        var children = new DocumentPart[item.Count];
        var next = 0;
        foreach (var child in item.RawItems)
        {
            children[next++] = await GenerateDictionaryItemAsync(child);
        }

        return children;
    }

    private ValueTask<DocumentPart> GenerateDictionaryItemAsync(KeyValuePair<PdfName, PdfObject> item) => 
        GeneratePartAsync($"{item.Key}: ", item.Value);

    public ValueTask<DocumentPart> Visit(PdfIndirectObject item) =>
        new(new ReferencePartViewModel(ConsumePrefix(), item.ObjectNumber, item.GenerationNumber));

    public ValueTask<DocumentPart> Visit(PdfName item) => TerminalAsync(item.ToString() ?? "<Null>");

    public async ValueTask<DocumentPart> Visit(PdfStream item)
    {
        var title = ConsumePrefix();
        var children = await ParseDictionaryChildrenAsync(item);
        if (item.SubTypeOrNull() == KnownNames.Image)
            return new ImagePartViewModel(title + "Image Stream", children, item);
        if ((await item.GetOrDefaultAsync(KnownNames.Type, KnownNames.None).CA()) == KnownNames.XRef)
            return new XrefPartViewModel(title + "XRef Stream", children, item);
        return new StreamPartViewModel(title + "Stream", children, item);
    }
    */
}