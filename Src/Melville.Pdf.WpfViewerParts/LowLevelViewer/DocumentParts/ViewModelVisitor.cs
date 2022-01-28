using System.Globalization;
using System.Net.NetworkInformation;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Visitors;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.WpfViewerParts.LowLevelViewer.DocumentParts.Fonts;
using Melville.Pdf.WpfViewerParts.LowLevelViewer.DocumentParts.Pages;
using Melville.Pdf.WpfViewerParts.LowLevelViewer.DocumentParts.Streams;

namespace Melville.Pdf.WpfViewerParts.LowLevelViewer.DocumentParts;

public class ViewModelVisitor : ILowLevelVisitor<ValueTask<DocumentPart>>
{
    private string prefix = "";

    public ValueTask<DocumentPart> GeneratePart(string newPrefix, PdfObject item)
    {
        prefix = newPrefix;
        return item.Visit(this);
    }

    private ValueTask<DocumentPart> Terminal(string text)
    {
        var ret = new ValueTask<DocumentPart>(new DocumentPart(prefix + text));
        prefix = "";
        return ret;
    }

    public async ValueTask<DocumentPart> Visit(PdfArray item)
    {
        var title = prefix + "Array";
        var children = new DocumentPart[item.Count];
        for (int i = 0; i < children.Length; i++)
        {
            children[i] = await GeneratePart($"[{i}]: ", item.RawItems[i]);
        }

        return new DocumentPart(title, children);
    }

    public ValueTask<DocumentPart> Visit(PdfBoolean item) => Terminal(item.ToString());

    public async ValueTask<DocumentPart> Visit(PdfDictionary item)
    {
        //Notice there is an ordering dependency in these two declarationsn.  The second
        //line changes prefix;
        var savedPrefix = prefix ;
        var children = await ParseDictionaryChildren(item);

        var type = await item.GetOrDefaultAsync(KnownNames.Type, KnownNames.Type);

        return type.GetHashCode() switch
        {
            KnownNameKeys.Font => new FontPartViewModel(savedPrefix + "Font", item, children),
            KnownNameKeys.Page => new PagePartViewModel(savedPrefix+"Page", children, new PdfPage(item)),
            _ => new DocumentPart(savedPrefix + DictionaryTitle(type), children)
        };
    }

    private static string DictionaryTitle(PdfName type)
    {
        return (type == KnownNames.Type?"Dictionary":type.ToString());
    }

    private async Task<DocumentPart[]> ParseDictionaryChildren(PdfDictionary item)
    {
        var children = new DocumentPart[item.Count];
        var next = 0;
        foreach (var child in item.RawItems)
        {
            children[next++] = await GenerateDictionaryItem(child);
        }

        return children;
    }

    private ValueTask<DocumentPart> GenerateDictionaryItem(KeyValuePair<PdfName, PdfObject> item) => 
        GeneratePart($"{item.Key}: ", item.Value);

    public ValueTask<DocumentPart> Visit(PdfTokenValues item) => Terminal(item.ToString());

    public async ValueTask<DocumentPart> Visit(PdfIndirectObject item)
    {
        prefix = $"{prefix}{item.ObjectNumber} {item.GenerationNumber}: ";
        return await (await item.DirectValueAsync()).Visit(this);
    }

    public ValueTask<DocumentPart> Visit(PdfIndirectReference item) => 
        Terminal ($"{item.Target.ObjectNumber} {item.Target.GenerationNumber} R");

    public ValueTask<DocumentPart> Visit(PdfName item) => Terminal(item.ToString());

    public ValueTask<DocumentPart> Visit(PdfInteger item) => Terminal(item.IntValue.ToString());

    public ValueTask<DocumentPart> Visit(PdfDouble item) => 
        Terminal(item.DoubleValue.ToString(CultureInfo.CurrentUICulture));

    public ValueTask<DocumentPart> Visit(PdfString item)
    {
        var stringDocumentPart = new StringDocumentPart(item, prefix);
        prefix = "";
        return new(stringDocumentPart);
    }

    public async ValueTask<DocumentPart> Visit(PdfStream item)
    {
        var title = prefix;
        var children = await ParseDictionaryChildren(item);
        if (item.TryGetValue(KnownNames.Subtype, out var stTask) && (await stTask) == KnownNames.Image)
            return new ImagePartViewModel(title + "Image Stream", children, item);
        return new StreamPartViewModel(title + "Stream", children, item);
    }

    public ValueTask<DocumentPart> Visit(PdfFreeListObject item) =>
        Terminal($"Deleted Slot. Next: " + item.NextItem);
}