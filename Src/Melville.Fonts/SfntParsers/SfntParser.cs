using Melville.Fonts.SfntParsers.TableDeclarations;
using Melville.Fonts.SfntParsers.TableParserParts;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.MultiplexSources;

namespace Melville.Fonts.SfntParsers;

internal readonly struct SfntParser(IMultiplexSource src)
{
    public async ValueTask<IReadOnlyList<IGenericFont>> ParseAsync(ulong rootPosition)
    {
        using var pipe = src.ReadPipeFrom((long)rootPosition);
        var rec = await FieldParser.ReadFromAsync<TableDirectory>(
            pipe).CA();
        return rec.Parse(src);
    }
}

internal readonly struct FontCollectionParser(IMultiplexSource src)
{
    public async ValueTask<IReadOnlyList<IGenericFont>> ParseAsync()
    {
        using var pipe = src.ReadPipeFrom(0);
        return await (
                await FieldParser.ReadFromAsync<FontCollectionHeader>(pipe).CA())
            .ParseAsync(src).CA();
    }
}