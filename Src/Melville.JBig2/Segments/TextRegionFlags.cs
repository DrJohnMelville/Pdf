namespace Melville.JBig2.Segments;

public readonly struct TextRegionFlags
{
    private readonly int data;

    public TextRegionFlags(int data)
    {
        this.data = data;
    }

    /// <summary>
    /// In standard SBHUFF
    /// </summary>
    public bool UseHuffman => BitOperations.CheckBit(data, 0x01);
    /// <summary>
    /// In Standard SBREFINE
    /// </summary>
    public bool UsesRefinement => BitOperations.CheckBit(data, 0x02);

    /// <summary>
    /// In standard LOGSBSTRIPS except this property evaluated the exponential to get the encoded value
    /// Also referred to STRIPS
    /// </summary>
    public int StripSize => 1 << LogStripSize;
    /// <summary>
    /// In Standard LogStripSize
    /// </summary>
    public int LogStripSize => BitOperations.UnsignedInteger(data, 2, 3);
    /// <summary>
    /// In standard REFCORNER
    /// </summary>
    public ReferenceCorner ReferenceCorner => (ReferenceCorner)BitOperations.UnsignedInteger(data, 4, 3);
    /// <summary>
    /// In standard TRANSPOSED
    /// </summary>
    public bool Transposed => BitOperations.CheckBit(data, 1 << 6);
    /// <summary>
    /// In standard SBCOMBOP
    /// </summary>
    public CombinationOperator CombinationOperator => 
        (CombinationOperator)BitOperations.UnsignedInteger(data, 7, 7);
    /// <summary>
    /// In standard SBDEFPIXEL
    /// </summary>
    public bool DefaultPixel => BitOperations.CheckBit(data, 1 << 9);
    /// <summary>
    /// In standard SBDSOFFSET
    /// </summary>
    public int DefaultCharacteSpacing => BitOperations.AsSignedInteger(BitOperations.UnsignedInteger(data, 10, 0x1F),5);
    /// <summary>
    /// In standard SBRTEMPLATE
    /// </summary>
    public bool RefinementTemplate => BitOperations.CheckBit(data, 1 << 15);
}