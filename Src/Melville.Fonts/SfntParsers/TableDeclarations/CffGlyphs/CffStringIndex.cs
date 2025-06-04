using System.Text;
using Melville.Parsing.AwaitConfiguration;

namespace Melville.Fonts.SfntParsers.TableDeclarations.CffGlyphs;

internal readonly struct CffStringIndex(CffIndex customStrings)
{
    public ValueTask<string> GetNameAsync(int index) =>
        index < Type1StandardStrings.Instance.Length? 
            new(Type1StandardStrings.Instance[index]):
            LoadCustomStringAsync(index - Type1StandardStrings.Instance.Length);

    private async ValueTask<string> LoadCustomStringAsync(int localIndex)
    {
        if (localIndex >= customStrings.Length)
            return ".notdef";
        using var data = await customStrings.ItemDataAsync(localIndex).CA();
        return Encoding.UTF8.GetString(
            data.Sequence);
    }
}

internal readonly struct StandardCharsetFactory<T>(T target) where T: ICharSetTarget
{
    public ValueTask<T> FromByteAsync(long charSetOffset) => charSetOffset switch
    {
        0 => IsoAdobeAsync(),
        1 => ExpertAsync(),
        2 => ExpertSubsetAsync(),
        _ => throw new InvalidDataException("Invalid standard charset idnex")
    };

    private static readonly (ushort, ushort)[] IsoAdobeMappings = [
      (0,228)
    ];

    private ValueTask<T> IsoAdobeAsync() => MapArrayAsync(IsoAdobeMappings);

    private async ValueTask<T> MapArrayAsync((ushort, ushort)[] values)
    {
        int index = 0;
        foreach (var (min, max) in values)
        {
            for (var i = min; i <= max; i++)
            {
                await target.SetGlyphNameAsync(index++, i).CA();
                if (index >= target.Count) return target;
            }
        }
        return target;
    }

    private static readonly (ushort, ushort)[] ExpertMappings =
    [
        (0, 1),
        (229, 238),
        (13, 15),
        (99, 99),
        (239, 248),
        (27, 28),
        (249, 265),
        (266, 266),
        (109, 110),
        (267, 318),
        (158, 158),
        (155, 155),
        (163, 163),
        (319, 326),
        (150, 150),
        (164, 164),
        (169, 169),
        (327, 378)
    ];


    private ValueTask<T> ExpertAsync() => MapArrayAsync(ExpertMappings);

    private static readonly (ushort, ushort)[] ExpertSubsetMappings =
    [
        (0, 1),
        (231, 232),
        (235, 238),
        (13, 15),
        (99, 99),
        (239, 248),
        (27,28),
        (249, 251),
        (253, 266),
        (109,110),
        (267,270),
        (272,272),
        (300,302),
        (305,305),
        (314, 315),
        (158, 158),
        (155, 155),
        (163, 163),
        (320,326),
        (150, 150),
        (164, 164),
        (169, 169),
        (327, 346),
    ];

    private ValueTask<T> ExpertSubsetAsync() =>
        MapArrayAsync(ExpertSubsetMappings);

}