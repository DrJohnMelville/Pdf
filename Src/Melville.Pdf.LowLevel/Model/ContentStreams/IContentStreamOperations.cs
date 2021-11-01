using System;
using System.Runtime.InteropServices;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Model.ContentStreams;

public static class ContentStreamExtendedOperations
{
    public static void SetLineDashPattern(
        this IStateChangingCSOperations target, double dashPhase = 0, params double[] dashArray) =>
        target.SetLineDashPattern(dashPhase, dashArray.AsSpan());
    public static PdfName LoadGraphicStateDictionary(
        this IStateChangingCSOperations target, string dictName)
    {
        var name = NameDirectory.Get(dictName);
        target.LoadGraphicStateDictionary(name);
        return name;
    }
}

public interface IDrawingCSOperations
{
    
}

public interface IContentStreamOperations: IStateChangingCSOperations, IDrawingCSOperations
{
    
}