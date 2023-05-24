using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Model.Wrappers.Trees;

/// <summary>
/// These are helper methods that allow various types of PDF trees to be searched with
/// the native C# types tather than having to convert to the corresponding PDF type.
/// </summary>
public static class SpecializedSearchMethods
{
    /// <summary>
    /// Search a name tree given a C# string.
    /// </summary>
    /// <param name="tree">The tree to search.</param>
    /// <param name="s">The desired key</param>
    /// <returns>The objet with the given key</returns>
    public static ValueTask<PdfObject> SearchAsync(this PdfTree<PdfString> tree, string s) =>
        tree.SearchAsync(PdfString.CreateAscii(s));

    /// <summary>
    /// Search a number tree given a C# integer.
    /// </summary>
    /// <param name="tree">The tree to search.</param>
    /// <param name="num">The desired key</param>
    /// <returns>The object with the given key</returns>
    public static ValueTask<PdfObject> SearchAsync(this PdfTree<PdfNumber> tree, int num) =>
        tree.SearchAsync(new PdfInteger(num));

    /// <summary>
    /// Search a number tree given a C# double.
    /// </summary>
    /// <param name="tree">The tree to search.</param>
    /// <param name="num">The desired key</param>
    /// <returns>The object with the given key</returns>
    public static ValueTask<PdfObject> SearchAsync(this PdfTree<PdfNumber> tree, double num) =>
        tree.SearchAsync(num);
}