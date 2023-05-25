using System.Collections.Immutable;
using System.IO;
using Melville.Icc.Model;
using Melville.Pdf.Model.Renderers.Colors;

namespace Melville.Pdf.LowLevelViewerParts.LowLevelViewer.DocumentParts.ColorSpaces;

public static class ColorSpaceViewModelFactory
{
    public static async ValueTask<MultiColorSpaceViewModel> CreateAsync(Stream iccData)
    {
        var models = new List<ColorSpaceViewModel>(2);
        await TryAddIccProfileAsync(iccData, models);
        AddColorSpace(models, StaticColorSpaces.DeviceRgb(), "Device RGB");
        return new MultiColorSpaceViewModel(models);
    }

    private static async Task TryAddIccProfileAsync(Stream iccData, List<ColorSpaceViewModel> models)
    {
        if (await TryReadIccProfileAsync(iccData) is {} colorSpace)
            AddColorSpace(models, colorSpace, "ICC Profile");
    }

    private static void AddColorSpace(
        List<ColorSpaceViewModel> models, IColorSpace colorSpace, string profileName) =>
        models.Add(new ColorSpaceViewModel(profileName, colorSpace, ParseInputAxes(colorSpace)));

    private static ImmutableList<ColorAxisViewModel> ParseInputAxes(IColorSpace colorSpace) => 
        colorSpace.ColorComponentRanges(8).Select(i=>new ColorAxisViewModel(i)).ToImmutableList();

    private static async Task<IColorSpace?> TryReadIccProfileAsync(Stream iccData)
    {
        try
        {
            return await IccProfileColorSpaceParser.ParseAsync(iccData);
        }
        catch (Exception)
        {
            return null;
        }
    }
}