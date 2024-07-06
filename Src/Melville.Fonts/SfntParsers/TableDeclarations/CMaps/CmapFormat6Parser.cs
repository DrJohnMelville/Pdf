using System.IO.Pipelines;
using Melville.Fonts.SfntParsers.TableParserParts;
using Melville.Parsing.AwaitConfiguration;

namespace Melville.Fonts.SfntParsers.TableDeclarations.CMaps;

internal readonly partial struct CmapFormat6Parser
{
    [SFntField] private readonly ushort format;
    [SFntField] private readonly ushort length;
    [SFntField] private readonly ushort language;
    [SFntField] private readonly ushort firstCode;
    [SFntField] private readonly ushort entryCount;
    [SFntField("entryCount")] private readonly ushort[] glyphIdArray;

    public static async ValueTask<ICmapImplementation> ParseAsync(PipeReader input)
    {
        var record = await FieldParser.ReadFromAsync<CmapFormat6Parser>(input).CA();
        return new CmapFormat6Implementation(
            record.firstCode, record.glyphIdArray);
    }
}

internal class CmapFormat6Implementation(ushort first, ushort[] glyphs) : ICmapImplementation
{
    public IEnumerable<(int Bytes, uint Character, uint Glyph)> AllMappings()
    {
        for (int i = 0; i < glyphs.Length; i++)
            yield return (2, (uint)(first + i), glyphs[i]);
    }

    public bool TryMap(int bytes, uint character, out uint glyph)
    {
        if (bytes < 2) { glyph = 0; return false; }

        var index = (uint)(character - first);
        glyph = index < glyphs.Length ? glyphs[index] : (uint)0;
        return true;
    }
}