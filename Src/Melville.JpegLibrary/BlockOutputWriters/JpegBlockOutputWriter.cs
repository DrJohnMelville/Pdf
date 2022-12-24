namespace Melville.JpegLibrary.BlockOutputWriters;

/// <summary>
/// A output writer that write spatial block to the destination buffer.
/// </summary>
internal abstract class JpegBlockOutputWriter
{
    /// <summary>
    /// Write a 8x8 spatial block into the destination buffer.
    /// </summary>
    /// <param name="blockRef">The reference to the block that the implementation should read from.</param>
    /// <param name="componentIndex">The index of the component.</param>
    /// <param name="x">The X offset in the image.</param>
    /// <param name="y">The Y offset in the image.</param>
    public abstract void WriteBlock(ref short blockRef, int componentIndex, int x, int y);
}