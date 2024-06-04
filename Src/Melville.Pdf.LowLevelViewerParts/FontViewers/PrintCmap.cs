using Melville.Fonts;

namespace Melville.Pdf.LowLevelViewerParts.FontViewers;

public static class PrintCmap
{
    public static async ValueTask<IReadOnlyList<String>> PrintCMapAsync(ICMapSource cmap, int index)
    {
        try
        {
            var subTable = await cmap.GetByIndexAsync(index);
            return subTable.AllMappings()
                .Where(i=>i.Glyph > 0) // everything not mapped is 0 so we do not need to list them
                .Select(i => $"{VariableByteString(i.Bytes, i.Character)} => {i.Glyph:X4}").ToArray();
        }
        catch (Exception e)
        {
            return [e.Message];
        }

    }

    public static string CmapName(ICMapSource cmap, int index)
    {
        var (platform, encoding) = cmap.GetPlatformEncoding(index);
        return $"{CmapPlatFormAndEncodingNames.PlatformName(platform)}({platform}): {CmapPlatFormAndEncodingNames.EncodingName(platform, encoding)} ({encoding})";
    }

    private static string VariableByteString(int bytes, uint character) => bytes switch
    {
        1 => $"{character:X2}",
        2 => $"{character:X4}",
        4 => $"{character:X8}",
        _ => $"{character:X}"
    };
}