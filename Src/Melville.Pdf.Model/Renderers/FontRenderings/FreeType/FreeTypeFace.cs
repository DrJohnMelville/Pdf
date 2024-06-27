using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Melville.Fonts;
using Melville.Fonts.SfntParsers.TableDeclarations.CMaps;
using Melville.SharpFont;

namespace Melville.Pdf.Model.Renderers.FontRenderings.FreeType;

public class FreeTypeFace(Face font) : IGenericFont, ICMapSource, IGlyphSource
{
    public ValueTask<ICMapSource> GetCmapSourceAsync() => 
        ValueTask.FromResult<ICMapSource>(this);

    public ValueTask<IGlyphSource> GetGlyphSourceAsync() => ValueTask.FromResult<IGlyphSource>(this);

    public ValueTask<string[]> GlyphNamesAsync()
    {
        var ret = font.AllGlyphNames().ToArray();
        return ValueTask.FromResult(ret.Select(i => i.Name).ToArray());
    }

    public int Count => font.CharmapsCount;

    public ValueTask<ICmapImplementation> GetByIndexAsync(int index) =>
        ValueTask.FromResult<ICmapImplementation>(new FreeTypeCmapImplementation(font.CharMaps[index]));

    public ValueTask<ICmapImplementation?> GetByPlatformEncodingAsync(int platform, int encoding)
    {
        var cmap = font.CharMapByInts(platform, encoding);
        return ValueTask.FromResult<ICmapImplementation?>(cmap is null ? null : 
            new FreeTypeCmapImplementation(cmap));
    }

    public (int platform, int encoding) GetPlatformEncoding(int index)
    {
        var map = font.CharMaps[index];
        return ((int)map.PlatformId, map.EncodingId);
    }

    public int GlyphCount => font.GlyphCount;

    public ValueTask RenderGlyphAsync<T>(uint glyph, T target, Matrix3x2 transform) where T : IGlyphTarget
    {
        throw new NotImplementedException();
    }
}

public class FreeTypeCmapImplementation (CharMap charmap) : ICmapImplementation
{
    public IEnumerable<(int Bytes, uint Character, uint Glyph)> AllMappings() =>
        charmap.AllMappings().Select(i => (2, i.Char, i.Glyph));

    public bool TryMap(int bytes, uint character, out uint glyph)
    {
        charmap.Face.SetCharmap(charmap);
        glyph = charmap.Face.GetCharIndex(character);
        return true;
    }
}