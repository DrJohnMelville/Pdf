
namespace Melville.Pdf.ReferenceDocuments.Graphics.Images.CCITTEncoded;

public class CcittBlackIs1 : CcottEncodedBase
{
    public CcittBlackIs1() : base("CCITT encoding with the black is 1 bit set")
    {
    }
    protected override PdfDictionary CcittParamDictionary() =>
        new DictionaryBuilder()
            .WithItem(KnownNames.K, -1)
            .WithItem(KnownNames.EncodedByteAlign, false)
            .WithItem(KnownNames.Columns, 32)
            .WithItem(KnownNames.EndOfBlock, false)
            .WithItem(KnownNames.BlackIs1, true)
            .WithItem(KnownNames.DamagedRowsBeforeError, 0)
            .AsDictionary();
}