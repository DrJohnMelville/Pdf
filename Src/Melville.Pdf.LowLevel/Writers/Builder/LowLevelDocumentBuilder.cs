using System.Collections.Generic;
using System.Linq;
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
            new(PairsToDictionary(items));

        private static Dictionary<PdfName, PdfObject> PairsToDictionary(
            IEnumerable<(PdfName Name, PdfObject Value)> items) =>
            new(
                items.Select(i => new KeyValuePair<PdfName, PdfObject>(i.Name, i.Value)));


        public static PdfStream NewStream(this ILowLevelDocumentBuilder _, string streamData, params
            (PdfName Name, PdfObject Value)[] items) =>
            new(StreamDictionary(items, streamData.Length), new LiteralStreamSource(streamData));

        private static Dictionary<PdfName, PdfObject> StreamDictionary(
            (PdfName Name, PdfObject Value)[] items, int length)
        {
            return PairsToDictionary(items.Append((KnownNames.Length, new PdfInteger(length))));
        }

        public static PdfStream NewStream(this ILowLevelDocumentBuilder _, byte[] streamData, params
            (PdfName Name, PdfObject Value)[] items) =>
            new(StreamDictionary(items, streamData.Length), new LiteralStreamSource(streamData));
    }

    public partial class LowLevelDocumentBuilder: ILowLevelDocumentBuilder
    {
        private byte major = 1;
        private byte minor = 7;
        private int nextObject = 1;
        private List<PdfIndirectReference> objects = new();
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
                _ => new(new LowLevelDocumentBuilder.PdfMutableIndirectObject(nextObject++, 0, value ?? PdfTokenValues.Null))
            };

        public void AssignValueToReference(PdfIndirectReference reference, PdfObject value)
        {
            ((LowLevelDocumentBuilder.PdfMutableIndirectObject)reference.Target).SetValue(value);
        }
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
            AddToTrailerDictionary(KnownNames.Size, new PdfInteger(TrailerLengthValue()));
        }
        private int TrailerLengthValue() => 1+objects.Max(i=>i.Target.ObjectNumber);
    }
}