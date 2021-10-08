using System;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Model.Wrappers
{
    public interface IPdfTreeKey<T> : IComparable<T>
    {
    }
    public readonly struct PdfTree<T> where T:PdfObject, IPdfTreeKey<T>
    {
        public PdfName TreeItemsKey { get; }
    }

    public static class PdfTreeElementNamer
    {
        //Trees use a different name for the array in the leaf nodes depending on whether or
        // not they are name trees or number trees.  Since I cannot require static members in an
        // interface yet, I need an ugly hack like this so that the PdfTree struct can be
        // polymorphic with respect to the name used
        public static PdfName FinalArrayName<T>() =>
            typeof(T).IsAssignableTo(typeof(PdfNumber)) ? KnownNames.Nums : KnownNames.Names;
    }
}