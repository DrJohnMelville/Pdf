using System.Runtime.InteropServices;
using Melville.INPC;

namespace Melville.Pdf.LowLevel.Model.ContentStreams;

public interface IContentStreamOperations: 
    IStateChangingCSOperations, IDrawingCSOperations, IColorCSOperations, 
    ITextObjectOperations, ITextBlockOperations, IMarkedContentCSOperations
{
}