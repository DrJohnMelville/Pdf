using Melville.Fonts.SfntParsers.TableDeclarations.CffGlyphs;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.MultiplexSources;

namespace Melville.Fonts.RawCffParsers;

public readonly struct RawCffParser(IMultiplexSource source)
{
    public async ValueTask<IReadOnlyList<IGenericFont>> ParseAsync()
    {
        var innerSource = 
            await new CffGlyphSourceParser(source, 1000).ParseAsync().CA();
        return new BareCffWrapper(innerSource);
    }   
}