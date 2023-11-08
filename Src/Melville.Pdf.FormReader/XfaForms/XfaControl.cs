using System.Xml.Linq;
using Melville.INPC;
using Melville.Pdf.FormReader.Interface;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Objects.StringEncodings;

namespace Melville.Pdf.FormReader.XfaForms;

internal interface IXfaControl
{
    void WriteValues(XElement dataSet);
    ReadOnlySpan<char> LocalName { get; }
    XElement GetSingleDataItems();
}

internal partial class XfaControl : IPdfFormField, IXfaControl
{
    [FromConstructor] [DelegateTo] protected readonly IPdfFormField backingField;
    public virtual void WriteValues(XElement dataSet)
    {
        backingField.Value = PdfDirectObject.CreateUtf8String(dataSet.InnerText());
    }

    public ReadOnlySpan<char> LocalName => NameSplitter.LocalName(Name);

    public XElement GetSingleDataItems() => 
        new(LocalName.ToString(), XmlValueContent());

    protected virtual string XmlValueContent() => 
        Value.TryGet(out PdfDictionary? _)?"": Value.DecodedString();
}