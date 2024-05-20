using Melville.Fonts.SfntParsers.TableParserParts;
using Melville.Parsing.MultiplexSources;

namespace Melville.Fonts.SfntParsers.TableDeclarations;

internal readonly partial struct TableDirectory
{
    [SFntField] private readonly UInt32 formatVersion;
    [SFntField] private readonly UInt16 numTables;
    [SFntField] private readonly UInt16 searchRange;
    [SFntField] private readonly UInt16 entrySelector;
    [SFntField] private readonly UInt16 rangeShift;
    [SFntField("this.numTables")] private readonly TableRecord[] tableRecords;

    public SFnt Parse(IMultiplexSource source) => new SFnt(source, tableRecords);
}