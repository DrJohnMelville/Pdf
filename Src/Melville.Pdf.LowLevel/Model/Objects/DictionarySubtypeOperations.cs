using System.Diagnostics.CodeAnalysis;
using Melville.Pdf.LowLevel.Model.Conventions;

namespace Melville.Pdf.LowLevel.Model.Objects;

/// <summary>
/// Operations to get the subtype of a dictonary supportin the shortcuts specified in Pdf SPec 7.3.7
/// </summary>
public static class DictionarySubtypeOperations
{
    /// <summary>
    /// Try to get the subtype of a dictionary.
    /// </summary>
    /// <param name="dict">The dictionary to check</param>
    /// <param name="result">If the operation succeds, this parameter receives the subtype, as a PDF Name.</param>
    /// <returns>True if a subtype is found, false otherwise.</returns>
    public static bool TryGetSubType(
        this PdfDictionary dict, [NotNullWhen(true)]out PdfDirectObject result)
    {
        result = dict.SubTypeOrNull();
        return !result.IsNull;
    }

    //Pdf Spec section 7.3.7 specifies that types and subtypes are always direct Names so I do not need async
    /// <summary>
    /// Get the PDfName that is the subtype of a dictionary
    /// </summary>
    /// <param name="dict">The dictionary to check</param>
    /// <returns>The subtyppe as a PdfName, or null if the subtype does not exist or is not a PDF name.</returns>
    public static PdfDirectObject SubTypeOrNull(this PdfDictionary dict, PdfDirectObject defaultValue = default) =>
        (dict.RawItems.TryGetValue(KnownNames.Subtype, out var obj) ||
         dict.RawItems.TryGetValue(KnownNames.S, out obj)) &&
        obj.TryGetEmbeddedDirectValue(out var dirObj) &&
        dirObj.IsName
            ? dirObj
            : defaultValue;

}