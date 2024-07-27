using Melville.Fonts.SfntParsers;
using Melville.Fonts.SfntParsers.TableDeclarations.CMaps;
using Melville.Fonts.SfntParsers.TableDeclarations.Heads;

namespace Melville.Fonts.RawCffParsers;

internal class BareCffWrapper(IGlyphSource innerSource) : 
    ListOf1GenericFont, ICMapSource, IGlyphWidthSource
{
    public override ValueTask<ICMapSource> GetCmapSourceAsync() =>
        new(this);
    
    public override ValueTask<IGlyphSource> GetGlyphSourceAsync() =>
        new(innerSource);

    public override ValueTask<string[]> GlyphNamesAsync() => new([]);

    public override ValueTask<IGlyphWidthSource> GlyphWidthSourceAsync() => new(this);

    public override ValueTask<string> FontFamilyNameAsync() => new ("Raw Cff Font");

    public override ValueTask<MacStyles> GetFontStyleAsync() => new (MacStyles.None);

    public ValueTask<ICmapImplementation> GetByIndexAsync(int index) =>
        new(new SingleArrayCmap<byte>(1, 0, []));

    public ValueTask<ICmapImplementation?> GetByPlatformEncodingAsync(int platform, int encoding)
    {
        return new((ICmapImplementation?)null);
    }

    public (int platform, int encoding) GetPlatformEncoding(int index)
    {
        return (0, 0);
    }

    public float GlyphWidth(ushort glyph)
    {
        return 0f;
    }
}