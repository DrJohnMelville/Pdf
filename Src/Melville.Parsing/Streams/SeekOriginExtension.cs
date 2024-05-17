namespace Melville.Parsing.Streams
{
    /// <summary>
    /// Implements a transform from a stream and a seek origin to the offset or the origin from
    /// the beginning of the stream.
    /// </summary>
    public static class SeekOriginExtension
    {
        /// <summary>
        /// Compute the offset of the seek origin from the beginning of a stream
        /// </summary>
        /// <param name="stream">The stream to be seeked within</param>
        /// <param name="origin">Seek origin type</param>
        /// <returns>Location of the seek origin as an offset from the beginning of the stream</returns>
        public static long SeekOriginLocation(this Stream stream, SeekOrigin origin) => origin switch
        {
            SeekOrigin.Current => stream.Position,
            SeekOrigin.End => stream.Length,
            _ => 0
        };

    }
}