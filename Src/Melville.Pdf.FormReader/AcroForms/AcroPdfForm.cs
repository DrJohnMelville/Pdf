using System.Text;
using System.Xml;
using System.Xml.Linq;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.Streams;
using Melville.Pdf.FormReader.Interface;
using Melville.Pdf.FormReader.XfaForms;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Document;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Writers;
using Melville.Pdf.LowLevel.Writers.Builder;

namespace Melville.Pdf.FormReader.AcroForms;

internal partial class AcroPdfForm : IPdfForm
{
    [FromConstructor] public readonly PdfLowLevelDocument document;
    [FromConstructor] public IReadOnlyList<IPdfFormField> Fields { get; }
    [FromConstructor] private readonly PdfDirectObject formAppearanceString;
    [FromConstructor] private readonly PdfIndirectObject xfaDataSet;
    [FromConstructor] private readonly XfaSubForm xfaForm;

    public async ValueTask<PdfLowLevelDocument> CreateModifiedDocumentAsync()
    {
        var ret = new ModifyableLowLevelDocument(document );
        await WriteChangedFieldsAsync(ret).CA();
        if (!xfaDataSet.IsNull)
        {
            WriteXfaDataset(ret);
        }
        return ret;
    }

    private async ValueTask WriteChangedFieldsAsync(ICanReplaceObjects target)
    {
        foreach (var field in Fields.OfType<AcroFormField>())
        {
            await field.WriteChangeToAsync(target, formAppearanceString).CA();
        }
    }

    private static readonly XNamespace DataNs = "http://www.xfa.org/schema/xfa-data/1.0/";

    private void WriteXfaDataset(ModifyableLowLevelDocument doc)
    {
        var targetXml = new XElement(DataNs + "datasets", 
            new XAttribute(XNamespace.Xmlns + "xfa", DataNs),
            new XElement(DataNs + "data", xfaForm.DataElements())
            );

        var mbs = new MultiBufferStream();
        var writer = XmlWriter.Create(mbs,
            new XmlWriterSettings()
            {
                CheckCharacters = true, 
                Encoding = new UTF8Encoding(false), 
                CloseOutput = false, 
                OmitXmlDeclaration=true
            });
        targetXml.WriteTo(writer);
        writer.Flush();

        doc.ReplaceReferenceObject(xfaDataSet, 
            new DictionaryBuilder()
                .WithItem(KnownNames.Type, KnownNames.EmbeddedFile)
                .WithFilter(FilterName.FlateDecode)
                .AsStream(mbs)
            );
    }
}