namespace Melville.Pdf.LowLevel.Model.Objects
{
    internal interface ICanSetIndirectTarget
    {
        void SetValue(PdfObject value);
    }
    public class PdfIndirectObject: PdfObject, ICanSetIndirectTarget
    {
        public int ObjectNumber { get; }
        public int GenerationNumber { get; }
        public PdfObject Value { get; private set; } = PdfEmptyConstants.Null;

        public PdfIndirectObject(int objectNumber, int generationNumber) : 
            this(objectNumber, generationNumber, PdfEmptyConstants.Null)
        {
        }

        public PdfIndirectObject(int objectNumber, int generationNumber, PdfObject value)
        {
            ObjectNumber = objectNumber;
            GenerationNumber = generationNumber;
            Value = value;
        }

        // users of the library should not be able to mutate the target value, but the
        //parser needs to because the indirect value may be created by a forward reference
        // long before the value is actually know.  Am internal interface lets only this module in.
        void ICanSetIndirectTarget.SetValue(PdfObject value) => Value = value;

        public override PdfObject DirectValue() => Value.DirectValue();
    }
}