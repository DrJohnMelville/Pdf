namespace Melville.Pdf.LowLevel.Model.Conventions
{
    /// <summary>
    /// This enum represents a nibble or 4 bit value in the HexMath routines.
    /// </summary>
    public enum Nibble 
    {
        /// <summary>
        /// Digit 0
        /// </summary>
        N0 = 0,
        /// <summary>
        /// Digit 1
        /// </summary>
        N1,
        /// <summary>
        /// Digit 2
        /// </summary>
        N2,
        /// <summary>
        /// Digit 3
        /// </summary>
        N3,
        /// <summary>
        /// Digit 4
        /// </summary>
        N4,
        /// <summary>
        /// Digit 5
        /// </summary>
        N5,
        /// <summary>
        /// Digit 6
        /// </summary>
        N6,
        /// <summary>
        /// Digit 7
        /// </summary>
        N7,
        /// <summary>
        /// Digit 8
        /// </summary>
        N8,
        /// <summary>
        /// Digit 9
        /// </summary>
        N9,
        /// <summary>
        /// Digit 10
        /// </summary>
        N10,
        /// <summary>
        /// Digit 11
        /// </summary>
        N11,
        /// <summary>
        /// Digit 12
        /// </summary>
        N12,
        /// <summary>
        /// Digit 13
        /// </summary>
        N13,
        /// <summary>
        /// Digit 14
        /// </summary>
        N14,
        /// <summary>
        /// Digit 15
        /// </summary>
        N15,
        /// <summary>
        /// Ran out of digits parsing a hex number
        /// </summary>
        OutOfSpace,
        /// <summary>
        /// Reached hex digit terminator
        /// </summary>
        Terminator = 255
    }
}