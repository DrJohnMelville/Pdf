using System.Buffers;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using Melville.Icc.Model.Tags;

namespace Melville.Icc.Model;

[DebuggerDisplay("Profile Tag {TagName}")]
public struct ProfileTag
{
    public uint Tag { get; init; }
    public string TagName => Visualizations.As4CC(Tag);
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

    public XyzNumber WhitePoint() => Header.Illuminant;
}