using System;
using System.Runtime.CompilerServices;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.LowLevel.Model.Conventions
{
    public static class NameDirectory
    {
        private static readonly NameDictionay allKnownNames = new ();

        static NameDirectory()
        {
            KnownNames.ForceInitialization();
        }
        public static T ForceAdd<T>(T item) where T:PdfName
        {
            allKnownNames.ForceAdd(item.Bytes, item);
            return item;
        }

        public static void AddSynonym(PdfName item, byte[] synonym) => 
            allKnownNames.AddSynonym(synonym, item);

        public static PdfName Get(ReadOnlySpan<byte> nameText) => allKnownNames.GetOrCreate(nameText);

        public static PdfName Get(string nameText)
        {
            Span<byte> span = stackalloc byte[nameText.Length];
            ExtendedAsciiEncoding.EncodeToSpan(nameText, span);
            return Get(span);
        }
        
    }
    public static partial class KnownNames
    {
        static KnownNames()
        {
            AddItemsToDict();
        }

        public static void ForceInitialization()
        {
            //calling any method of a static class calls the static constructor if necessary
        }
    }
}