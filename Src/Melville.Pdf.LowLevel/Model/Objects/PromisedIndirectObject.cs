using Melville.CSJ2K.j2k.wavelet.analysis;

namespace Melville.Pdf.LowLevel.Model.Objects;

public sealed class PromisedIndirectObject : PdfIndirectObject
{
    internal PromisedIndirectObject(int objectNumber, int generationNumber) : 
        base(objectNumber, generationNumber, UnsetValueSentinel)
    {
    }
    /// <summary>
    /// Set the object referred to by this object.  This violates the rule that PdfObjects are immutable, but is needed for a lot of PDF generation
    /// scenerios to work.  This should be used sparingly on objects that have not yet been shared widely on consumers that may depend on their immutability.
    /// </summary>
    /// <param name="value"></param>
    public void SetValue(PdfObject value)
    {
        this.value = value;
    }

    public void SetIfHasNoValue(PdfObject value)
    {
        if (!HasValue()) SetValue(value);
    }

    private bool HasValue() => value != UnsetValueSentinel;
    private static PdfTokenValues UnsetValueSentinel => PdfTokenValues.ArrayTerminator;
}