namespace Melville.Icc.Model.Tags
{
    /// <summary>
    /// Describes the geometry used in measured color values
    /// </summary>
    public enum MeasurmentGeomenty : uint
    {
        /// <summary>
        /// Unknown measurement geometry
        /// </summary>
        /// <summary>
        /// Standard Illumination Unkown
        /// </summary>
        Unkown = 0,
        /// <summary>
        /// A45 measurement geometry
        /// </summary>
        /// <summary>
        /// Standard Illumination a45
        /// </summary>
        a45 = 1,
        /// <summary>
        /// A0 Measurement geometry
        /// </summary>
        /// <summary>
        /// Standard Illumination a0
        /// </summary>
        a0 = 2
    }
}