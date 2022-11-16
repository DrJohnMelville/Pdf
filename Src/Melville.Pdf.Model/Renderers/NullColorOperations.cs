using System;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.Model.Renderers;

[StaticSingleton()]
public partial class NullColorOperations : IColorOperations
{
    public ValueTask SetStrokingColorSpace(PdfName colorSpace) => ValueTask.CompletedTask;

    public ValueTask SetNonstrokingColorSpace(PdfName colorSpace) => ValueTask.CompletedTask;

    public ValueTask SetStrokeColorExtended(PdfName? patternName, in ReadOnlySpan<double> colors) => 
        ValueTask.CompletedTask;

    public ValueTask SetNonstrokingColorExtended(PdfName? patternName, in ReadOnlySpan<double> colors) => 
        ValueTask.CompletedTask;

    public ValueTask SetStrokeGray(double grayLevel) => ValueTask.CompletedTask;

    public ValueTask SetStrokeRGB(double red, double green, double blue) => ValueTask.CompletedTask;

    public ValueTask SetStrokeCMYK(double cyan, double magenta, double yellow, double black) => 
        ValueTask.CompletedTask;

    public ValueTask SetNonstrokingGray(double grayLevel) => ValueTask.CompletedTask;

    public ValueTask SetNonstrokingRGB(double red, double green, double blue) => ValueTask.CompletedTask;

    public ValueTask SetNonstrokingCMYK(double cyan, double magenta, double yellow, double black) => 
        ValueTask.CompletedTask;

    public void SetStrokeColor(in ReadOnlySpan<double> components) {}

    public void SetNonstrokingColor(in ReadOnlySpan<double> components) {}

    public void SetRenderIntent(RenderIntentName intent) { }
}