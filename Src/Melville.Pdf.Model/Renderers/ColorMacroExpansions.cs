using System;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.Renderers.Colors;
using Melville.Pdf.Model.Renderers.GraphicsStates;

namespace Melville.Pdf.Model.Renderers;

public partial class ColorMacroExpansions : IColorOperations
{
    [FromConstructor] private readonly IGraphicsState target;
    [FromConstructor] private readonly IHasPageAttributes page;
    [FromConstructor] private readonly DocumentRenderer renderer;

    public void SetStrokeColor(in ReadOnlySpan<double> components) => 
        target.CurrentState().SetStrokeColor(components);

    public void SetNonstrokingColor(in ReadOnlySpan<double> components) => 
        target.CurrentState().SetNonstrokingColor(components);

    public void SetRenderIntent(RenderIntentName intent) => target.CurrentState().SetRenderIntent(intent);

    public async ValueTask SetStrokingColorSpace(PdfName colorSpace) =>
        target.CurrentState().SetStrokeColorSpace(
            await new ColorSpaceFactory(page).ParseColorSpace(colorSpace).CA());
    
    public async ValueTask SetNonstrokingColorSpace(PdfName colorSpace) =>
        target.CurrentState().SetNonstrokeColorSpace(
            await new ColorSpaceFactory(page).ParseColorSpace(colorSpace).CA());
    
    public ValueTask SetStrokeColorExtended(PdfName? patternName, in ReadOnlySpan<double> colors)
    {
        if (colors.Length > 0) SetStrokeColor(colors);
        return SetStrokingPattern(patternName);
    }

    private async ValueTask SetStrokingPattern(PdfName? patternName)
    {
        if ((await GetPatternDict(patternName).CA()) is { } patternDict)
            await target.CurrentState()
                .SetStrokePattern(patternDict, renderer).CA();
    }

    public ValueTask SetNonstrokingColorExtended(PdfName? patternName, in ReadOnlySpan<double> colors)
    {
        if (colors.Length > 0) SetNonstrokingColor(colors);
        return SetNonstrokingPattern(patternName);
    }

    private async ValueTask SetNonstrokingPattern(PdfName? patternName)
    {
        if ((await GetPatternDict(patternName).CA()) is { } patternDict)
            await target.CurrentState()
                .SetNonstrokePattern(patternDict, renderer).CA();
    }

    private async ValueTask<PdfDictionary?> GetPatternDict(PdfName? patternName) =>
        patternName != null && (await page.GetResourceAsync(ResourceTypeName.Pattern, patternName).CA()) is
        PdfDictionary patternDict
            ? patternDict
            : null;

        public async ValueTask SetStrokeGray(double grayLevel)
    {
        await SetStrokingColorSpace(KnownNames.DeviceGray).CA();
        SetStrokeColor(stackalloc double[] { grayLevel });
    }

    public async ValueTask SetStrokeRGB(double red, double green, double blue)
    {
        await SetStrokingColorSpace(KnownNames.DeviceRGB).CA();
        SetStrokeColor(stackalloc double[] { red, green, blue });
    }

    public async ValueTask SetStrokeCMYK(double cyan, double magenta, double yellow, double black)
    {
        await SetStrokingColorSpace(KnownNames.DeviceCMYK).CA();
        SetStrokeColor(stackalloc double[] { cyan, magenta, yellow, black });
    }

    public async ValueTask SetNonstrokingGray(double grayLevel)
    {
        await SetNonstrokingColorSpace(KnownNames.DeviceGray).CA();
        SetNonstrokingColor(stackalloc double[] { grayLevel });
    }

    public async ValueTask SetNonstrokingRGB(double red, double green, double blue)
    {
        await SetNonstrokingColorSpace(KnownNames.DeviceRGB).CA();
        SetNonstrokingColor(stackalloc double[] { red, green, blue });
    }

    public async ValueTask SetNonstrokingCMYK(double cyan, double magenta, double yellow, double black)
    {
        await SetNonstrokingColorSpace(KnownNames.DeviceCMYK).CA();
        SetNonstrokingColor(stackalloc double[] { cyan, magenta, yellow, black });
    }
}