using System.Globalization;
using System.Net.NetworkInformation;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Objects2;
using Melville.Pdf.LowLevel.Visitors;
using Melville.Pdf.LowLevelViewerParts.LowLevelViewer.DocumentParts.Fonts;
using Melville.Pdf.LowLevelViewerParts.LowLevelViewer.DocumentParts.Pages;
using Melville.Pdf.LowLevelViewerParts.LowLevelViewer.DocumentParts.References;
using Melville.Pdf.LowLevelViewerParts.LowLevelViewer.DocumentParts.Streams;
using Melville.Pdf.Model.Documents;

namespace Melville.Pdf.LowLevelViewerParts.LowLevelViewer.DocumentParts;

public class ViewModelVisitor
{

    public ValueTask<DocumentPart> VisitTopLevelObject(PdfIndirectValue item)
    {
        return new(new DocumentPart("Need to Implement the object printer."));
    }
    public ValueTask<DocumentPart> GeneratePartAsync(string trailer, PdfValueDictionary lowlevelTrailerDictionary)
    {
        return new(new DocumentPart("Need to implement trailer dictionary printer"));
    }

    /*
    private string prefix = "";

    public ValueTask<DocumentPart> GeneratePartAsync(string newPrefix, PdfValueDictionary item)
    {
        prefix = newPrefix;
        return item.InvokeVisitor(this);
    }

    private string ConsumePrefix()
    {
        var ret = prefix;
        prefix = "";
        return ret;
    }

    private ValueTask<DocumentPart> TerminalAsync(string text) => 
        new ValueTask<DocumentPart>(new DocumentPart(ConsumePrefix() + text));

    public async ValueTask<DocumentPart> Visit(PdfArray item)
    {
        var title = prefix + "Array";
        var children = new DocumentPart[item.Count];
        for (int i = 0; i < children.Length; i++)
        {
            children[i] = await GeneratePartAsync($"[{i}]: ", item.RawItems[i]);
        }

        return new DocumentPart(title, children);
    }

    public ValueTask<DocumentPart> Visit(PdfBoolean item) => TerminalAsync(item.ToString());

    public async ValueTask<DocumentPart> Visit(PdfDictionary item)
    {
        //Notice there is an ordering dependency in these two declarationsn.  The second
        //line changes prefix;
        var savedPrefix = ConsumePrefix() ;
        var children = await ParseDictionaryChildrenAsync(item);

        var type = await item.GetOrDefaultAsync(KnownNames.Type, KnownNames.Type);

        return type.GetHashCode() switch
        {
            KnownNameKeys.Font => new FontPartViewModel(savedPrefix + "Font", item, children),
            KnownNameKeys.Page => new PagePartViewModel(savedPrefix+"Page", children, new PdfPage(item)),
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

    public ValueTask<DocumentPart> Visit(PdfTokenValues item) => TerminalAsync(item.ToString());

    public async ValueTask<DocumentPart> VisitTopLevelObject(PdfIndirectObject item)
    {
        prefix = $"{prefix}{item.ObjectNumber} {item.GenerationNumber}: ";
        return (await (await item.DirectValueAsync()).InvokeVisitor(this))
            .WithTarget(item.ObjectNumber, item.GenerationNumber);
    }

    public ValueTask<DocumentPart> Visit(PdfIndirectObject item) =>
        new(new ReferencePartViewModel(ConsumePrefix(), item.ObjectNumber, item.GenerationNumber));

    public ValueTask<DocumentPart> Visit(PdfName item) => TerminalAsync(item.ToString() ?? "<Null>");

    public ValueTask<DocumentPart> Visit(PdfInteger item) => TerminalAsync(item.IntValue.ToString());

    public ValueTask<DocumentPart> Visit(PdfDouble item) => 
        TerminalAsync(item.DoubleValue.ToString(CultureInfo.CurrentUICulture));

    public ValueTask<DocumentPart> Visit(PdfString item) => 
        new(new StringDocumentPart(item, ConsumePrefix()));

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