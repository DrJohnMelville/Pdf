namespace Melville.Icc.Model.Tags
{
    /// <summary>
    /// Describes illumination used during a color measurement.
    /// </summary>
    public enum StandardIllumination : uint
    {
        /// <summary>
        /// Standard Illumination Unknown
        /// </summary>
        Unknown = 0,
        /// <summary>
        /// Standard Illumination D50
        /// </summary>
        D50 = 1,
        /// <summary>
        /// Standard Illumination D65
        /// </summary>
        D65 = 2,
        /// <summary>
        /// Standard Illumination D93
        /// </summary>
        D93 = 3,
        /// <summary>
        /// Standard Illumination F2
        /// </summary>
        F2 = 4,
        /// <summary>
        /// Standard Illumination D55
        /// </summary>
        D55 = 5,
        /// <summary>
        /// Standard Illumination A
        /// </summary>
        A = 6,
        /// <summary>
        /// Standard Illumination EquiPower
        /// </summary>
        EquiPower = 7,
        /// <summary>
        /// Standard Illumination F8
        /// </summary>
        F8 = 8
    }
}