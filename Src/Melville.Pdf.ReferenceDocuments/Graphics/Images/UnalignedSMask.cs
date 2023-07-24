using Melville.Pdf.LowLevel.Model.Objects2;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Writers.ContentStreams;

namespace Melville.Pdf.ReferenceDocuments.Graphics.Images
{
    public class UnalignedSMask: DisplayImageTest
    {
        public UnalignedSMask() : base("Draw a trasparent smasked image")
        {
        }

        private PdfIndirectObject? smask;
        protected override ValueTask AddContentToDocumentAsync(PdfDocumentCreator docCreator)
        {
            smask = docCreator.LowLevelCreator.Add(new ValueDictionaryBuilder()
                .WithItem(KnownNames.TypeTName, KnownNames.XObjectTName)
                .WithItem(KnownNames.SubtypeTName, KnownNames.ImageTName)
                .WithItem(KnownNames.WidthTName, 2)
                .WithItem(KnownNames.HeightTName, 2)
                .WithItem(KnownNames.ColorSpaceTName, KnownNames.DeviceGrayTName)
                .WithItem(KnownNames.BitsPerComponentTName, 8)
                .AsStream(new byte[]{10,64,127,212})
            );
            return base.AddContentToDocumentAsync(docCreator);
        }

        protected override async ValueTask DoPaintingAsync(ContentStreamWriter csw)
        {
            await csw.SetStrokeRGBAsync(0,0, 1);
            csw.SetLineWidth(100);
            csw.MoveTo(0,0);
            csw.LineTo(300,200);
            csw.StrokePath();
            await base.DoPaintingAsync(csw);
        }

        protected override PdfValueStream CreateImage()
        {
            return new ValueDictionaryBuilder()
                .WithItem(KnownNames.TypeTName, KnownNames.XObjectTName)
                .WithItem(KnownNames.SubtypeTName, KnownNames.ImageTName)
                .WithItem(KnownNames.ColorSpaceTName, KnownNames.DeviceRGBTName)
                .WithItem(KnownNames.WidthTName, 256)
                .WithItem(KnownNames.HeightTName, 256)
                .WithItem(KnownNames.BitsPerComponentTName, 8)
                .WithItem(KnownNames.SMaskTName, smask)
                .AsStream(GenerateImage());
        }

        private byte[] GenerateImage()
        {
            var ret = new byte[256 * 256 * 3];
            var pos = 0;
            for (int i = 0; i < 256; i++)
            for (int j = 0; j < 256; j++)
            {
                ret[pos++] = (byte)i;
                ret[pos++] = (byte)j;
                ret[pos++] = 0;
            }
            return ret;
        }
    }
}