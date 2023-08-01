using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects2;

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
    public static ValueTask<PdfDirectValue> SearchAsync(this PdfTree tree, string s) =>
        tree.SearchAsync(PdfDirectValue.CreateString(s.AsExtendedAsciiBytes()));

    /// <summary>
    /// Search a number tree given a C# integer.
    /// </summary>
    /// <param name="tree">The tree to search.</param>
    /// <param name="num">The desired key</param>
    /// <returns>The object with the given key</returns>
    public static ValueTask<PdfDirectValue> SearchAsync(this PdfTree tree, int num) =>
        tree.SearchAsync(num);

    /// <summary>
    /// Search a number tree given a C# double.
    /// </summary>
    /// <param name="tree">The tree to search.</param>
    /// <param name="num">The desired key</param>
    /// <returns>The object with the given key</returns>
    public static ValueTask<PdfDirectValue> SearchAsync(this PdfTree tree, double num) =>
        tree.SearchAsync(num);
}