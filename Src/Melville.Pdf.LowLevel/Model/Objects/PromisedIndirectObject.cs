using Melville.CSJ2K.j2k.wavelet.analysis;

namespace Melville.Pdf.LowLevel.Model.Objects;

/// <summary>
/// PromisedIndirectObject is a PdfIndirectObject that does not yet have a value.
/// This class gets used in PDF generation to generate object cycles, we need to
/// have a referencable value before we can construct the object.
/// </summary>
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
    /// <param name="value">The vew value of the object</param>
    public void SetValue(PdfObject value)
    {
        this.value = value;
    }

    /// <summary>
    /// Accept the promised value, if this object does not already have avalue.
    /// </summary>
    /// <param name="value">The vew value of the object</param>
    public void SetIfHasNoValue(PdfObject value)
    {
        if (!HasValue()) SetValue(value);
    }

    internal override bool HasValue() => value != UnsetValueSentinel;
    private static PdfTokenValues UnsetValueSentinel => PdfTokenValues.ArrayTerminator;
}