namespace Melville.Pdf.LowLevel.Model.Wrappers.ContentValueStreamUnions;

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