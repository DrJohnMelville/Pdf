using System.Runtime.InteropServices.ComTypes;
using System.Xml;
using System.Xml.Linq;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.FormReader.Interface;
using Melville.Pdf.FormReader.XfaForms;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Document;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Objects.StringEncodings;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.FormReader.AcroForms;

internal class AcroFieldFactory
{
    private readonly List<IPdfFormField> pdfControls = new();
    private readonly XfaSubForm xfaForm = new("");
    private PdfIndirectObject dataset = PdfDirectObject.CreateNull();

    public AcroFieldFactory()
    {
    }

    public ValueTask ParseAcroFieldsAsync(PdfArray fields) =>
        ParseFieldsAsync(fields.RawItems, "");

    public ValueTask ParseXfaFieldsAsync(PdfArray fields) =>
        ParseXfaAsync(fields.RawItems);

    public IPdfForm Build(PdfLowLevelDocument doc, in PdfDirectObject formDefaultAppearance) =>
        new AcroPdfForm(doc, pdfControls, formDefaultAppearance, dataset, xfaForm);

    public async ValueTask ParseFieldsAsync(
        IReadOnlyList<PdfIndirectObject> fieldReferences, 
        string namePrefix)
    {
        foreach (var reference in fieldReferences)
        {
            var field = await reference.LoadValueAsync().CA();
            if (field.TryGet(out PdfDictionary? dict))
            {
                await ParseSingleFieldAsync(namePrefix, dict, reference).CA();
            }
        }
    }

    private async ValueTask ParseSingleFieldAsync(string namePrefx, PdfDictionary dict, PdfIndirectObject reference)
    {
        var name = (await dict[KnownNames.T].CA()).DecodedString();
        if (namePrefx.Length > 0)
            name = $"{namePrefx}.{name}";
        if ((await dict.GetOrNullAsync(KnownNames.FT).CA()) is { IsNull: false} type)
        {
            var value = await dict.GetOrDefaultAsync(KnownNames.V).CA();
            var flags = (AcroFieldFlags)(await dict.GetOrDefaultAsync(KnownNames.Ff, 0).CA());

            await new FieldBuilder(name, value, type, flags, dict, reference, pdfControls)
                .CreateAsync().CA();
            return;
        }
        await ReadSubFormAsync(name, dict).CA();
    }

    private async ValueTask ReadSubFormAsync(string name, PdfDictionary dict)
    {
        var kids = await dict.GetOrDefaultAsync(KnownNames.Kids, PdfArray.Empty).CA();
        await ParseFieldsAsync(kids.RawItems, name).CA();
    }

    private async ValueTask ParseXfaAsync(IReadOnlyList<PdfIndirectObject> xfaArray)
    {
        int state = -1;
        PdfIndirectObject template = PdfDirectObject.CreateNull();
        foreach (var reference in xfaArray)
        {
            var item = await reference.LoadValueAsync().CA();
            switch (state)
            {
                case 1:
                    template = item;
                    state = -1;
                    break;
                case 2: 
                    dataset = reference;
                    state = -1;
                    break;
                default:
                    state = NextItem(item);
                    break;
            }
        }

        if (!(template.IsNull || dataset.IsNull))
            await ParseXfaFieldsAsync(template).CA();
    }

    private static readonly PdfDirectObject templateString = 
        PdfDirectObject.CreateString("template"u8);
    private static readonly PdfDirectObject datasetsString =
        PdfDirectObject.CreateString("datasets"u8);

    private int NextItem(in PdfDirectObject item) => item switch
    {
        _ when item.Equals(templateString) => 1,
        _ when item.Equals(datasetsString) => 2,
        _ => -1
    };

    private async ValueTask ParseXfaFieldsAsync(
        PdfIndirectObject template)
    {
        ParseXElement(xfaForm, await ReadXmlFromIndirectObjectAsync(template).CA());
        xfaForm.WriteValues((await ReadXmlFromIndirectObjectAsync(dataset).CA())
            .Elements().First());
    }

    private static async Task<XElement> ReadXmlFromIndirectObjectAsync(PdfIndirectObject template)
    {
        var reader = XmlReader.Create(await (await template.LoadValueAsync().CA())
                .Get<PdfStream>().StreamContentAsync().CA(), 
            new XmlReaderSettings(){Async = true});
        await reader.MoveToContentAsync().CA();
        return (XElement)await XNode.ReadFromAsync(reader, CancellationToken.None).CA();
    }

    public void ParseXElement(XfaSubForm subForm, XElement element)
    {
        foreach (var child in element.Elements())
        {
            ParseSingleElement(subForm, child);
        }
    }

    private void ParseSingleElement(XfaSubForm xfaSubForm, XElement child)
    {
        switch (child.Name.LocalName)
        {
            case "exclGroup" :
            case "field":
                ParseField(xfaSubForm, child);
                break;
            case "subform":
                ParseSubForm(xfaSubForm, child);
                break;
        }
    }

    private void ParseSubForm(XfaSubForm parent, XElement child)
    {
        var name = child.Attribute("name");
        if (name is not { Value: not null })
        {
            ParseXElement(parent, child);
            return;
        }
        var newNode = new XfaSubForm(parent.ChildControlName(name.Value));
        parent.AddControl(newNode);
        ParseXElement(newNode, child);
    }

    private void ParseField(XfaSubForm parent, XElement child)
    {
        var typeNode = child.Element("ui")?.Elements().First();
        var name = child.Attribute("name");
        if (name is not {Value: not null}) return;
        var fullName = parent.ChildControlName(name.Value);

        var index = pdfControls.NameToFieldIndex(fullName);

        if (index >= 0)
        {
            parent.AddControl(CreateXfaControl(child, pdfControls[index]));
        }
        else
        {
            var ctrl = CreateXfaControl(child,new NonMirroredXfaValue(fullName));
            if (ctrl is not null) pdfControls.Add(ctrl);
            parent.AddControl(ctrl);
        }
    }

    private static readonly XNamespace xfaTemplate =
        XNamespace.Get(@"http://www.xfa.org/schema/xfa-template/2.5/");

    private XfaControl? CreateXfaControl(XElement field, IPdfFormField dataStore)
    {
        if (field.Name.LocalName == "exclGroup") return ParseExclGroup(field, dataStore);
        return XfaControlType(field) switch
        {
            "choiceList" => ParsePick(field, dataStore),
            "checkButton" => new XfaCheckBox(dataStore),
            "button" => null,
            "signature" => null,
            "textEdit" => new XfaTextBox(dataStore),
            "dateTimeEdit" => new XfaTextBox(dataStore),
            _ => new XfaTextBox(dataStore)
        };
    }

    private XfaControl ParseExclGroup(XElement field, IPdfFormField dataStore) =>
        new XfaSinglePick(dataStore, 
            field.Descendants(xfaTemplate+"field").Select(ParseExclField).ToList());

    private PdfPickOption ParseExclField(XElement field) => 
        new(ReadExclCaption(field), ReadExclValue(field));


    private static string ReadExclCaption(XElement field) =>
        ReadTypedValue(
            field.Element(xfaTemplate + "caption")?.Element(xfaTemplate + "value"));

    private static string ReadExclValue(XElement field) => ReadTypedValue(field.Element(xfaTemplate + "items"));

    private static string ReadTypedValue(XElement? typeNodeParent) =>
        typeNodeParent?.Elements().First()
            ?.InnerText() ?? throw new PdfParseException("Cannot get value");

    private static string XfaControlType(XElement field) =>
        field.Element(xfaTemplate+"ui")?
            .Elements().First()?.Name.LocalName ??
        throw new PdfParseException("Xfa control missing ui element");

    private XfaControl ParsePick(XElement field, IPdfFormField dataStore)
    {
        var items = field.Elements(xfaTemplate + "items").ToList();
        if (items.Count is not (1 or 2))
          //  throw new PdfParseException("Xfa choice list has invalid number of items elements./");
            return new XfaSinglePick(dataStore, Array.Empty<PdfPickOption>());

        var valueItems = SelectItemshild(items, false).Elements();
        var displalyItems = SelectItemshild(items, true).Elements();
        return new XfaSinglePick(dataStore,
            displalyItems.Zip(valueItems, (d, v) => new PdfPickOption(d.InnerText(),
                PdfDirectObject.CreateUtf8String(v.InnerText()))).ToList());
    }

    private static XElement SelectItemshild(List<XElement> items, bool getDisplayItems) =>
        items
            .Where(i => getDisplayItems ^ 
                        (i.Attribute("save")?.Value ?? "") == "1")
            .Concat(items)
            .First();
}
