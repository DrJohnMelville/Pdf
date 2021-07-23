using System;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Visitors
{
    public abstract class RecursiveDescentVisitor<T>: ILowLevelVisitor<T>
    {
        protected virtual T VisitAny(PdfObject item) => default!;
        protected virtual T VisitNumber(PdfNumber item) => VisitAny(item);

        protected virtual T VisitDictionaryPair(PdfName key, PdfObject item)
        {
            key.Visit(this);
            return item.Visit(this);
        }
        
        public virtual T Visit(PdfBoolean item) => Visit((PdfTokenValues)item);
        public virtual T Visit(PdfTokenValues item) => VisitAny(item);
        public virtual T Visit(PdfIndirectObject item) => VisitAny(item);
        public virtual T Visit(PdfIndirectReference item) => VisitAny(item);
        public virtual T Visit(PdfName item) => VisitAny(item);
        public virtual T Visit(PdfInteger item) => VisitNumber(item);
        public virtual T Visit(PdfDouble item) => VisitNumber(item);
        public virtual T Visit(PdfString item) => VisitAny(item);
        public virtual T Visit(PdfStream item) => Visit((PdfDictionary)item);
        public virtual T Visit(PdfFreeListObject item) => VisitAny(item);
        
        public virtual T Visit(PdfArray item)
        {
            var ret = VisitAny(item);
            foreach (var element in item.RawItems)
            {
                element.Visit(this);
            }

            return ret;
        }

        public virtual T Visit(PdfDictionary item)
        {
            var ret = VisitAny(item);
            foreach (var pair in item.RawItems)
            {
                VisitDictionaryPair(pair.Key, pair.Value);
            }

            return ret;
        }
    }
}