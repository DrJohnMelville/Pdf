using System.Diagnostics.CodeAnalysis;
using Melville.Icc.ColorTransforms;
using Melville.Icc.Model.Tags;

namespace Melville.Icc.Model;

public ref struct TrcTransformParser
{
    XyzArray? rXYZ = null, gXYZ = null, bXYZ = null;
    ICurveTag? rTRC = null, gTRC = null, bTRC = null;

    public TrcTransformParser(IccProfile profile)
    {
        ReadProfile(profile);
    }

    private void ReadProfile(IccProfile profile)
    {
        foreach (var tag in profile.Tags)
        {
            ClassifyTag(tag);
        }
    }

    private void ClassifyTag(ProfileTag tag)
    {
        switch ((TransformationNames)tag.Tag)
        {
            case TransformationNames.rXYZ:
                rXYZ = tag.Data as XyzArray;
                break;
            case TransformationNames.gXYZ:
                gXYZ = tag.Data as XyzArray;
                break;
            case TransformationNames.bXYZ:
                bXYZ = tag.Data as XyzArray;
                break;
            case TransformationNames.rTRC:
                rTRC = tag.Data as ICurveTag;
                break;
            case TransformationNames.gTRC:
                gTRC = tag.Data as ICurveTag;
                break;
            case TransformationNames.bTRC:
                bTRC = tag.Data as ICurveTag;
                break;
        }
    }

    public IColorTransform? Create() =>
        IsValid() ? new TrcTransform(
            CreateMatrix(rXYZ.Values[0], gXYZ.Values[0], bXYZ.Values[0]), rTRC, gTRC, bTRC) : null;


    [MemberNotNullWhen(true, "rXYZ", "gXYZ", "bXYZ", "rTRC", "gTRC", "bTRC")]
    private bool IsValid() =>
        rXYZ is { Values.Count: 1 } &&
        gXYZ is { Values.Count: 1 } &&
        bXYZ is { Values.Count: 1 } &&
        rTRC is not null &&
        gTRC is not null &&
        bTRC is not null;

    private Matrix3x3 CreateMatrix(XyzNumber redMatrixCol, XyzNumber greenMatrixCol, XyzNumber blueMatrixCol) =>
        //this code intentionally mirrors Annex F.3 in ICC.1:2010 color standard
        new(
            redMatrixCol.X, greenMatrixCol.X, blueMatrixCol.X,
            redMatrixCol.Y, greenMatrixCol.Y, blueMatrixCol.Y,
            redMatrixCol.Z, greenMatrixCol.Z, blueMatrixCol.Z
        );
}