using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Model.ContentStreams;

public interface IContentStreamOperations: 
    IStateChangingOperations, IDrawingOperations, IColorOperations, 
    ITextObjectOperations, ITextBlockOperations, IMarkedContentCSOperations, ICompatibilityOperations,
    IFontMetricsOperations
{
    /// <summary>
    /// Content stream operator gs
    /// </summary>
    ValueTask LoadGraphicStateDictionary(PdfName dictionaryName);
}