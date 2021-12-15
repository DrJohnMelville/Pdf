using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
// needed in rendering code.
using Melville.Pdf.Model.Renderers.Colors;

namespace Melville.Pdf.Model.Renderers.GraphicsStates;

public interface IGraphiscState : IStateChangingOperations
{
    ValueTask LoadGraphicStateDictionary(PdfDictionary dictionary);
    void SetStrokeColorSpace(IColorSpace colorSpace);
    void SetNonstrokeColorSpace(IColorSpace colorSpace);
}

public partial class GraphicsState: IGraphiscState
{
    [MacroItem("Matrix3x2", "TransformMatrix", "Matrix3x2.Identity")]
    [MacroItem("double", "LineWidth", "1.0")]
    [MacroItem("double", "MiterLimit", "10.0")]
    [MacroItem("LineJoinStyle", "LineJoinStyle", "LineJoinStyle.Miter")]
    [MacroItem("LineCap", "LineCap", "LineCap.Butt")]
    [MacroItem("double", "DashPhase", "0.0")]
    [MacroItem("double", "FlatnessTolerance", "0.0")]
    [MacroItem("double[]", "DashArray", "Array.Empty<double>()")]
    [MacroItem("RenderIntentName", "RenderIntent", "RenderIntentName.RelativeColoriMetric")]
    [MacroItem("IColorSpace", "StrokeColorSpace", "DeviceGray.Instance")]
    [MacroItem("IColorSpace", "NonstrokeColorSpace", "DeviceGray.Instance")]
    [MacroItem("DeviceColor", "StrokeColor", "DeviceColor.Black")]
    [MacroItem("DeviceColor", "NonstrokeColor", "DeviceColor.Black")]
    [MacroCode("public ~0~ ~1~ {get; private set;} = ~2~;")]
    [MacroCode("    ~1~ = other.~1~;", Prefix = "public void CopyFrom(GraphicsState other){", Postfix = "}")]
    public void SaveGraphicsState() { }
    public void RestoreGraphicsState() { }

    public void ModifyTransformMatrix(in Matrix3x2 newTransform)
    {
        TransformMatrix = newTransform * TransformMatrix;
    }

    public Vector2 ApplyCurrentTransform(in Vector2 point) => Vector2.Transform(point, TransformMatrix);

    #region Drawing

    public void SetLineWidth(double width) => LineWidth = width;
    public void SetLineCap(LineCap cap) => LineCap = cap;
    public void SetLineJoinStyle(LineJoinStyle lineJoinStyle) => LineJoinStyle = lineJoinStyle;
    public void SetMiterLimit(double miter) => MiterLimit = miter;

    public void SetLineDashPattern(double dashPhase, in ReadOnlySpan<double> dashArray)
    {
        DashPhase = dashPhase;
        // this copies the dasharray on write, which is unavoidable, but then it reuses the array object when
        // we duplicate the graphicsstate, which we expect to happen frequently.
        DashArray = dashArray.ToArray(); 
    }
    
    private async ValueTask SetLineDashPattern(PdfArray entryValue)
    {
        DashPhase = (await entryValue.GetAsync<PdfNumber>(1)).DoubleValue;
        var pattern = await entryValue.GetAsync<PdfArray>(0);
        var patternNums = new double[pattern.Count];
        for (int i = 0; i < pattern.Count; i++)
        {
            patternNums[i] = (await pattern.GetAsync<PdfNumber>(i)).DoubleValue;
        }
        DashArray = patternNums;
    }
    
    // as of 11/18/2021 this parameter is ignored by both the Pdf and Skia renderers, but
    // we will preserve the property.
    public void SetFlatnessTolerance(double flatness) => FlatnessTolerance = flatness;
    

    #endregion    
    
    public async ValueTask LoadGraphicStateDictionary(PdfDictionary dictionary)
    {
        foreach (var entry in dictionary.RawItems)
        {
            var hashCode = entry.Key.GetHashCode();
            switch (hashCode)
            {
                case KnownNameKeys.LW:
                    SetLineWidth((await EntryValue<PdfNumber>(entry)).DoubleValue);
                    break;
                case KnownNameKeys.FL:
                    SetFlatnessTolerance((await EntryValue<PdfNumber>(entry)).DoubleValue);
                    break;
                case KnownNameKeys.LC:
                    SetLineCap((LineCap)(await EntryValue<PdfNumber>(entry)).IntValue);
                    break;
                case KnownNameKeys.LJ:
                    SetLineJoinStyle((LineJoinStyle)(await EntryValue<PdfNumber>(entry)).IntValue);
                    break;
                case KnownNameKeys.ML:
                    SetMiterLimit((await EntryValue<PdfNumber>(entry)).DoubleValue);
                    break;
                case KnownNameKeys.D:
                    await SetLineDashPattern(await EntryValue<PdfArray>(entry));
                    break;
                case KnownNameKeys.RI:
                    SetRenderIntent(new RenderIntentName(await EntryValue<PdfName>(entry)));
                    break;
            }
        }
    }
    
    private static async ValueTask<T> EntryValue<T>(KeyValuePair<PdfName, PdfObject> entry)
        where T:PdfObject
    {
        return (T) await entry.Value.DirectValueAsync();
    }

    #region Text

    public void SetCharSpace(double value)
    {
        throw new NotImplementedException();
    }

    public void SetWordSpace(double value)
    {
        throw new NotImplementedException();
    }

    public void SetHorizontalTextScaling(double value)
    {
        throw new NotImplementedException();
    }

    public void SetTextLeading(double value)
    {
        throw new NotImplementedException();
    }

    public void SetFont(PdfName font, double size)
    {
        throw new NotImplementedException();
    }

    public void SetTextRender(TextRendering rendering)
    {
        throw new NotImplementedException();
    }

    public void SetTextRise(double value)
    {
        throw new NotImplementedException();
    }

    #endregion

    #region Color
    public void SetRenderIntent(RenderIntentName intent) => RenderIntent = intent;
    public void SetStrokeColorSpace(IColorSpace colorSpace)
    {
        StrokeColor = colorSpace.DefaultColor();
        StrokeColorSpace = colorSpace;
    }

    public void SetNonstrokeColorSpace(IColorSpace colorSpace)
    {
        NonstrokeColor = colorSpace.DefaultColor();
        NonstrokeColorSpace = colorSpace;
    }

    public void SetStrokeColor(in ReadOnlySpan<double> color) => 
        StrokeColor = StrokeColorSpace.SetColor(color);
    public void SetNonstrokingColor(in ReadOnlySpan<double> color) => 
        NonstrokeColor = NonstrokeColorSpace.SetColor(color);
    #endregion
}

public static class GraphicsStateHelpers
{
    public static bool IsDashedStroke(this GraphicsState gs) =>
        gs.DashArray.Length > 0;
}