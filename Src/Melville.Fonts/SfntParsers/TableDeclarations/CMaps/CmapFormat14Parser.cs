﻿using System.IO.Pipelines;
using System.Security.AccessControl;
using Melville.Fonts.SfntParsers.TableParserParts;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.CountingReaders;
using Melville.Parsing.MultiplexSources;

namespace Melville.Fonts.SfntParsers.TableDeclarations.CMaps;

internal readonly partial struct CmapFormat14Parser
{
    [SFntField] private readonly ushort format;
    [SFntField] private readonly uint length;
    [SFntField] private readonly uint numVarSelectorRecords;
    [SFntField("numVarSelectorRecords")] private readonly VariantSelectorRecord[] varSelectorRecords;
    public static async ValueTask<ICmapImplementation> ParseAsync(IMultiplexSource input)
    {
        using var pipe = input.ReadPipeFrom(0);
        var header = await FieldParser.ReadFromAsync<CmapFormat14Parser>(
            pipe).CA();

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
                await LoadDefaultTableAsync(input.ReadPipeFrom(defaultUVSOffset)).CA():[];
        using var pipe = input.ReadPipeFrom(nonDefaultUVSOffset);
        var mappings = nonDefaultUVSOffset is > 0?
                await LoadNonDefaultTableAsync(pipe).CA():[];
        return new VariantSelection(
            varSelector, defaults, mappings);
    }


    private async ValueTask<UvsDefaultMapping[]> LoadDefaultTableAsync(IByteSource pipe)
    {
        var record = await FieldParser.ReadFromAsync<DefaultUvsTableRecord>(pipe).CA();
        return record.unicodeValueRanges;
    }
    private async ValueTask<UvsMapping[]> LoadNonDefaultTableAsync(IByteSource readPipeFrom)
    {
        var record = await FieldParser.ReadFromAsync<NonDefaultUvsTableRecord>(
            readPipeFrom).CA();
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