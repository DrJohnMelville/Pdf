using System.IO.Pipelines;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.MultiplexSources;

namespace Melville.Fonts.SfntParsers.TableDeclarations.CMaps;


/// <summary>
/// This class represents a Cmap from a font file.  You can inspect the tables available and
/// request one
/// </summary>
/// <param name="source">MultiplexSource containing the Cmap</param>
/// <param name="subtables">The CmapTables used</param>
public class ParsedCmap(IMultiplexSource source, CmapTablePointer[] subtables): ICMapSource
{
    /// <summary>
    /// CMaps offered by this font
    /// </summary>
    public IReadOnlyList<CmapTablePointer> Tables => subtables;

    /// <summary>
    /// Retrieve a CMapImplementation for the given subtable pointer
    /// </summary>
    /// <param name="pointer">A pointer from the Tables array to a subtable</param>
    /// <returns>An ICMapImplementation that executes the given cmap</returns>
    public ValueTask<ICmapImplementation> GetSubtableAsync(CmapTablePointer pointer)
    {
        return GetSubtableAsync(source.ReadPipeFrom(pointer.Offset));
    }

    private async ValueTask<ICmapImplementation> GetSubtableAsync(PipeReader input)
    {
        var tag = await input.PeekTag(2).CA();
        return tag switch
        {
            0 =>  await CmapFormat0Parser.ParseAsync(input).CA(),
            2 => await CmapFormat2Parser.ParseAsync(input).CA(),
            4 => await CmapFormat4Parser.ParseAsync(input).CA(),
            _ => throw new InvalidDataException($"Unknown Cmap format {tag}")
        };
    }

    /// <inheritdoc />
    public int Count => Tables.Count;

    /// <inheritdoc />
    public ValueTask<ICmapImplementation> GetByIndexAsync(int index)
    {
        return GetSubtableAsync(Tables[index]);
    }

    /// <inheritdoc />
    public ValueTask<ICmapImplementation?> GetByPlatformEncodingAsync(int platform, int encoding)
    {
        foreach (var table in Tables)
        {
            if (table.PlatformId == platform && table.EncodingId == encoding)
            {
                return GetSubtableAsync(table)!;
            }
        }
        return new((ICmapImplementation?)null);
    }

    /// <inheritdoc />
    public (int platform, int encoding) GetPlatformEncoding(int index) => 
        (Tables[index].PlatformId, Tables[index].EncodingId);
}