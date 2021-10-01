using System;
using System.Collections.Generic;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Writers.Builder
{
    public interface ILowLevelDocumentBuilder
    {
        PdfIndirectReference AsIndirectReference(PdfObject? value = null);
        void AssignValueToReference(PdfIndirectReference reference, PdfObject value);
        PdfIndirectReference Add(PdfObject item);
        public PdfIndirectReference Add(PdfObject item, int objectNumber, int generation);
        void AddToTrailerDictionary(PdfName key, PdfObject item);
        public PdfArray EnsureDocumentHasId();
        public byte[] UserPassword { get; set; }
    }

    public static class BuildEncryptedDocument
    {
        public static void AddEncryption(
            this ILowLevelDocumentBuilder builder,ILowLevelDocumentEncryptor encryptor)
        {
            builder.UserPassword = encryptor.UserPassword;
            builder.AddToTrailerDictionary(
                KnownNames.Encrypt,
                builder.Add(
                    encryptor.CreateEncryptionDictionary(builder.EnsureDocumentHasId())));
        }
        
    }
    
    public class LowLevelDocumentBuilder : ILowLevelDocumentBuilder
    {
        private int nextObject;
        public List<PdfIndirectReference> Objects { get;  }= new();
        private readonly Dictionary<PdfName, PdfObject> trailerDictionaryItems = new();

        public byte[] UserPassword { get; set; } = Array.Empty<byte>();

        public LowLevelDocumentBuilder(int nextObject = 1)
        {
            this.nextObject = nextObject;
        }
        
        public PdfIndirectReference AsIndirectReference(PdfObject? value = null) =>
            value switch
            {
                PdfIndirectReference pir => pir,
                PdfIndirectObject pio => new PdfIndirectReference(pio),
                _ => new(new PdfIndirectObject(nextObject++, 0, value ?? PdfTokenValues.Null))
            };

        public void AssignValueToReference(PdfIndirectReference reference, PdfObject value)
        {
            ((IMultableIndirectObject)reference.Target).SetValue(value);
        }
        public PdfIndirectReference Add(PdfObject item) => InnerAdd(AsIndirectReference(item));
        public PdfIndirectReference Add(PdfObject item, int objectNumber, int generation) => 
            InnerAdd(new PdfIndirectReference(new PdfIndirectObject(objectNumber, generation, item)));

        private PdfIndirectReference InnerAdd(PdfIndirectReference item)
        {
            Objects.Add(item);
            return item;
        }

        public void AddToTrailerDictionary(PdfName key, PdfObject item) => 
            trailerDictionaryItems[key] = item;
        
        public PdfDictionary CreateTrailerDictionary()
        {
            AddLengthToTrailerDictionary();
            return new(new Dictionary<PdfName, PdfObject>(trailerDictionaryItems));
        }
        private void AddLengthToTrailerDictionary()
        {
            AddToTrailerDictionary(KnownNames.Size, new PdfInteger(nextObject));
        }

        public PdfArray EnsureDocumentHasId() =>
            trailerDictionaryItems.TryGetValue(KnownNames.ID, out var val) && val is PdfArray ret
                ? ret
                : AddNewIdArray();

        private PdfArray AddNewIdArray()
        {
            var array = NewIdArray();
            AddToTrailerDictionary(KnownNames.ID, array);
            return array;
        }

        private PdfArray NewIdArray() => new(IdElement(), IdElement());

        private PdfString IdElement()
        {
            var ret = new byte[32];
            Guid.NewGuid().TryWriteBytes(ret);
            Guid.NewGuid().TryWriteBytes(ret.AsSpan(16));
            return new PdfString(ret);
        }

    }
}