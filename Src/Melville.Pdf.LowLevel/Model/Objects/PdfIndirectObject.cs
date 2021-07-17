using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Visitors;

namespace Melville.Pdf.LowLevel.Model.Objects
{
    internal interface ICanSetIndirectTarget
    {
        void SetValue(PdfObject value);
        void SetValue(Func<ValueTask<PdfObject>> valueGetter);
    }
    public class PdfIndirectObject: PdfObject, ICanSetIndirectTarget
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

        public PdfIndirectObject(int objectNumber, int generationNumber) : 
            this(objectNumber, generationNumber, PdfTokenValues.Null)
        {
        }

        public PdfIndirectObject(int objectNumber, int generationNumber, PdfObject value)
        {
            ObjectNumber = objectNumber;
            GenerationNumber = generationNumber;
            ((ICanSetIndirectTarget)this).SetValue(value);
        }

        // users of the library should not be able to mutate the target value, but the
        // parser needs to because the indirect value may be created by a forward reference
        // long before the value is actually know.  Am internal interface lets only this module in.
        void ICanSetIndirectTarget.SetValue(PdfObject value)
        {
            this.value = value;
            accessor = null;
        }

        void ICanSetIndirectTarget.SetValue(Func<ValueTask<PdfObject>> valueGetter)
        {
            accessor = valueGetter;
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