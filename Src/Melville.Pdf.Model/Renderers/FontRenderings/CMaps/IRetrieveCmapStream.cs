using System.IO;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.Model.Renderers.FontRenderings.CMaps;

/// <summary>
/// This interface is used by the CMAP parser to find the CMAP text referenced by a usecmap operator
/// </summary>
public interface IRetrieveCmapStream
{
    /// <summary>
    /// Loodup a CMap by name and return a stream with the source code for that CMap.
    /// </summary>
    /// <param name="name">The name of the desired CMap</param>
    /// <returns>A stream containing the code for the CMao</returns>
    Stream? CMapStreamFor(PdfDirectObject name);
}