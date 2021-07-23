﻿using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Writers.Builder
{
    public partial class LowLevelDocumentBuilder 
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