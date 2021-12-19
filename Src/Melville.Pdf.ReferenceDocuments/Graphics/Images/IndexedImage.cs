namespace Melville.Pdf.ReferenceDocuments.Graphics.Images;

public class IndexedImage : IndexedImageBase
{
    public IndexedImage() : base(8, 
        0,1,2,3,4,5,6,7,8)
    {
    }
}

public class IndexedImage4Bit : IndexedImageBase
{
    public IndexedImage4Bit() : base(4, 0x01, 0x20, 
        0x34, 0x50, 
        0x67, 0x80)
    {
    }
}

public class IndexedImage2Bit : IndexedImageBase
{
    public IndexedImage2Bit() : base(2,
        0b00011000,
        0b11000100,
        0b10110000
        )
    {
    }
}

public class IndexedImage1Bit : IndexedImageBase
{
    public IndexedImage1Bit() : base(1,
        0b01000000,
        0b10100000,
        0b01000000
        )
    {
    }
}