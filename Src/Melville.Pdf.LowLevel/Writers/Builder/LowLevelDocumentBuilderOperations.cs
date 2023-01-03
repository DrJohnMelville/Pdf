using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Writers.Builder;

public static class LowLevelDocumentBuilderOperations
{
    /// <summary>
    /// Add a root element to an IPdfObjectRegistry
    /// </summary>
    /// <param name="creator">IPdfObjectRegistry to add the root element to.</param>
    /// <param name="rootElt"></param>
    public static void AddRootElement(
        this IPdfObjectRegistry creator, PdfDictionary rootElt) =>
        creator.AddToTrailerDictionary(KnownNames.Root, creator.Add(rootElt));
}