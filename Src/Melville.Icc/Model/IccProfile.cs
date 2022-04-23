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

    public bool UseWhitePoint()
    {
        // I am not completely confident this is the correct rule for  when to use the media whote point
        // vs the illuminant white point.  Hopefully I will find some more profiles that will either
        // work or not work and give me more insight, because I cannot figure out the rule from the spec.
        foreach (var tag in Tags)
        {
            if ((TransformationNames)tag.Tag is
                TransformationNames.AtoB0 or
                TransformationNames.AtoB1 or
                TransformationNames.AtoB2 or
                TransformationNames.BtoA0 or
                TransformationNames.BtoA1 or
                TransformationNames.BtoA2
               ) return true;
        }

        return false;
    }

    public XyzNumber WhitePoint()
    {
        if (UseWhitePoint())
        {
            foreach (var tag in Tags)
            {
                if (tag.Tag is 0x77747074 && tag.Data is XyzArray xyzArr)
                    return xyzArr.Values[0];
            }
        }

        return Header.Illuminant;
        
    }

}