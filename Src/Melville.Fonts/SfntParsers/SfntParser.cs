using Melville.Fonts.SfntParsers.TableDeclarations;
using Melville.Fonts.SfntParsers.TableParserParts;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.MultiplexSources;

namespace Melville.Fonts.SfntParsers;

internal readonly struct SfntParser(IMultiplexSource src)
{
    public async ValueTask<IReadOnlyList<IGenericFont>> ParseAsync(int rootPosition)
    {
        return new SFnt();
    }
}

internal readonly struct FontCollectionParser(IMultiplexSource src)
{
    public async ValueTask<IReadOnlyList<IGenericFont>> ParseAsync()
    {
        var header = await FieldParser.ReadFromAsync<FontCollectionHeader>(src.ReadPipeFrom(0));
        var ret = new IGenericFont[header.]
    }
}