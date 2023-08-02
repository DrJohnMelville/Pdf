namespace Melville.Pdf.ReferenceDocuments.Graphics.Images.CCITTEncoded;

public class CcittType4Encoded : CcottEncodedBase
{
    public CcittType4Encoded() : base("Encode using CCITT Type 4 encoding")
    {
    }
    protected override PdfValueDictionary CcittParamDictionary()
    {
        return new ValueDictionaryBuilder()
            .WithItem(KnownNames.KTName, -1)
            .WithItem(KnownNames.EncodedByteAlignTName, false)
            .WithItem(KnownNames.ColumnsTName, 32)
            .WithItem(KnownNames.EndOfBlockTName, false)
            .WithItem(KnownNames.BlackIs1TName, false)
            .WithItem(KnownNames.DamagedRowsBeforeErrorTName, 0)
            .AsDictionary();
    }

}

public class CcittType3K0Encoded : CcottEncodedBase
{
    public CcittType3K0Encoded() : base("Encode using CCITT Type 3 K = 0 encoding")
    {
    }
    protected override PdfValueDictionary CcittParamDictionary()
    {
        return new ValueDictionaryBuilder()
            .WithItem(KnownNames.KTName, 0)
            .WithItem(KnownNames.EncodedByteAlignTName, false)
            .WithItem(KnownNames.ColumnsTName, 32)
            .WithItem(KnownNames.EndOfBlockTName, false)
            .WithItem(KnownNames.BlackIs1TName, false)
            .WithItem(KnownNames.DamagedRowsBeforeErrorTName, 0)
            .AsDictionary();
    }

}
public class CcittType3K1Encoded : CcottEncodedBase
{
    public CcittType3K1Encoded() : base("Encode using CCITT Type 3 K = 1 encoding")
    {
    }
    protected override PdfValueDictionary CcittParamDictionary()
    {
        return new ValueDictionaryBuilder()
            .WithItem(KnownNames.KTName, 1)
            .WithItem(KnownNames.EncodedByteAlignTName, false)
            .WithItem(KnownNames.ColumnsTName, 32)
            .WithItem(KnownNames.EndOfBlockTName, false)
            .WithItem(KnownNames.BlackIs1TName, false)
            .WithItem(KnownNames.DamagedRowsBeforeErrorTName, 0)
            .AsDictionary();
    }
}

public class CcittType3K10Encoded : CcottEncodedBase 
{
    public CcittType3K10Encoded() : base("Encode using CCITT Type 3 K = 10 encoding")
    {
    }
    protected override PdfValueDictionary CcittParamDictionary()
    {
        return new ValueDictionaryBuilder()
            .WithItem(KnownNames.KTName, 10)
            .WithItem(KnownNames.EncodedByteAlignTName, false)
            .WithItem(KnownNames.ColumnsTName, 32)
            .WithItem(KnownNames.EndOfBlockTName, false)
            .WithItem(KnownNames.BlackIs1TName, false)
            .WithItem(KnownNames.DamagedRowsBeforeErrorTName, 0)
            .AsDictionary();
    }
}

public class CcittEncodedByteAlign : CcottEncodedBase
{
    public CcittEncodedByteAlign() : base("Encode using CCITT with byte aligned rows")
    {
    }
    protected override PdfValueDictionary CcittParamDictionary()
    {
        return new ValueDictionaryBuilder()
            .WithItem(KnownNames.KTName, -1)
            .WithItem(KnownNames.EncodedByteAlignTName, true)
            .WithItem(KnownNames.ColumnsTName, 32)
            .WithItem(KnownNames.EndOfBlockTName, false)
            .WithItem(KnownNames.BlackIs1TName, false)
            .WithItem(KnownNames.DamagedRowsBeforeErrorTName, 0)
            .AsDictionary();
    }
}
public class CcittFirst8Rows : CcottEncodedBase
{
    public CcittFirst8Rows() : base("Encode using CCITT but rows forces decode of top half of image")
    {
    }
    protected override PdfValueDictionary CcittParamDictionary()
    {
        return new ValueDictionaryBuilder()
            .WithItem(KnownNames.KTName, -1)
            .WithItem(KnownNames.EncodedByteAlignTName, false)
            .WithItem(KnownNames.ColumnsTName, 32)
            .WithItem(KnownNames.RowsTName, 8)
            .WithItem(KnownNames.EndOfBlockTName, false)
            .WithItem(KnownNames.BlackIs1TName, false)
            .WithItem(KnownNames.DamagedRowsBeforeErrorTName, 0)
            .AsDictionary();
    }
}
public class CcittEndOfBlockRescuesLowRowCount : CcottEncodedBase
{
    public CcittEndOfBlockRescuesLowRowCount() : base("Encode using CCITT and bad row count by detecting end of block")
    {
    }
    protected override PdfValueDictionary CcittParamDictionary() =>
        new ValueDictionaryBuilder()
            .WithItem(KnownNames.KTName, -1)
            .WithItem(KnownNames.EncodedByteAlignTName, false)
            .WithItem(KnownNames.ColumnsTName, 32)
            .WithItem(KnownNames.RowsTName, 8)
            .WithItem(KnownNames.EndOfBlockTName, true)
            .WithItem(KnownNames.BlackIs1TName, false)
            .WithItem(KnownNames.DamagedRowsBeforeErrorTName, 0)
            .AsDictionary();
}