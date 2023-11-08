using System.Xml.Linq;
using Melville.INPC;
using Melville.Pdf.FormReader.Interface;
using Melville.Pdf.LowLevel.Model.Conventions;

namespace Melville.Pdf.FormReader.XfaForms;

[FromConstructor]
internal partial class XfaCheckBox : XfaControl, IPdfCheckBox
{
    public bool IsChecked
    {
        get => Value.Equals(KnownNames.Yes);
        set => Value = value ? KnownNames.Yes : KnownNames.OFF;
    }

    public override void WriteValues(XElement dataSet) =>
        backingField.Value = dataSet.InnerText().Equals("0", StringComparison.OrdinalIgnoreCase)
            ? KnownNames.OFF
            : KnownNames.Yes;

    protected override string XmlValueContent() => IsChecked ? "-1" : "0";
}