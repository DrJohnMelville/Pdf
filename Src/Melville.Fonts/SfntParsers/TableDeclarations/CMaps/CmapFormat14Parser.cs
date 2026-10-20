using System.IO.Pipelines;
using System.Security.AccessControl;
using Melville.Fonts.SfntParsers.TableParserParts;

using Melville.Parsing.CountingReaders;
using Melville.Parsing.MultiplexSources;
using Melville.StreamInterfaces;
using Melville.StreamInterfaces.Adapters;
using Melville.StreamInterfaces.ParsingBuffers;
using Microsoft.CodeAnalysis;

namespace Melville.Fonts.SfntParsers.TableDeclarations.CMaps;

internal readonly partial struct CmapFormat14Parser
{
    [SFntField] private readonly ushort format;
    [SFntField] private readonly uint length;
    [SFntField] private readonly uint numVarSelectorRecords;
    [SFntField("numVarSelectorRecords")] private readonly VariantSelectorRecord[] varSelectorRecords;
    public static async ValueTask<ICmapImplementation> ParseAsync(IMultiplexSource input)
    {
        var header = await FieldParser.ReadFromAsync<CmapFormat14Parser>(input).CA();

        return await header.CreateImplementationAsync(input).CA();
       
    }

    private async ValueTask<CmapFormat14Implementation> CreateImplementationAsync(
        IMultiplexSource input)
    {
        var selectors = new VariantSelection[numVarSelectorRecords];
        for (int i = 0; i < selectors.Length; i++)
        {
            selectors[i] = await varSelectorRecords[i].
                CreateVariantSelectionAsync(input).CA();
        }
        return new(selectors);
    }
}

internal readonly partial struct VariantSelectorRecord
{
    [SFntField] private readonly UInt24 varSelector;
    [SFntField] private readonly uint defaultUVSOffset;
    [SFntField] private readonly uint nonDefaultUVSOffset;

    public async ValueTask<VariantSelection> CreateVariantSelectionAsync(IMultiplexSource input)
    {
        var defaults = defaultUVSOffset is > 0?
                await LoadDefaultTableAsync(input,defaultUVSOffset).CA():[];

        var mappings = nonDefaultUVSOffset is > 0?
                await LoadNonDefaultTableAsync(input,nonDefaultUVSOffset).CA():[];
        return new VariantSelection(
            varSelector, defaults, mappings);
    }


    private async ValueTask<UvsDefaultMapping[]> LoadDefaultTableAsync(IMultiplexSource input, long offset)
    {
        var record = await FieldParser.ReadFromAsync<DefaultUvsTableRecord>(input, offset).CA();
        return record.unicodeValueRanges;
    }

    private async ValueTask<UvsMapping[]> LoadNonDefaultTableAsync(IMultiplexSource input, long offset)
    {
        var record = await FieldParser.ReadFromAsync<NonDefaultUvsTableRecord>(input, offset).CA();
        return record.uvsMappings;
    }
}


internal readonly partial struct DefaultUvsTableRecord
{
    [SFntField] private readonly uint numUnicodeValueRanges;
    [SFntField("numUnicodeValueRanges")] public readonly UvsDefaultMapping[] unicodeValueRanges;
}


internal readonly partial struct NonDefaultUvsTableRecord
{
    [SFntField] private readonly uint numUvsMappings;
    [SFntField("numUvsMappings")] public readonly UvsMapping[] uvsMappings;
}