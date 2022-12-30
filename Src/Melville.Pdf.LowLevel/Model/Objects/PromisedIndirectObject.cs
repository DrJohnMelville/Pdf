namespace Melville.Pdf.LowLevel.Model.Objects;

public sealed class PromisedIndirectObject : PdfIndirectObject
{
    public PromisedIndirectObject(int objectNumber, int generationNumber) : 
        base(objectNumber, generationNumber, PdfTokenValues.ArrayTerminator)
    {
    }
    public void SetValue(PdfObject value)
    {
        this.value = value;
    }

    public bool HasRegisteredAccessor() => value != PdfTokenValues.ArrayTerminator;
}