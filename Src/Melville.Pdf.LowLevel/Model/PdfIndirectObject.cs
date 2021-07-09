namespace Melville.Pdf.LowLevel.Model
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

        public PdfIndirectObject(int objectNumber, int generationNumber)
        {
            ObjectNumber = objectNumber;
            GenerationNumber = generationNumber;
        }

        // users of the library should not be able to mutate the target value, but the
        //parser needs to because the indirect value may be created by a forward reference
        // long before the value is actually know.  Am internal interface lets only this module in.
        void ICanSetIndirectTarget.SetValue(PdfObject value) => Value = value;
    }

    public class PdfIndirectReference : PdfObject
    {
        public PdfIndirectObject Target { get; }
        public PdfIndirectReference(PdfIndirectObject target)
        {
            Target = target;
        }
    }
}