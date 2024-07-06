namespace Melville.Fonts.SfntParsers.TableDeclarations.CffGlyphs;

/// <summary>
/// CFF charstring operator codes.
/// </summary>
public enum CharStringOperators
{
    /// <summary>
    /// Hstem hint operator
    /// </summary>
    HStem = 0x01,
    /// <summary>
    /// Vstem hint operator
    /// </summary>
    VStem = 0x03,
    /// <summary>
    /// Move vertically by the given offset.
    /// </summary>
    VMoveTo = 0x04,
    /// <summary>
    /// Draw a line from the curent point to a giveen relative point
    /// </summary>
    RLineTo = 0x05,
    /// <summary>
    /// Draw a horizontal line by the given offset.
    /// </summary>
    HLineTo = 0x06,
    /// <summary>
    /// Draw a vertical line by the given offset.
    /// </summary>
    VLineTo = 0x07,
    /// <summary>
    /// Draw a sequence of curves for each hextet of offsets, starting at the
    /// current point
    /// </summary>
    RRCurveTo = 0x08,
    /// <summary>
    /// In type 1 fonts, close the stroke path.  (Can be a noop, because we c lose all paths.)
    /// </summary>
    ClosePath = 0x09,
    /// <summary>
    /// Call a local subroutine
    /// </summary>
    CallSubr = 0x0A,
    /// <summary>
    /// Return from a subroutine
    /// </summary>
    Return = 0x0B,
    /// <summary>
    /// Access the extended instruction set with the next byte
    /// </summary>
    Escape = 0x0C,
    /// <summary>
    /// In type 1 fonts set the font width and left sidebearing point
    /// </summary>
    Hsbw = 0x0D,
    /// <summary>
    /// End the charstring
    /// </summary>
    EndChar = 0x0E,
    /// <summary>
    /// 
    /// </summary>
    VsIndex = 0x0F,
    /// <summary>
    /// Blend operator
    /// </summary>
    Blend = 0x10,
    /// <summary>
    /// HStem + hintmask hint
    /// </summary>
    HStemHM = 0x12,
    /// <summary>
    /// declare which hints are active
    /// </summary>
    HintMask = 0x13,
    /// <summary>
    /// Control active hints
    /// </summary>
    CntrMask = 0x14,
    /// <summary>
    /// Move to an x,y location relative to the current point.
    /// </summary>
    RMoveTo = 0x15,
    /// <summary>
    /// Move by a given horizontal offset.
    /// </summary>
    HMoveTo = 0x16,
    /// <summary>
    /// VStem hint + a hintmask operator
    /// </summary>
    VStemHM = 0x17,
    /// <summary>
    /// draw a curve ending in a line.
    /// </summary>
    RCurveLine = 0x18,
    /// <summary>
    /// draw a line ending in a curve.
    /// </summary>
    RLineCurve = 0x19,
    /// <summary>
    /// Draw a sequence of curves that begin and ent with vertical tangents.
    /// </summary>
    VVCurveTo = 0x1A,
    /// <summary>
    /// Draw a sequence of curves that begin and end with horizontal tangents.
    /// </summary>
    HHCurveTo = 0x1B,
    /// <summary>
    /// prefix for a 2 byte short interget
    /// </summary>
    ShortInt = 0x1C,
    /// <summary>
    /// Call a global subroutine.
    /// </summary>
    CallGSubr = 0x1D,
    /// <summary>
    /// Draw a series of curves with alternating vertical and horizontal tangents.
    /// </summary>
    VHCurveTo = 0x1E,
    /// <summary>
    /// Draw a series of curves with alternating horizontal and vertical tangents.
    /// </summary>
    HVCurveTo = 0x1F,
    /// <summary>
    /// Brackets the dots in characters such as i and j and ?
    /// </summary>
    DotSection = 0x0C00,
    /// <summary>
    /// Hstem3 command in the type1 fonts definition
    /// </summary>
    HStem3 = 0x0C02,
    /// <summary>
    /// Logical AND
    /// </summary>
    And = 0x0C03,
    /// <summary>
    /// Logical OR
    /// </summary>
    Or = 0x0C04,
    /// <summary>
    /// Logial NOT
    /// </summary>
    Not = 0x0C05,
    /// <summary>
    /// In type 1 fonts make an accented character from two other characters.
    /// </summary>
    Seac = 0x0C06,
    /// <summary>
    /// In type 1 fonts set the width of the font and the left sidebearing point.
    /// </summary>
    SbW = 0x0C07,
    /// <summary>
    /// Compute absolute value of top stack element
    /// </summary>
    Abs = 0x0C09,
    /// <summary>
    /// Add the two top elements on the stack.
    /// </summary>
    Add = 0x0C0A,
    /// <summary>
    /// Subtract the top stack element from the second to top stack element
    /// </summary>
    Sub = 0x0C0B,
    /// <summary>
    /// Divide the second to top stack element by the top stack element.
    /// </summary>
    Div = 0x0C0C,
    /// <summary>
    /// additive inverse of the top stack element
    /// </summary>
    Negative = 0x0C0E,
    /// <summary>
    /// Check the top two stack elements for equality.
    /// </summary>
    Eq = 0x0C0F,
    /// <summary>
    ///  Type 1 Call other subr command.
    /// </summary>
    CallOtherSubr = 0x0C10,
    /// <summary>
    /// In type 1 fonts pop an item from the postscript stack to the Charstring stack.
    /// </summary>
    Pop = 0x0C11,
    /// <summary>
    /// Remove the top element from the stack.
    /// </summary>
    Drop = 0x0C12,
    /// <summary>
    /// Put an element from the stack into a given storage spot.
    /// </summary>
    Put = 0x0C14,
    /// <summary>
    /// Retrieve a value from a given storage spot.
    /// </summary>
    Get = 0x0C15,
    /// <summary>
    /// Pick one of two values based on a third boolean value.
    /// </summary>
    IfElse = 0x0C16,
    /// <summary>
    /// Compute a random value between 0 and 1.
    /// </summary>
    Random = 0x0C17,
    /// <summary>
    /// multiply the top two values.
    /// </summary>
    Mul = 0x0C18,
    /// <summary>
    /// compute the square root of the top value
    /// </summary>
    Sqrt = 0x0C1A,
    /// <summary>
    /// duplicate the top two values/
    /// </summary>
    Dup = 0x0C1B,
    /// <summary>
    /// Exchange the top two values.
    /// </summary>
    Exch = 0x0C1C,
    /// <summary>
    /// 
    /// </summary>
    Index = 0x0C1D,
    /// <summary>
    /// Rotate the parameter array
    /// </summary>
    Roll = 0x0C1E,
    /// <summary>
    /// In a type 1 font  set an absolute current point without calling the moveto operatiom
    /// </summary>
    SetCurrentPoint = 0x0C21,
    /// <summary>
    /// draw a horizontal flex curve
    /// </summary>
    HFlex = 0x0C22,
    /// <summary>
    /// Draw a line or a curve depending on the rendered size
    /// </summary>
    Flex = 0x0C23,
    /// <summary>
    /// draw a horizontal flex curve second version
    /// </summary>
    HFlex1 = 0x0C24,
    /// <summary>
    /// second version of the flex command.
    /// </summary>
    Flex1 = 0x0C25
}