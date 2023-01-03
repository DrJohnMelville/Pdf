using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Writers.Builder;

public static class LowLevelDocumentBuilderOperations
{
    /// <summary>
    /// Add a root element to an ILowLevelDocumentBuilder
    /// </summary>
    /// <param name="creator">ILowLevelDocumentBuilder to add the root element to.</param>
    /// <param name="rootElt"></param>
    public static void AddRootElement(
        this ILowLevelDocumentBuilder creator, PdfDictionary rootElt) =>
        creator.AddToTrailerDictionary(KnownNames.Root, creator.Add(rootElt));
}