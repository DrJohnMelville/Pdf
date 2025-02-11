namespace Melville.Pdf.ReferenceDocuments.Text.OtherFontTypes
{

    public class SimplifiedChinese() : CjkBase("Simplified Chinese using the GB1 character set",
        "UniGB-UCS2-H", "GB1",
        "\x59\x2A\x53\x9F\x5E\x02\x6C\x47\x90\x1A\x51\x74\x67\x3A\x68\xB0\x52\x36\x90\x20\x67\x09\x96\x50\x51\x6C\x53\xF8")
    {
    }
    public class TraditionalChinese() : CjkBase("Traditional Chinese using the GB1 character set",
        "UniCNS-UCS2-H", "CNS1",
        "\x59\x2A\x53\x9F\x5E\x02\x6C\x47\x90\x1A\x51\x74\x67\x3A\x68\xB0\x52\x36\x90\x20\x67\x09\x96\x50\x51\x6C\x53\xF8")
    {
    }
    public class Japan1() : CjkBase("Japanese using the Japan1 character set",
        "UniJIS-UTF16-H", "Japan1",
        "\x59\x2A\x53\x9F\x5E\x02\x6C\x47\x90\x1A\x51\x74\x67\x3A\x68\xB0\x52\x36\x90\x20\x67\x09\x96\x50\x51\x6C\x53\xF8")
    {
    }
    public class Korea1() : CjkBase("Korean using the Korea1 character set",
        "UniKS-UTF16-H", "Korea1",
        "\x59\x2A\x53\x9F\x5E\x02\x6C\x47\x90\x1A\x51\x74\x67\x3A\x68\xB0\x52\x36\x90\x20\x67\x09\x96\x50\x51\x6C\x53\xF8")
    {
    }
    public abstract class CjkBase : FontDefinitionTest
    {
        private string outerMapping;
        private string innerOrdering;
        protected CjkBase(string description, string outerMapping, string innerOrdering, string text) : base(description)
        {
            this.outerMapping = outerMapping;
            this.innerOrdering = innerOrdering;
            TextToRender = text;
        }

        protected override PdfDirectObject CreateFont(IPdfObjectCreatorRegistry arg)
        {
            var CIDFont = arg.Add(CidFontBuilder(arg).AsDictionary());
            return new DictionaryBuilder()
                .WithItem(KnownNames.Type, KnownNames.Font)
                .WithItem(KnownNames.Subtype, KnownNames.Type0)
                .WithItem(KnownNames.BaseFont, PdfDirectObject.CreateName("AAAA+STSong-Light"))
                .WithItem(KnownNames.Encoding, PdfDirectObject.CreateName(outerMapping))
                .WithItem(KnownNames.DescendantFonts, new PdfArray(CIDFont))
                .AsDictionary();
        }

        protected virtual DictionaryBuilder CidFontBuilder(IPdfObjectCreatorRegistry arg)
        {
            var descrip = arg.Add(new DictionaryBuilder()
                .WithItem(KnownNames.Type, KnownNames.FontDescriptor)
                .WithItem(KnownNames.Flags, 32)
                .WithItem(KnownNames.FontBBox,
                    new PdfArray(-511, -250, 1390, 750))
                .AsDictionary());
            var sysinfo = arg.Add(new DictionaryBuilder()
                .WithItem(KnownNames.Registry, "Adobe")
                .WithItem(KnownNames.Ordering, innerOrdering)
                .WithItem(KnownNames.Supplement, 4)
                .AsDictionary()
            );
            var CIDFontBuilder = new DictionaryBuilder()
                .WithItem(KnownNames.Type, KnownNames.Font)
                .WithItem(KnownNames.Subtype, KnownNames.CIDFontType0)
                .WithItem(KnownNames.FontDescriptor, descrip)
                .WithItem(KnownNames.BaseFont, PdfDirectObject.CreateName("STSong-Light"))
                .WithItem(KnownNames.CIDSystemInfo, sysinfo);
            return CIDFontBuilder;
        }
    }
}