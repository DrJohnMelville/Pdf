using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Visitors;

namespace Melville.Pdf.LowLevel.Model.Objects
{
    public class PdfIndirectObject: PdfObject
    {
        public int ObjectNumber { get; }
        public int GenerationNumber { get; }
        private Func<ValueTask<PdfObject>>? accessor = null;
        private PdfObject value = PdfTokenValues.Null;


        public PdfIndirectObject(
            int objectNumber, int generationNumber, Func<ValueTask<PdfObject>>? accessor)
        {
            this.accessor = accessor;
            ObjectNumber = objectNumber;
            GenerationNumber = generationNumber;
        }

        public PdfIndirectObject(int objectNumber, int generationNumber, PdfObject value)
        {
            ObjectNumber = objectNumber;
            GenerationNumber = generationNumber;
            this.value = value;
        }
        
        public override async ValueTask<PdfObject> DirectValue()
        {
            if (accessor != null)
            {
                value = await accessor();
                accessor = null;
            } 
            return value;
        }
        public override T Visit<T>(ILowLevelVisitor<T> visitor) => visitor.Visit(this);
    }
}