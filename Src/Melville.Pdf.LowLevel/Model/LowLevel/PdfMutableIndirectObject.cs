using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Model.LowLevel
{
    public partial class LowLevelDocumentBuilder : ILowLevelDocumentBuilder
    {

        private class PdfMutableIndirectObject : PdfIndirectObject
        {
            private PdfObject assignedValue;

            public PdfMutableIndirectObject(int objectNumber, int generationNumber, PdfObject value) :
                base(objectNumber, generationNumber, value)
            {
                assignedValue = value;
            }

            public void SetValue(PdfObject value)
            {
                assignedValue = value;
            }

            public override ValueTask<PdfObject> DirectValue() => new(assignedValue);
        }
    }
}