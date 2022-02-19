using Melville.Pdf.LowLevel.Model.Primitives.VariableBitEncoding;

namespace Melville.Pdf.ReferenceDocuments.Graphics.Images.CCITTEncoded;

public class Type4Encoded: DisplayImageTest
    {
        public Type4Encoded() : base("Use A Mask to crop a rectangle out of an image")
        {
        }


        protected override PdfStream CreateImage()
        {
            return new DictionaryBuilder()
                .WithItem(KnownNames.Type, KnownNames.XObject)
                .WithItem(KnownNames.Subtype, KnownNames.Image)
                .WithItem(KnownNames.ColorSpace, KnownNames.DeviceGray)
                .WithItem(KnownNames.Width, new PdfInteger(32))
                .WithItem(KnownNames.Height, new PdfInteger(16))
                .WithItem(KnownNames.BitsPerComponent, new PdfInteger(1))
                .WithFilter(FilterName.CCITTFaxDecode)
                .WithFilterParam(new DictionaryBuilder()
                    .WithItem(KnownNames.K, -1)
                    .WithItem(KnownNames.EncodedByteAlign, false)
                    .WithItem(KnownNames.Columns, 32)
                    .WithItem(KnownNames.EndOfBlock, false)
                    .WithItem(KnownNames.BlackIs1, false)
                    .WithItem(KnownNames.DamagedRowsBeforeError, 0)
                    .AsDictionary())
                .AsStream(GenerateImage());
        }

        private byte[] GenerateImage()
        {
            var ret = new byte[32*16];
            var bitWriter = new BitWriter();
            var pos = 0;
            for (int i = 0; i < 16; i++)
            for (int j = 0; j < 32; j++)
            {
                pos += bitWriter.WriteBits((i + j) & 1, 1, ret.AsSpan(pos));
            }
            return ret;
        }
    }    