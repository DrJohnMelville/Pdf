using Melville.Fonts.SfntParsers.TableDeclarations;
using Melville.Fonts.SfntParsers.TableParserParts;

using Melville.Parsing.MultiplexSources;
using Melville.StreamInterfaces;
using Melville.StreamInterfaces.Adapters;
using Melville.StreamInterfaces.ParsingBuffers;

namespace Melville.Fonts.SfntParsers;

internal readonly struct SfntParser(IMultiplexSource src)
{
    public async ValueTask<IReadOnlyList<IGenericFont>> ParseAsync(ulong rootPosition)
    {
        using var buffer = src.ParsingBufferFrom((long)rootPosition);
        var rec = await FieldParser.ReadFromAsync<TableDirectory>(buffer).CA();
        return rec.Parse(src);
    }
}

internal readonly struct FontCollectionParser(IMultiplexSource src)
{
    public async ValueTask<IReadOnlyList<IGenericFont>> ParseAsync()
    {
        using var buffer = src.ParsingBufferFrom(0);
        return await (
                await FieldParser.ReadFromAsync<FontCollectionHeader>(buffer).CA())
            .ParseAsync(src).CA();
    }
}