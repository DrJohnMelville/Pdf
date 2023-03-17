namespace Melville.Icc.Model.Tags
{
    /// <summary>
    /// Describes th=e observer used in color measurements
    /// </summary>
    public enum StandardObserver: uint
    {
        /// <summary>
        /// Undefined observer
        /// </summary>
        /// <summary>
        /// Standard Illumination Unknown
        /// </summary>
        Unknown = 0,
        /// <summary>
        /// CIE 1931 standard observer
        /// </summary>
        /// <summary>
        /// Standard Illumination Cie1931
        /// </summary>
        Cie1931 = 1,
        /// <summary>
        /// CIE 1964 standard observer
        /// </summary>
        /// <summary>
        /// Standard Illumination Cie1964
        /// </summary>
        Cie1964 = 2
    }
}