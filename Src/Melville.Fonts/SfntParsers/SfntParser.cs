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
        using var _ = Force.Using(src.ReaderFrom((long)rootPosition), out var reader);
        using var buffer = new ParsingBuffer(reader);
        var rec = await FieldParser.ReadFromAsync<TableDirectory>(buffer).CA();
        return rec.Parse(src);
    }
}

internal readonly struct FontCollectionParser(IMultiplexSource src)
{
    public async ValueTask<IReadOnlyList<IGenericFont>> ParseAsync()
    {
        using var _ = Force.Using(src.ReaderFrom(0), out var reader);
        using var buffer = new ParsingBuffer(reader);
        return await (
                await FieldParser.ReadFromAsync<FontCollectionHeader>(buffer).CA())
            .ParseAsync(src).CA();
    }
}