using System.Diagnostics.CodeAnalysis;
using Melville.Pdf.LowLevel.Model.Conventions;

namespace Melville.Pdf.LowLevel.Model.Objects;

public static class DictionarySubtypeOperations
{
    /// <summary>
    /// Try to get the subtype of a dictionary.
    /// </summary>
    /// <param name="dict">The dictionary to check</param>
    /// <param name="result">If the operation succeds, this parameter receives the subtype, as a PDF Name.</param>
    /// <returns>True if a subtype is found, false otherwise.</returns>
    public static bool TryGetSubType(
        this PdfDictionary dict, [NotNullWhen(true)]out PdfName? result)
    {
        result = dict.SubTypeOrNull();
        return result is not null;
    }

    //Pdf Spec section 7.3.7 specifies that types and subtypes are always Names so I do not need async
    /// <summary>
    /// Get the PDfName that is the subtype of a dictionary
    /// </summary>
    /// <param name="dict">The dictionary to check</param>
    /// <returns>The subtyppe as a PdfName, or null if the subtype does not exist or is not a PDF name.</returns>
    public static PdfName? SubTypeOrNull(this PdfDictionary dict) =>
        (dict.RawItems.TryGetValue(KnownNames.Subtype, out var obj) ||
         dict.RawItems.TryGetValue(KnownNames.S, out obj)) && 
        obj is PdfName name? name:null;
    
}