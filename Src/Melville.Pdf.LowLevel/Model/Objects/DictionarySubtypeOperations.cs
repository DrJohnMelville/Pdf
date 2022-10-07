using System.Diagnostics.CodeAnalysis;
using Melville.Pdf.LowLevel.Model.Conventions;

namespace Melville.Pdf.LowLevel.Model.Objects;

public static class DictionarySubtypeOperations
{
    public static bool TryGetSubType(
        this PdfDictionary dict, [NotNullWhen(true)]out PdfName? result)
    {
        result = dict.SubTypeOrNull();
        return result is not null;
    }

    //Pdf Spec section 7.3.7 specifies that types and subtypes are always Names so I do not need async
    public static PdfName? SubTypeOrNull(this PdfDictionary dict) =>
        (dict.RawItems.TryGetValue(KnownNames.Subtype, out var obj) ||
         dict.RawItems.TryGetValue(KnownNames.S, out obj)) && 
        obj is PdfName name? name:null;
    
}