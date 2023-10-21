using Melville.INPC;
using Melville.Pdf.LowLevel.Model.Conventions;

namespace Melville.Pdf.FormReader.AcroForms
{
    [FromConstructor]
    internal partial class AcroCheckBox : AcroFormField, IPdfCheckBox
    {
        public bool IsChecked
        {
            get => Value.Equals(KnownNames.Yes);
            set => Value = value ? KnownNames.Yes : KnownNames.OFF;
        }
    }
}