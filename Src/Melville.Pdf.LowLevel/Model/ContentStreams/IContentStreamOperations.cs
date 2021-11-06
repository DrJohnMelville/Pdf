using System.Runtime.InteropServices;
using Melville.INPC;

namespace Melville.Pdf.LowLevel.Model.ContentStreams;

public interface ICompatibilityOperations
{
    void BeginCompatibilitySection();
    void EndCompatibilitySection();
}
public interface IContentStreamOperationses: 
    IStateChangingOperations, IDrawingOperations, IColorOperations, 
    ITextObjectOperations, ITextBlockOperations, IMarkedContentCSOperations, ICompatibilityOperations
{
}