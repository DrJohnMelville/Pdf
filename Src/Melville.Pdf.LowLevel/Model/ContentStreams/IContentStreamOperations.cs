namespace Melville.Pdf.LowLevel.Model.ContentStreams;

public interface IStateChangingCSOperations
{
    /// <summary>
    /// Content Stream Operator q
    /// </summary>
    void SaveGraphicsState();
    
    /// <summary>
    /// Content Stream Operator Q
    /// </summary>
    void RestoreGraphicsState();

    /// <summary>
    /// Content Stream Operator a b c d e f cm
    /// </summary>
    void ModifyTransformMatrix(double a, double b, double c, double d, double e, double f);

    /// <summary>
    /// Content stream operator lineWidth w
    /// </summary>
    void SetLineWidth(double width);

    /// <summary>
    /// Content stream operator J
    /// </summary>
    void SetLineCap(LineCap cap);
}

public interface IStatePreservingCSOperations
{
}

public interface IContentStreamOperations: IStateChangingCSOperations, IStatePreservingCSOperations
{
    
}