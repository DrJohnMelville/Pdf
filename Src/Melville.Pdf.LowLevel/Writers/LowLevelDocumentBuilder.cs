using System;
using System.Collections.Generic;
using System.Linq;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.LowLevel;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Visitors;

namespace Melville.Pdf.LowLevel.Writers
{
    public interface ILowLevelDocumentBuilder
    {
        PdfIndirectReference AsIndirectReference(PdfObject? value = null);
        void AssignValueToReference(PdfIndirectReference reference, PdfObject value);
        PdfIndirectReference Add(PdfObject item);
        void AddToTrailerDictionary(PdfName key, PdfObject item);
        PdfLowLevelDocument CreateDocument();
        void SetVersion(byte major, byte minor);
    }

    public static class LowLevelDocumentBuilderOperations
    {
        public static void AddRootElement(
            this ILowLevelDocumentBuilder builder, PdfDictionary rootElt) =>
            builder.AddToTrailerDictionary(KnownNames.Root, builder.Add(rootElt));

        public static PdfDictionary NewDictionary(this ILowLevelDocumentBuilder _, params
            (PdfName Name, PdfObject Value)[] items) =>
            new(new Dictionary<PdfName, PdfObject>(
                items.Select(i => new KeyValuePair<PdfName, PdfObject>(i.Name, i.Value))));
    }

    public class LowLevelDocumentBuilder: ILowLevelDocumentBuilder
    {
        private byte major = 1;
        private byte minor = 7;
        private int nextObject = 1;
        private List<PdfIndirectReference> objects = new List<PdfIndirectReference>();
        private Dictionary<PdfName, PdfObject> trailerDictionaryItems = new();

        public void SetVersion(byte major, byte minor)
        {
            this.major = major;
            this.minor = minor;
        }

        public PdfIndirectReference AsIndirectReference(PdfObject? value = null) =>
            value switch
            {
                PdfIndirectReference pir => pir,
                PdfIndirectObject pio => new PdfIndirectReference(pio),
                _ => new(new PdfIndirectObject(nextObject++, 0, value ?? PdfTokenValues.Null))
            };

        public void AssignValueToReference(PdfIndirectReference reference, PdfObject value) => 
            ((ICanSetIndirectTarget)reference.Target).SetValue(value);

        public PdfIndirectReference Add(PdfObject item) => InnerAdd(AsIndirectReference(item));

        private PdfIndirectReference InnerAdd(PdfIndirectReference item)
        {
            objects.Add(item);
            return item;
        }

        public void AddToTrailerDictionary(PdfName key, PdfObject item) => 
            trailerDictionaryItems[key] = item;

        public PdfLowLevelDocument CreateDocument() => 
            new(major, minor, CreateTrailerDictionary(), CreateObjectList());

        private Dictionary<(int, int), PdfIndirectReference> CreateObjectList() => 
            objects.ToDictionary(item => (item.Target.ObjectNumber, item.Target.GenerationNumber));

        private PdfDictionary CreateTrailerDictionary()
        {
            AddLengthToTrailerDictionary();
            return new(new Dictionary<PdfName, PdfObject>(trailerDictionaryItems));
        }
        private void AddLengthToTrailerDictionary()
        {
            AddToTrailerDictionary(KnownNames.Length, new PdfInteger(TrailerLengthValue()));
        }
        private int TrailerLengthValue() => 1+objects.Max(i=>i.Target.ObjectNumber);
    }
}