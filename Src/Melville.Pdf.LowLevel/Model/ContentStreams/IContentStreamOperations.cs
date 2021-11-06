using System.Runtime.InteropServices;
using Melville.INPC;

namespace Melville.Pdf.LowLevel.Model.ContentStreams;

public interface ICompatibilityOperation
{
    // void BeginCompatibilitySection();
    // void EndCompatibilitySection();
}
public interface IContentStreamOperations: 
    IStateChangingOperations, IDrawingOperations, IColorOperations, 
    ITextObjectOperations, ITextBlockOperations, IMarkedContentCSOperations, ICompatibilityOperation
{
}