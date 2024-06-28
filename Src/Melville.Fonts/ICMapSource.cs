using Melville.Fonts.SfntParsers.TableDeclarations.CMaps;
using Melville.Parsing.AwaitConfiguration;

namespace Melville.Fonts;

/// <summary>
/// This interface represents the CMaps or character to glyph mappings available within
/// a font.
/// </summary>
public interface ICMapSource
{
    /// <summary>
    /// Number of CMaps supported
    /// </summary>
    int Count { get; }

    /// <summary>
    /// Load a CMap implementation by index.
    /// </summary>
    /// <param name="index">The index of the desired Cmap</param>
    /// <returns>An object that implements the given CMAP</returns>
    ValueTask<ICmapImplementation> GetByIndexAsync(int index);

    /// <summary>
    /// Load a CMap implementation by platform and encoding
    /// </summary>
    /// <param name="platform">The Platform encoding that was desired</param>
    /// <param name="encoding">The desired encoding for the platform</param>
    /// <returns>An object that implements the given CMAP, or null if there is none</returns>
    ValueTask<ICmapImplementation?> GetByPlatformEncodingAsync(int platform, int encoding);

    /// <summary>
    /// Get the platform and encoding for a given subtable.
    /// </summary>
    /// <param name="index">The index of the subtable to get information from</param>
    /// <returns>The platform and encoding values for the indicated CMap</returns>
    (int platform, int encoding) GetPlatformEncoding(int index);
}

/// <summary>
/// Extra methods for ICMapSource
/// </summary>
public static class ICMapSourceExtensions
{
    /// <summary>
    /// Get the most modern and largest unicode cmap.
    /// </summary>
    /// <param name="source">The source of the CMaps</param>
    /// <returns>The best unicode encoding cmap, or null if there is none</returns>
    public static async ValueTask<ICmapImplementation?> GetUnicodeCMapAsync(this ICMapSource source)
    {
        var (index, ranking) = source.AllPlatformEncodings()
            .DefaultIfEmpty((index:-1, ranking: -1))
            .MaxBy(i=>i.ranking);

        return index >= 0 && ranking >= 0  ? await source.GetByIndexAsync(index).CA() :null;

    }

    private static IEnumerable<(int index, int ranking)> AllPlatformEncodings(this ICMapSource source)
    {
        for (int i = 0; i < source.Count; i++)
        {
            var (platform, encoding) = source.GetPlatformEncoding(i);
            yield return (i, RankEncoding(platform, encoding));
        }
    }

    private static int RankEncoding(int platform, int encoding) => (platform, encoding) switch
    {
        (3,10) => 101,
        (3,1) => 100,
        (0, var x) => 80+x,
        _=> -1
    };
}