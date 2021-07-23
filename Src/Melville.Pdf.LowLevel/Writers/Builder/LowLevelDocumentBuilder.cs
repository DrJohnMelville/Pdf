using System.Collections.Generic;
using Melville.Pdf.LowLevel.Model.Document;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Writers.Builder
{
    public interface ILowLevelDocumentBuilder
    {
        PdfIndirectReference AsIndirectReference(PdfObject? value = null);
        void AssignValueToReference(PdfIndirectReference reference, PdfObject value);
        PdfIndirectReference Add(PdfObject item);
        void AddToTrailerDictionary(PdfName key, PdfObject item);
    }
    
    public partial class LowLevelDocumentBuilder : ILowLevelDocumentBuilder
    {
        private int nextObject;
        public List<PdfIndirectReference> Objects { get;  }= new();
        public Dictionary<PdfName, PdfObject> TrailerDictionaryItems { get; }= new();

        public LowLevelDocumentBuilder(int nextObject)
        {
            this.nextObject = nextObject;
        }
        
        public PdfIndirectReference AsIndirectReference(PdfObject? value = null) =>
            value switch
            {
                PdfIndirectReference pir => pir,
                PdfIndirectObject pio => new PdfIndirectReference(pio),
                _ => new(new PdfMutableIndirectObject(nextObject++, 0, value ?? PdfTokenValues.Null))
            };

        public void AssignValueToReference(PdfIndirectReference reference, PdfObject value)
        {
            ((PdfMutableIndirectObject)reference.Target).SetValue(value);
        }
        public PdfIndirectReference Add(PdfObject item) => InnerAdd(AsIndirectReference(item));

        private PdfIndirectReference InnerAdd(PdfIndirectReference item)
        {
            Objects.Add(item);
            return item;
        }

        public void AddToTrailerDictionary(PdfName key, PdfObject item) => 
            TrailerDictionaryItems[key] = item;
    }
}