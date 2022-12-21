namespace Melville.Icc.Model.Tags;

internal interface ICurveSegment: ICurveTag
{
    void Initialize(float minimum, float maximum, float valueAtMinimum);
}