using System.Collections.Generic;
using System.Linq;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Document;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Writers.Builder
{
    public interface ILowLevelDocumentCreator: ILowLevelDocumentBuilder
    {
        PdfLowLevelDocument CreateDocument();
        void SetVersion(byte major, byte minor);
    }

    public partial class LowLevelDocumentCreator: ILowLevelDocumentCreator
    {
        private byte major = 1;
        private byte minor = 7;
        private readonly LowLevelDocumentBuilder data = new LowLevelDocumentBuilder(1);
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