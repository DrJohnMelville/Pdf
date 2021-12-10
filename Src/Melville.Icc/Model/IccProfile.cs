using System.Buffers;

namespace Melville.Icc.Model;

public struct ProfileTag
{
    public uint Tag { get; init; }
    public uint Offset { get; init; }
    public uint Size { get; init; }
    public object? Data { get; init; }

    public ProfileTag(uint tag, uint offset, uint size, object? data)
    {
        Tag = tag;
        Offset = offset;
        Size = size;
        Data = data;
    }
}

public class IccProfile
{
    public IccHeader Header { get; }
    public IReadOnlyList<ProfileTag> Tags { get; }

    public IccProfile(IccHeader header, IReadOnlyList<ProfileTag> tags)
    {
        Header = header;
        Tags = tags;
    }
}