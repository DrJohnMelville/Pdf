using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Postscript.Interpreter.Values;

namespace Melville.Pdf.LowLevel.Parsing.ContentStreams
{
    internal abstract class DictionaryTranslator
    {
        public static readonly DictionaryTranslator None = new NullTranslator();
        public static readonly DictionaryTranslator Image= new InlineImageTranslator();

        protected DictionaryTranslator(){}
        public abstract PdfDirectObject PopName(in PdfObjectCreator creator);
        public abstract PdfDirectObject PopNonname(in PdfObjectCreator creator);

        private class NullTranslator: DictionaryTranslator
        {
            public override PdfDirectObject PopName(in PdfObjectCreator creator) =>
                creator.PopPdfObjectFromStack();

            public override PdfDirectObject PopNonname(in PdfObjectCreator creator) =>
                creator.PopPdfObjectFromStack();
        }

        private class InlineImageTranslator : DictionaryTranslator
        {
            public override PdfDirectObject PopName(in PdfObjectCreator creator) =>
                TranslateName(creator.PopPdfObjectFromStack());

            private PdfDirectObject TranslateName(PdfDirectObject item)
            {
                return !item.IsName
                    ? item
                    : PdfDirectObject.CreateName(
                        item.Get<StringSpanSource>().GetSpan() switch
                        {
                            [(byte)'B', (byte)'P', (byte)'C'] => "BitsPerComponent"u8,
                            [(byte)'C', (byte)'S'] => "ColorSpace"u8,
                            [(byte)'D', (byte)'P'] => "DecodeParms"u8,
                            [(byte)'I', (byte)'M'] => "ImageMask"u8,
                            [(byte)'D'] => "Decode"u8,
                            [(byte)'F'] => "Filter"u8,
                            [(byte)'H'] => "Height"u8,
                            [(byte)'I'] => "Interpolate"u8,
                            [(byte)'L'] => "Length"u8,
                            [(byte)'W'] => "Width"u8,
                            var name => name
                        });
            }

            public override PdfDirectObject PopNonname(in PdfObjectCreator creator) =>
                TranslateNonName(creator.PopPdfObjectFromStack());

            private PdfDirectObject TranslateNonName(PdfDirectObject item)
            {
                return !item.IsName
                    ? item
                    : PdfDirectObject.CreateName(
                        item.Get<StringSpanSource>().GetSpan() switch
                        {
                            [(byte)'A', (byte)'H',(byte)'x']=> "ASCIIHexDecode"u8,
                            [(byte)'A', (byte)'8',(byte)'5']=> "ASCII85Decode"u8,
                            [(byte)'L', (byte)'Z',(byte)'W']=> "LZWDecode"u8,
                            [(byte)'C', (byte)'C',(byte)'F']=> "CCITTFaxDecode"u8,
                            [(byte)'D', (byte)'C', (byte)'T'] => "DCTDecode"u8,
                            [(byte)'R', (byte)'G', (byte)'B'] => "DeviceRGB"u8,
                            [(byte)'C', (byte)'M', (byte)'Y', (byte)'K'] => "DeviceCMYK"u8,
                            [(byte)'F', (byte)'l']=> "FlateDecode"u8,
                            [(byte)'R', (byte)'L']=> "RunLengthDecode"u8,
                            [(byte)'G']=> "DeviceGray"u8,
                            [(byte)'I']=> "Indexed"u8,
                            var name => name
                        });
            }
        }
    }
}