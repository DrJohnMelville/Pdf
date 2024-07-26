using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Xml;
using Melville.Fonts.SfntParsers.TableParserParts;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.MultiplexSources;

namespace Melville.Fonts.SfntParsers.TableDeclarations.Names;

internal readonly struct NameTableParser(IMultiplexSource source)
{
    public async ValueTask<ParsedNameTable> ParseAsync()
    {
        using var pipe = source.ReadPipeFrom(0);
        var header = await FieldParser.ReadFromAsync<NameTableHeader>(pipe).CA();
        return new ParsedNameTable(source, header);
    }
}

internal readonly partial struct NameTableHeader
{
    [SFntField] private readonly ushort formatVersion;
    [SFntField] private readonly ushort numNameRecords;
    [SFntField] private readonly ushort storageOffset;
    [SFntField("this.numNameRecords")] readonly NameRecord[] nameRecords;

    public NameRecord[] Items => nameRecords;
    public ushort StorageOffset => storageOffset;
}

internal readonly partial struct NameRecord
{
    [SFntField] public ushort PlatformId { get; }
    [SFntField] public ushort EncodingId { get; }
    [SFntField] public ushort LanguageId { get; }
    [SFntField] private readonly ushort nameId;
    [SFntField] private readonly ushort length;
    [SFntField] private readonly ushort offset;

    public SfntNameKey NameId => (SfntNameKey)nameId;

    internal bool Matches(SfntNameKey name, ushort platform)
    {
        return name == NameId &&
                         (platform == PlatformId || platform == 0xFFFF);
    }

    internal ushort Offset => offset;
    internal ushort Length => length;
}

internal class NullNameTableView: INameTableView
{
    public ValueTask<NameTableLine[]> GetAllNamesAsync() => new([]);
}