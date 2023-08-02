using System;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.Renderers.Colors;
using Melville.Pdf.Model.Renderers.DocumentRenderers;
using Melville.Pdf.Model.Renderers.GraphicsStates;

namespace Melville.Pdf.Model.Renderers.ColorOperations;

internal partial class ColorMacroExpansions : IColorOperations
{
    [FromConstructor] private readonly IGraphicsState target;
    [FromConstructor] private readonly IHasPageAttributes page;
    [FromConstructor] private readonly DocumentRenderer renderer;

    public void SetStrokeColor(in ReadOnlySpan<double> components) => 
        target.CurrentState().SetStrokeColor(components);

    public void SetNonstrokingColor(in ReadOnlySpan<double> components) => 
        target.CurrentState().SetNonstrokingColor(components);

    public void SetRenderIntent(RenderIntentName intent) => target.CurrentState().SetRenderIntent(intent);

    public async ValueTask SetStrokingColorSpaceAsync(PdfDirectValue colorSpace) =>
        target.CurrentState().SetStrokeColorSpace(
            await new ColorSpaceFactory(page).ParseColorSpaceAsync(colorSpace).CA());
    
    public async ValueTask SetNonstrokingColorSpaceAsync(PdfDirectValue colorSpace) =>
        target.CurrentState().SetNonstrokeColorSpace(
            await new ColorSpaceFactory(page).ParseColorSpaceAsync(colorSpace).CA());
    
    public ValueTask SetStrokeColorExtendedAsync(PdfDirectValue? patternName, in ReadOnlySpan<double> colors)
    {
        if (colors.Length > 0) SetStrokeColor(colors);
        return SetStrokingPatternAsync(patternName);
    }

    private async ValueTask SetStrokingPatternAsync(PdfDirectValue? patternName)
    {
        if ((await GetPatternDictAsync(patternName).CA()) is { } patternDict)
            await target.CurrentState()
                .SetStrokePatternAsync(patternDict, renderer).CA();
    }

    public ValueTask SetNonstrokingColorExtendedAsync(PdfDirectValue? patternName, in ReadOnlySpan<double> colors)
    {
        if (colors.Length > 0) SetNonstrokingColor(colors);
        return SetNonstrokingPatternAsync(patternName);
    }

    private async ValueTask SetNonstrokingPatternAsync(PdfDirectValue? patternName)
    {
        if ((await GetPatternDictAsync(patternName).CA()) is { } patternDict)
            await target.CurrentState()
                .SetNonstrokePatternAsync(patternDict, renderer).CA();
    }

    private async ValueTask<PdfValueDictionary?> GetPatternDictAsync(PdfDirectValue? patternName) =>
        patternName.HasValue && (await page.GetResourceAsync(ResourceTypeName.Pattern, patternName.Value).CA())
        .TryGet(out PdfValueDictionary patternDict)
            ? patternDict
            : null;

        public async ValueTask SetStrokeGrayAsync(double grayLevel)
    {
        await SetStrokingColorSpaceAsync(KnownNames.DeviceGrayTName).CA();
        SetStrokeColor(stackalloc double[] { grayLevel });
    }

    public async ValueTask SetStrokeRGBAsync(double red, double green, double blue)
    {
        await SetStrokingColorSpaceAsync(KnownNames.DeviceRGBTName).CA();
        SetStrokeColor(stackalloc double[] { red, green, blue });
    }

    public async ValueTask SetStrokeCMYKAsync(double cyan, double magenta, double yellow, double black)
    {
        await SetStrokingColorSpaceAsync(KnownNames.DeviceCMYKTName).CA();
        SetStrokeColor(stackalloc double[] { cyan, magenta, yellow, black });
    }

    public async ValueTask SetNonstrokingGrayAsync(double grayLevel)
    {
        await SetNonstrokingColorSpaceAsync(KnownNames.DeviceGrayTName).CA();
        SetNonstrokingColor(stackalloc double[] { grayLevel });
    }

    public async ValueTask SetNonstrokingRgbAsync(double red, double green, double blue)
    {
        await SetNonstrokingColorSpaceAsync(KnownNames.DeviceRGBTName).CA();
        SetNonstrokingColor(stackalloc double[] { red, green, blue });
    }

    public async ValueTask SetNonstrokingCMYKAsync(double cyan, double magenta, double yellow, double black)
    {
        await SetNonstrokingColorSpaceAsync(KnownNames.DeviceCMYKTName).CA();
        SetNonstrokingColor(stackalloc double[] { cyan, magenta, yellow, black });
    }
}