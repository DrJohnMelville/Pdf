using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.ReferenceDocuments.Graphics.Images.CCITTEncoded;

public class CcittType4Encoded : CcottEncodedBase
{
    public CcittType4Encoded() : base("Encode using CCITT Type 4 encoding")
    {
    }
    protected override PdfDictionary CcittParamDictionary()
    {
        return new DictionaryBuilder()
            .WithItem(KnownNames.K, -1)
            .WithItem(KnownNames.EncodedByteAlign, false)
            .WithItem(KnownNames.Columns, 32)
            .WithItem(KnownNames.EndOfBlock, false)
            .WithItem(KnownNames.BlackIs1, false)
            .WithItem(KnownNames.DamagedRowsBeforeError, 0)
            .AsDictionary();
    }

}

public class CcittType3K0Encoded : CcottEncodedBase
{
    public CcittType3K0Encoded() : base("Encode using CCITT Type 3 K = 0 encoding")
    {
    }
    protected override PdfDictionary CcittParamDictionary()
    {
        return new DictionaryBuilder()
            .WithItem(KnownNames.K, 0)
            .WithItem(KnownNames.EncodedByteAlign, false)
            .WithItem(KnownNames.Columns, 32)
            .WithItem(KnownNames.EndOfBlock, false)
            .WithItem(KnownNames.BlackIs1, false)
            .WithItem(KnownNames.DamagedRowsBeforeError, 0)
            .AsDictionary();
    }

}
public class CcittType3K1Encoded : CcottEncodedBase
{
    public CcittType3K1Encoded() : base("Encode using CCITT Type 3 K = 1 encoding")
    {
    }
    protected override PdfDictionary CcittParamDictionary()
    {
        return new DictionaryBuilder()
            .WithItem(KnownNames.K, 1)
            .WithItem(KnownNames.EncodedByteAlign, false)
            .WithItem(KnownNames.Columns, 32)
            .WithItem(KnownNames.EndOfBlock, false)
            .WithItem(KnownNames.BlackIs1, false)
            .WithItem(KnownNames.DamagedRowsBeforeError, 0)
            .AsDictionary();
    }
}

public class CcittType3K10Encoded : CcottEncodedBase 
{
    public CcittType3K10Encoded() : base("Encode using CCITT Type 3 K = 10 encoding")
    {
    }
    protected override PdfDictionary CcittParamDictionary()
    {
        return new DictionaryBuilder()
            .WithItem(KnownNames.K, 10)
            .WithItem(KnownNames.EncodedByteAlign, false)
            .WithItem(KnownNames.Columns, 32)
            .WithItem(KnownNames.EndOfBlock, false)
            .WithItem(KnownNames.BlackIs1, false)
            .WithItem(KnownNames.DamagedRowsBeforeError, 0)
            .AsDictionary();
    }
}

public class CcittEncodedByteAlign : CcottEncodedBase
{
    public CcittEncodedByteAlign() : base("Encode using CCITT with byte aligned rows")
    {
    }
    protected override PdfDictionary CcittParamDictionary()
    {
        return new DictionaryBuilder()
            .WithItem(KnownNames.K, -1)
            .WithItem(KnownNames.EncodedByteAlign, true)
            .WithItem(KnownNames.Columns, 32)
            .WithItem(KnownNames.EndOfBlock, false)
            .WithItem(KnownNames.BlackIs1, false)
            .WithItem(KnownNames.DamagedRowsBeforeError, 0)
            .AsDictionary();
    }
}
public class CcittFirst8Rows : CcottEncodedBase
{
    public CcittFirst8Rows() : base("Encode using CCITT but rows forces decode of top half of image")
    {
    }
    protected override PdfDictionary CcittParamDictionary()
    {
        return new DictionaryBuilder()
            .WithItem(KnownNames.K, -1)
            .WithItem(KnownNames.EncodedByteAlign, false)
            .WithItem(KnownNames.Columns, 32)
            .WithItem(KnownNames.Rows, 8)
            .WithItem(KnownNames.EndOfBlock, false)
            .WithItem(KnownNames.BlackIs1, false)
            .WithItem(KnownNames.DamagedRowsBeforeError, 0)
            .AsDictionary();
    }
}
public class CcittEndOfBlockRescuesLowRowCount : CcottEncodedBase
{
    public CcittEndOfBlockRescuesLowRowCount() : base("Encode using CCITT and bad row count by detecting end of block")
    {
    }
    protected override PdfDictionary CcittParamDictionary()
    {
        return new DictionaryBuilder()
            .WithItem(KnownNames.K, -1)
            .WithItem(KnownNames.EncodedByteAlign, false)
            .WithItem(KnownNames.Columns, 32)
            .WithItem(KnownNames.Rows, 8)
            .WithItem(KnownNames.EndOfBlock, true)
            .WithItem(KnownNames.BlackIs1, false)
            .WithItem(KnownNames.DamagedRowsBeforeError, 0)
            .AsDictionary();
    }
}
public class CcittBlackIs1 : CcottEncodedBase
{
    public CcittBlackIs1() : base("CCITT encoding with the black is 1 bit set")
    {
    }
    protected override PdfDictionary CcittParamDictionary()
    {
        return new DictionaryBuilder()
            .WithItem(KnownNames.K, -1)
            .WithItem(KnownNames.EncodedByteAlign, false)
            .WithItem(KnownNames.Columns, 32)
            .WithItem(KnownNames.EndOfBlock, false)
            .WithItem(KnownNames.BlackIs1, true)
            .WithItem(KnownNames.DamagedRowsBeforeError, 0)
            .AsDictionary();
    }

}