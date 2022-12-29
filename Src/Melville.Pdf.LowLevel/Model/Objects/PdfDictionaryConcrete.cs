using Melville.INPC;
using Melville.Pdf.LowLevel.Visitors;
namespace Melville.Pdf.LowLevel.Model.Objects;

[FromConstructor]
internal sealed partial class PdfDictionaryConcrete : PdfDictionary
{
    //The PdfObject hierarchy is intended to be closed to implementers outside of Melville.Pdf.LowLevel.
    //PdfDictionary needs to not be sealed because streams are dictionaries.  By having all plain dictionaries be this
    // internal class, we prevent users from descending from PdfDictionary because they cannot implement the internal Visit method.

    internal override T Visit<T>(ILowLevelVisitor<T> visitor) => visitor.Visit(this);
}