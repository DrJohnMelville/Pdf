using System;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.LowLevel.Model.Conventions
{
    public static partial class KnownNames
    {
        private static readonly NameDictionay allKnownNames = new NameDictionay();

        static KnownNames()
        {
            AddItemsToDict(allKnownNames);
        }

         private static void AddSynonym(NameDictionay dict, PdfName item, byte[] synonym) => 
             dict.AddSynonym(synonym, item);

         public static PdfName Get(ReadOnlySpan<byte> nameText) => allKnownNames.GetOrCreate(nameText);

         public static PdfName Get(string nameText)
         {
             Span<byte> span = stackalloc byte[nameText.Length];
             ExtendedAsciiEncoding.EncodeToSpan(nameText, span);
             return Get(span);
         }
    }
}