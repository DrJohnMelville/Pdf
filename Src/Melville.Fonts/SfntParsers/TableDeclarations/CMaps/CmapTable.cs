using Melville.Fonts.SfntParsers.TableParserParts;

namespace Melville.Fonts.SfntParsers.TableDeclarations.CMaps;

internal readonly partial struct CmapTable
{
    [SFntField] private readonly ushort version;
    [SFntField] private readonly ushort numTables;
    [SFntField("this.numTables")] public CmapTablePointer[] Tables { get; }
}