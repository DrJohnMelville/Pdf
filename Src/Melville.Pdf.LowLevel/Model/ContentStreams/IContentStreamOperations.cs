using System.Runtime.InteropServices;
using Melville.INPC;

namespace Melville.Pdf.LowLevel.Model.ContentStreams;

public interface IContentStreamOperations: 
    IStateChangingOperations, IDrawingOperations, IColorOperations, 
    ITextObjectOperations, ITextBlockOperations, IMarkedContentCSOperations, ICompatibilityOperations,
    IFontMetricsOperations
{
}