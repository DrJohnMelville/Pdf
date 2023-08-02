
namespace Melville.Pdf.ReferenceDocuments.Graphics.Images.CCITTEncoded;

public class CcittBlackIs1 : CcottEncodedBase
{
    public CcittBlackIs1() : base("CCITT encoding with the black is 1 bit set")
    {
    }
    protected override PdfDictionary CcittParamDictionary() =>
        new DictionaryBuilder()
            .WithItem(KnownNames.KTName, -1)
            .WithItem(KnownNames.EncodedByteAlignTName, false)
            .WithItem(KnownNames.ColumnsTName, 32)
            .WithItem(KnownNames.EndOfBlockTName, false)
            .WithItem(KnownNames.BlackIs1TName, true)
            .WithItem(KnownNames.DamagedRowsBeforeErrorTName, 0)
            .AsDictionary();
}