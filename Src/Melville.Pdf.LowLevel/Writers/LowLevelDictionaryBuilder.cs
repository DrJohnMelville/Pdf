using System.Collections.Generic;
using Melville.Pdf.LowLevel.Model.LowLevel;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Writers
{
    public interface ILowLevelDictionaryBuilder
    {
        PdfIndirectReference CreateNewReference(PdfObject? value = null);
        void AssignValueToReference(PdfIndirectReference reference, PdfObject value);
        PdfIndirectReference Add(PdfObject item);
        PdfLowLevelDocument CreateDocument();
        void SetVersion(byte major, byte minor);
    }

    public class LowLevelDictionaryBuilder: ILowLevelDictionaryBuilder
    {
        private byte major = 1;
        private byte minor = 7;
        private int nextObject = 1;
        private List<PdfIndirectReference> objects = new List<PdfIndirectReference>();

        public void SetVersion(byte major, byte minor)
        {
            this.major = major;
            this.minor = minor;
        }

        public PdfIndirectReference CreateNewReference(PdfObject? value = null) => 
            new(new PdfIndirectObject(nextObject++, 0, value ?? PdfTokenValues.Null));

        public void AssignValueToReference(PdfIndirectReference reference, PdfObject value) => 
            ((ICanSetIndirectTarget)reference.Target).SetValue(value);

        public PdfLowLevelDocument CreateDocument()
        {
            throw new System.NotImplementedException();
        }
    }
}