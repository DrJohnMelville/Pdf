using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Visitors;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Melville.Pdf.LowLevel.Model.Objects;

/// <summary>
/// Root of the PdfObject hierarchy.  This is the root of all the types that can appear in a pdf file.
/// </summary>
public abstract class PdfObject
{
    /// <summary>
    /// Get the actual value of this PdfObject.  All PdfObjects except PdfIndirectReference return themselves.
    /// Indirect reference objects return the referred to object -- and may need to touch the disk to get it.
    /// </summary>
    /// <returns>Either this object or the object this object refers to.</returns>
    public virtual ValueTask<PdfObject> DirectValueAsync() => new(this);

    /// <summary>
    /// Invoke the given visitor object on this pdf object.
    /// </summary>
    /// <typeparam name="T">The return type of the visitor object</typeparam>
    /// <param name="visitor"></param>
    /// <returns></returns>
    public T InvokeVisitor<T>(ILowLevelVisitor<T> visitor) => Visit(visitor);

    // by making this internal we hide the visitor mechanism from our consumers.  Incidentally,
    // so long as all the classes that implement this method are sealed, then other assemblies cannot
    // declare children of PdfObject because the will be unable to implement this internal method.
    internal abstract T Visit<T>(ILowLevelVisitor<T> visitor);

    /// <summary>
    /// Create a PdfDouble from a C# double
    /// </summary>
    /// <param name="value">The desired C# value</param>
    public static implicit operator PdfObject(double value) => (PdfDouble)value;

    /// <summary>
    /// Create a PdfInteger from a C# integer
    /// </summary>
    /// <param name="value">The desired C# value</param>
    public static implicit operator PdfObject(int value) => (PdfInteger)value;

    /// <summary>
    /// Create a PdfBoolean from a bool.
    /// </summary>
    /// <param name="value">The value of the desired PDF boolean</param>
    public static implicit operator PdfObject(bool value) => (PdfBoolean)value;

    /// <summary>
    /// Create a PdfString from a C# string
    /// </summary>
    /// <param name="value">The desired C# value</param>
    public static implicit operator PdfObject(string value) => (PdfString)value;
}