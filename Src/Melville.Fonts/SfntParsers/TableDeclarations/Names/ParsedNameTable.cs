using System.Buffers;
using System.Text;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.CountingReaders;
using Melville.Parsing.MultiplexSources;

namespace Melville.Fonts.SfntParsers.TableDeclarations.Names;

/// <summary>
/// Represents a name in the Names table
/// </summary>
/// <param name="NameId">Enum of what kind of name this is</param>
/// <param name="PlatformId">Platform</param>
/// <param name="EncodingId">Encoding</param>
/// <param name="LanguageId">Language</param>
/// <param name="Value">The value of the string</param>
public record NameTableLine (
        SfntNameKey NameId, ushort PlatformId, ushort EncodingId, ushort LanguageId,
        string Value);

/// <summary>
/// Can show all the strings in the low level UI
/// </summary>
public interface INameTableView
{
    /// <summary>
    /// get all strings out of the table.
    /// </summary>
    /// <returns>An array of table lines</returns>
    public ValueTask<NameTableLine[]> GetAllNamesAsync();
}

internal class ParsedNameTable(IMultiplexSource source, NameTableHeader header):
    INameTableView
{
    public async ValueTask<string?> GetNameAsync(SfntNameKey nameId, 
        ushort desiredPlatform = 0xFFFF)
    {
        foreach (var item in header.Items)
        {
            if (item.Matches(nameId, desiredPlatform))
            {
                return await StringForItemAsync(item).CA();
            }
        }

        return null;
    }

    private async Task<string> StringForItemAsync(NameRecord item)
    {
        using var pipe = source.ReadPipeFrom(item.Offset + header.StorageOffset);
        var result = await pipe.ReadAtLeastAsync(item.Length).CA();
        return DecodeString(item.PlatformId, result.Buffer.Slice(0, item.Length));
    }

    private string DecodeString(int platform, ReadOnlySequence<byte> slice)
    {
        if (slice.IsSingleSegment)
            return DecodeString(platform, slice.First.Span);
        Span<byte> buffer = stackalloc byte[(int)slice.Length];
        new SequenceReader<byte>(slice).TryCopyTo(buffer);
        return DecodeString(platform, buffer);
    }

    private string DecodeString(int platform, ReadOnlySpan<byte> input) =>
        platform == 1?
            Encoding.UTF8.GetString(input):
            Encoding.BigEndianUnicode.GetString(input);

    public async ValueTask<NameTableLine[]> GetAllNamesAsync()
    {
        var ret = new NameTableLine[header.Items.Length];
        for (int i = 0; i < ret.Length; i++)
        {
            var line = header.Items[i];
            ret[i] = new NameTableLine(line.NameId, line.PlatformId, line.EncodingId,
                line.LanguageId, 
                await StringForItemAsync(line).CA() );
        }

        return ret;
    }
}