using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Writers.Builder;

/// <summary>
/// Add a root element to a Pdf Document Registry.
/// </summary>
public static class LowLevelDocumentBuilderOperations
{
    /// <summary>
    /// Add a root element to an IPdfObjectRegistry
    /// </summary>
    /// <param name="creator">IPdfObjectRegistry to add the root element to.</param>
    /// <param name="rootElt"></param>
    public static void AddRootElement(
        this IPdfObjectCreatorRegistry creator, PdfValueDictionary rootElt) =>
        creator.AddToTrailerDictionary(KnownNames.RootTName, creator.Add(rootElt));
}