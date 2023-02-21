using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Visitors;

namespace Melville.Pdf.LowLevel.Model.Objects;

/// <summary>
/// This class represents a PDF object reference in PDF.  Users should rarely see this class unless they specifically request raw items.
/// </summary>
public class PdfIndirectObject: PdfObject
{
    /// <summary>
    /// The Object Number for this Reference, as defined in the PDF Spec.
    /// </summary>
    public int ObjectNumber { get; }
    /// <summary>
    /// The generation number, as defined in the PDF spec.
    /// </summary>
    public int GenerationNumber { get; }
    /// <summary>
    /// The value of the indirect object, if known.
    /// </summary>
    protected PdfObject value;

    internal PdfIndirectObject(int objectNumber, int generationNumber, PdfObject value)
    {
        ObjectNumber = objectNumber;
        GenerationNumber = generationNumber;
        this.value = value;
    }

    /// <inheritdoc />
    public override async ValueTask<PdfObject> DirectValueAsync() => 
        value = await value.DirectValueAsync().CA();

    internal virtual bool TryGetDirectValue(out PdfObject result)
    {
        result = value;
        return true;
    }

    internal override T Visit<T>(ILowLevelVisitor<T> visitor) => visitor.Visit(this);

    internal virtual bool HasValue() => true;
}