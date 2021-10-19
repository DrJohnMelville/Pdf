using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.LowLevel.Model.Conventions
{
    public static partial class KnownNames
    {
        private static readonly Dictionary<int, PdfName> allKnownNames;

        static KnownNames()
        {
            allKnownNames = CreateFilledDictionary();
        }

        private static Dictionary<int, PdfName> CreateFilledDictionary()
        {
            var ret = new Dictionary<int, PdfName>();
            AddItemsToDict(ret);
            return ret;
        }

        public static bool LookupName(uint key, [NotNullWhen(true)] out PdfName? name) => 
            allKnownNames.TryGetValue((int)key, out name);

        // this is private internal class because KnownNames ensures that only one KnownPdfName
        // is created for each value.  This makes KnownPdfNames have object identity.  This would
        // be broken if clients could make their own KnownPdfNames
        private class KnownPdfName : PdfName
        {
            public KnownPdfName(byte[] name) : base(name, true)
            {
            }

            public override bool Equals(object? obj) => ReferenceEquals(this, obj);
            public override int GetHashCode() => base.GetHashCode(); // fix a spurious warning
        }

        private class KnownPdfSynonym : KnownPdfName
        {
            private readonly PdfName preferred;
            public override PdfName PreferredName() => preferred;
            public KnownPdfSynonym(byte[] name, PdfName preferred) : base(name)
            {
                this.preferred = preferred;
            }
        }

        private static void AddTo(Dictionary<int, PdfName> dict, PdfName item) => 
            dict.Add(item.GetHashCode(), item);

        private static void AddSynonym(Dictionary<int, PdfName> dict, PdfName item, byte[] synonym) => 
            AddTo(dict, new KnownPdfSynonym(synonym, item));
    }
}