using System.IO.Pipelines;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.MultiplexSources;

namespace Melville.Fonts.SfntParsers.TableDeclarations.CMaps;


internal class ParsedCmap(IMultiplexSource source, CmapTablePointer[] subtables): ICMapSource
{
    public IReadOnlyList<CmapTablePointer> Tables => subtables;

    public ValueTask<ICmapImplementation> GetSubtableAsync(CmapTablePointer pointer)
    {
        return GetSubtableAsync(source.ReadPipeFrom(pointer.Offset));
    }

    private async ValueTask<ICmapImplementation> GetSubtableAsync(PipeReader input)
    {
        var tag = await input.PeekTagAsync(2).CA();
        return tag switch
        {
            0 =>  await CmapFormat0Parser.ParseAsync(input).CA(),
            2 => await CmapFormat2Parser.ParseAsync(input).CA(),
            4 => await CmapFormat4Parser.ParseAsync(input).CA(),
            6 => await CmapFormat6Parser.ParseAsync(input).CA(),
            8 => throw new NotSupportedException("""
            Type 8 true type CMAPs are discouraged by the spec and not supported by this library.
            I cannot find a type 8 CMAP for testing.  If you hit this bug, send me the file
            and I will support reading it.
            """),
            10 => await CmapFormat10Parser.ParseAsync(input).CA(),
            12 or 13 => await CmapFormat12Parser.ParseAsync(input).CA(),
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