using System;
using System.Runtime.CompilerServices;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.LowLevel.Model.Conventions
{
    public static class NameDirectory
    {
        private static readonly NameDictionay allKnownNames = new ();

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
        [ModuleInitializer]
        internal static void EnsureAllItesDeclared()
        {
            // we want to declare all the standard names first to ensure that our simple allocator for subtyped
            // names does not collide with a parsed version of the same name.
            AddItemsToDict();
        }
    }
}