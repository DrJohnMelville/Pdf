using Melville.INPC;
using Melville.Pdf.FontLibrary;
using Melville.Pdf.LowLevel.Writers.ContentStreams;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.ReferenceDocuments.Graphics;

namespace Melville.Pdf.ReferenceDocuments.Text.CharSet.CMaps
{
    public abstract partial class BuiltinCMaps : Card3x5
    {
        protected BuiltinCMaps() : base("Show CMap")
        {
        }

        //Generic
        [MacroItem("Identity_")]
        // simplified chineses
        [MacroItem("GB_EUC_")]
        [MacroItem("GBpc_EUC_")]
        [MacroItem("GBK_EUC_")]
        [MacroItem("GBKp_EUC_")]
        [MacroItem("GBK2K_")]
        [MacroItem("UniGB_UCS2_")]
        [MacroItem("UniGB_UTF16_")]
        // traditional chineese
        [MacroItem("B5pc_")]
        [MacroItem("HKscs_B5_")]
        [MacroItem("ETen_B5_")]
        [MacroItem("ETenms_B5_")]
        [MacroItem("CNS_EUC_")]
        [MacroItem("UniCNS_UCS2_")]
        [MacroItem("UniCNS_UTF16_")]
        //japanese
        [MacroItem("_83pv_RKJS_")]
        [MacroItem("_90ms_RKJS_")]
        [MacroItem("_90msp_RKJS_")]
        [MacroItem("_90pv_RKJS_")]
        [MacroItem("Add_RKJS_")]
        [MacroItem("EUC_")]
        [MacroItem("Ext_RKSJ_")]
        [MacroItem("")]
        [MacroItem("UniJIS_UCS2_")]
        [MacroItem("UniJIS_UCS2_HW_")]
        [MacroItem("UniJIS_UTF16_")]
        //Korean
        [MacroItem("KSC_EUC_")]
        [MacroItem("KSCms_UHC_")]
        [MacroItem("KSCms_UHC_HW_")]
        [MacroItem("KSCpc_EUC_HW_")] // vertical version not mentioned in spec
        [MacroItem("UniKS_UCS2_")]
        [MacroItem("UniKS_UTF16_")]
        //every encoding comes in horizontal and vertical versions.
        [MacroCode("public class ~0~H : BuiltinCMaps { }")]
        [MacroCode("public class ~0~V : BuiltinCMaps { }")]
        protected override void SetPageProperties(PageCreator page)
        {
            page.AddResourceObject(ResourceTypeName.Font, fontName, CreateCIDFont);
        }

        private PdfIndirectObject CreateCIDFont(IPdfObjectCreatorRegistry register)
        {
            var fontStream = register.Add(new DictionaryBuilder()
                .WithItem(KnownNames.Subtype, KnownNames.OpenType)
                .AsStream(SelfContainedDefaultFonts.Instance
                    .FontFromName(KnownNames.TimesRoman, FontFlags.Serif).Source));
            var cidSystemInfo = register.Add(new   DictionaryBuilder()
                .WithItem(KnownNames.Ordering, "Identity")
                .WithItem(KnownNames.Registry, "Adobe")
                .WithItem(KnownNames.Supplement, 1)
                .AsDictionary());
            var fontDescriptor= register.Add(new   DictionaryBuilder()
                .WithItem(KnownNames.Flags, 34)
                .WithItem(KnownNames.FontBBox, PdfDirectObject.FromArray(-8, -145, 1000, 859))
                .WithItem(KnownNames.FontName, PdfDirectObject.CreateName("CourierNew"))
                .WithItem(KnownNames.FontWeight, 400)
                .WithItem(KnownNames.Type, KnownNames.FontDescriptor)
                .WithItem(KnownNames.FontFile3, fontStream)
                .AsDictionary());
            var cidFont= register.Add(new DictionaryBuilder()
                .WithItem(KnownNames.Type, KnownNames.Font)
                .WithItem(KnownNames.Subtype, KnownNames.CIDFontType0)
                .WithItem(KnownNames.BaseFont, PdfDirectObject.CreateName("CourierNew"))
                .WithItem(KnownNames.CIDSystemInfo, cidSystemInfo)
                .WithItem(KnownNames.FontDescriptor, fontDescriptor)
                .WithItem(KnownNames.DW, 1000)
                .AsDictionary());
            return register.Add(new DictionaryBuilder()
                .WithItem(KnownNames.Type, KnownNames.Font)
                .WithItem(KnownNames.Subtype, KnownNames.Type0)
                .WithItem(KnownNames.Name, PdfDirectObject.CreateName("CourierNew"))
                .WithItem(KnownNames.Encoding, CreateEncodingFromClass())
                .WithItem(KnownNames.DescendantFonts, PdfDirectObject.FromArray(cidFont))
                .AsDictionary());
        }

        private PdfIndirectObject CreateEncodingFromClass()
        {
            return PdfDirectObject.CreateName(
                GetType().Name
                    .TrimStart('_')
                    .Replace("_", "-"));
        }

        private static readonly PdfDirectObject fontName = PdfDirectObject.CreateName("F1");
        protected override async ValueTask DoPaintingAsync(ContentStreamWriter csw)
        {
            using (var tr = csw.StartTextBlock())
            {
                tr.SetTextMatrix(1, 0, 0, 1, 30, 200);
                csw.SetTextLeading(12);
                await csw.SetFontAsync(fontName, 12);
                foreach (var root in Enumerable.Range(0,16).Select(i=> (i<<4)))
                {
                    var items = Enumerable.Range(0, 16)
                        .Select(i => root + i)
                        .SelectMany(i => new byte[] { (byte)(i >> 8), (byte)(i & 0xFF) }).ToArray();

                    await tr.MoveToNextLineAndShowStringAsync(items);
                }

            }
        }
    }
}