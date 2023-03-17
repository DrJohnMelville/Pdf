namespace Melville.Icc.Model;

/// <summary>
/// Describes colorspaces that an ICC profile might convert to or from.  See Table 19 on page 21 of the ICC spec
/// </summary>
public enum ColorSpace : uint
{

    /// <summary>
    /// Constant for colorspace XYZ
    /// </summary>
    XYZ = 0X58595A20,
    /// <summary>
    /// Constant for colorspace Lab
    /// </summary>
    Lab = 0x4c616220,
    /// <summary>
    /// Constant for colorspace Luv
    /// </summary>
    Luv = 0x4c757620,
    /// <summary>
    /// Constant for colorspace Ycbr
    /// </summary>
    Ycbr = 0x59436272,
    /// <summary>
    /// Constant for colorspace Yxy
    /// </summary>
    Yxy = 0x59787920,
    /// <summary>
    /// Constant for colorspace RGB
    /// </summary>
    RGB = 0x52474220,
    /// <summary>
    /// Constant for colorspace GRAY
    /// </summary>
    GRAY = 0x47524159,
    /// <summary>
    /// Constant for colorspace HSV
    /// </summary>
    HSV = 0x48535620,
    /// <summary>
    /// Constant for colorspace HLS
    /// </summary>
    HLS = 0x484c5320,
    /// <summary>
    /// Constant for colorspace CMYK
    /// </summary>
    CMYK = 0X434D594B,
    /// <summary>
    /// Constant for colorspace CMY
    /// </summary>
    CMY = 0X434D5920,
    /// <summary>
    /// Generic 0x2 element colorspace.
    /// </summary>
    Col2 = 0x32434c52,
    /// <summary>
    /// Generic 0x3 element colorspace.
    /// </summary>
    Col3 = 0x33434c52,
    /// <summary>
    /// Generic 0x4 element colorspace.
    /// </summary>
    Col4 = 0x34434c52,
    /// <summary>
    /// Generic 0x5 element colorspace.
    /// </summary>
    Col5 = 0x35434c52,
    /// <summary>
    /// Generic 0x6 element colorspace.
    /// </summary>
    Col6 = 0x36434c52,
    /// <summary>
    /// Generic 0x7 element colorspace.
    /// </summary>
    Col7 = 0x37434c52,
    /// <summary>
    /// Generic 0x8 element colorspace.
    /// </summary>
    Col8 = 0x38434c52,
    /// <summary>
    /// Generic 0x9 element colorspace.
    /// </summary>
    Col9 = 0x39434c52,
    /// <summary>
    /// Generic 0xA element colorspace.
    /// </summary>
    ColA = 0x41434c52,
    /// <summary>
    /// Generic 0xB element colorspace.
    /// </summary>
    ColB = 0x42434c52,
    /// <summary>
    /// Generic 0xC element colorspace.
    /// </summary>
    ColC = 0x43434c52,
    /// <summary>
    /// Generic 0xD element colorspace.
    /// </summary>
    ColD = 0x44434c52,
    /// <summary>
    /// Generic 0xE element colorspace.
    /// </summary>
    ColE = 0x45434c52,
    /// <summary>
    /// Generic 0xF element colorspace.
    /// </summary>
    ColF = 0x46434c52,
};