using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Model.Wrappers.Trees;

public static class SpecializedSearchMethods
{
    public static ValueTask<PdfObject> Search(this PdfTree<PdfString> tree, string s) =>
        tree.Search(PdfString.CreateAscii(s));
    public static ValueTask<PdfObject> Search(this PdfTree<PdfNumber> tree, int num) =>
        tree.Search(new PdfInteger(num));
    public static ValueTask<PdfObject> Search(this PdfTree<PdfNumber> tree, double num) =>
        tree.Search(num);
}