namespace Melville.Pdf.LowLevel.Model.Wrappers.ContentValueStreamUnions;

/// <summary>
/// This enum defines which field in the ContentStreamValueUnion has real data in it.
/// </summary>
public enum ContentStreamValueType
{
    /// <summary>
    /// The value is a C# object.
    /// </summary>
    Object, 
    /// <summary>
    /// The value value is a number
    /// </summary>
    Number, 
    /// <summary>
    /// The value is a Memory&lt;byte&gt;
    /// </summary>
    Memory
}