using System.IO.Pipelines;
using Melville.Fonts.SfntParsers.TableParserParts;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.CountingReaders;

namespace Melville.Fonts.SfntParsers.TableDeclarations.CMaps;

internal readonly partial struct CmapFormat0Parser
{
    [SFntField] private readonly ushort format;
    [SFntField] private readonly ushort length;
    [SFntField] private readonly ushort language;
    [SFntField("256")] private readonly byte[] glyphIdArray;

    public static async Task<ICmapImplementation> ParseAsync(IByteSource input)
    {
        var parsed = await FieldParser.ReadFromAsync<CmapFormat0Parser>(input).CA();
        return new SingleArrayCmap<byte>(1, 0, parsed.glyphIdArray);
    }
}