using System;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.Model.Renderers.ColorOperations;

[StaticSingleton()]
internal partial class NullColorOperations : IColorOperations
{
    public ValueTask SetStrokingColorSpaceAsync(PdfName colorSpace) => ValueTask.CompletedTask;

    public ValueTask SetNonstrokingColorSpaceAsync(PdfName colorSpace) => ValueTask.CompletedTask;

    public ValueTask SetStrokeColorExtendedAsync(PdfName? patternName, in ReadOnlySpan<double> colors) => 
        ValueTask.CompletedTask;

    public ValueTask SetNonstrokingColorExtendedAsync(PdfName? patternName, in ReadOnlySpan<double> colors) => 
        ValueTask.CompletedTask;

    public ValueTask SetStrokeGrayAsync(double grayLevel) => ValueTask.CompletedTask;

    public ValueTask SetStrokeRGBAsync(double red, double green, double blue) => ValueTask.CompletedTask;

    public ValueTask SetStrokeCMYKAsync(double cyan, double magenta, double yellow, double black) => 
        ValueTask.CompletedTask;

    public ValueTask SetNonstrokingGrayAsync(double grayLevel) => ValueTask.CompletedTask;

    public ValueTask SetNonstrokingRgbAsync(double red, double green, double blue) => ValueTask.CompletedTask;

    public ValueTask SetNonstrokingCMYKAsync(double cyan, double magenta, double yellow, double black) => 
        ValueTask.CompletedTask;

    public void SetStrokeColor(in ReadOnlySpan<double> components) {}

    public void SetNonstrokingColor(in ReadOnlySpan<double> components) {}

    public void SetRenderIntent(RenderIntentName intent) { }
}