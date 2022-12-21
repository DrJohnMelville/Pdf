namespace Melville.Icc.Model.Tags;

/// <summary>
/// This type represents the various colorants listed in Table 31 of the ICC spec
/// </summary>
public enum Colorant
{
    Unknown = 0,
    /// <summary>
    /// ITU-R BT.709-2 
    /// </summary>
    ItuRBT709 = 1,
    /// <summary>
    /// SMPTE RP145
    /// </summary>
    SMPTEP145 = 2,
    /// <summary>
    /// EBU Tech. 3213-E
    /// </summary>
    EBU3213E = 3,
    /// <summary>
    /// P22
    /// </summary>
    P22 = 4
}