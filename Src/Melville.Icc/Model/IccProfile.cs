using System.Buffers;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using Melville.Icc.Model.Tags;

namespace Melville.Icc.Model;

/// <summary>
/// Represents a single tag in an ICC profile
/// </summary>
[DebuggerDisplay("Profile Tag {TagName}")]
public struct ProfileTag
{
    /// <summary>
    /// Tag signature as listed in clause 10 of the ICC profile.
    /// </summary>
    public uint Tag { get; init; }
    /// <summary>
    /// Tag signature as a string
    /// </summary>
    public string TagName => Visualizations.As4CC(Tag);
    /// <summary>
    /// Offset to the beginning of the tag data in the ICC profile stream
    /// </summary>
    public uint Offset { get; init; }
    /// <summary>
    /// Length of the tag data in the ICC profile stream
    /// </summary>
    public uint Size { get; init; }
    /// <summary>
    /// Parsed representation of the tag -- if it is a recognized tag.
    /// </summary>
    public object? Data { get; init; }

    internal ProfileTag(uint tag, uint offset, uint size, object? data)
    {
        Tag = tag;
        Offset = offset;
        Size = size;
        Data = data;
    }
}

/// <summary>
/// Represents a parsed ICC profile.
/// </summary>
public class IccProfile
{
    /// <summary>
    /// Header structure that contains information about this profile
    /// </summary>
    public IccHeader Header { get; }
    /// <summary>
    /// A list of tags that define the profile operations or provide additional information\
    /// </summary>
    public IReadOnlyList<ProfileTag> Tags { get; }

    internal IccProfile(IccHeader header, IReadOnlyList<ProfileTag> tags)
    {
        Header = header;
        Tags = tags;
    }

    /// <summary>
    /// The white point for the profile connection space in this profile
    /// </summary>
    /// <returns>CIE XYZ values for the illuminant of the PCS, which will always be D50</returns>
    public XyzNumber WhitePoint() => Header.Illuminant;
}