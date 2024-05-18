using Melville.Fonts.SfntParsers.TableDeclarations;
using Melville.Fonts.SfntParsers.TableParserParts;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.MultiplexSources;

namespace Melville.Fonts.SfntParsers;

internal readonly struct SfntParser(IMultiplexSource src)
{
    public async ValueTask<IReadOnlyList<IGenericFont>> ParseAsync(ulong rootPosition)
    {
        var rec = await FieldParser.ReadFromAsync<TableDirectory>(
            src.ReadPipeFrom((long)rootPosition));
        return rec.Parse(src);
    }
}

internal readonly struct FontCollectionParser(IMultiplexSource src)
{
    public async ValueTask<IReadOnlyList<IGenericFont>> ParseAsync() => 
        await (
            await FieldParser.ReadFromAsync<FontCollectionHeader>(src.ReadPipeFrom(0)).CA())
            .ParseAsync(src).CA();
}