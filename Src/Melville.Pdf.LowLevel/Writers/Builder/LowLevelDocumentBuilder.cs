using System.Collections.Generic;
using System.Linq;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.Conventions;
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
    public interface ILowLevelDocumentCreator: ILowLevelDocumentBuilder
    {
        PdfLowLevelDocument CreateDocument();
        void SetVersion(byte major, byte minor);
    }

    public interface ILowLevelDocumentModifier
    {
        
    }

    public partial class DocumentCreationData : ILowLevelDocumentBuilder
    {
        private int nextObject;
        public List<PdfIndirectReference> Objects { get;  }= new();
        public Dictionary<PdfName, PdfObject> TrailerDictionaryItems { get; }= new();

        public DocumentCreationData(int nextObject)
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

    public partial class LowLevelDocumentCreator: ILowLevelDocumentCreator
    {
        private byte major = 1;
        private byte minor = 7;
        private readonly DocumentCreationData data = new DocumentCreationData(1);
        [DelegateTo]
        private ILowLevelDocumentBuilder Builder => data;
    
        public void SetVersion(byte major, byte minor)
        {
            this.major = major;
            this.minor = minor;
        }

        public PdfLowLevelDocument CreateDocument() => 
            new(major, minor, CreateTrailerDictionary(), CreateObjectList());

        private Dictionary<(int, int), PdfIndirectReference> CreateObjectList() => 
            data.Objects.ToDictionary(item => (item.Target.ObjectNumber, item.Target.GenerationNumber));

        private PdfDictionary CreateTrailerDictionary()
        {
            AddLengthToTrailerDictionary();
            return new(new Dictionary<PdfName, PdfObject>(data.TrailerDictionaryItems));
        }
        private void AddLengthToTrailerDictionary()
        {
            data.AddToTrailerDictionary(KnownNames.Size, new PdfInteger(TrailerLengthValue()));
        }
        private int TrailerLengthValue() => 1+data.Objects.Max(i=>i.Target.ObjectNumber);
    }
}