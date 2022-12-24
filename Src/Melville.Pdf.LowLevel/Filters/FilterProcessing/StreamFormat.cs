namespace Melville.Pdf.LowLevel.Filters.FilterProcessing;

/// <summary>
/// This is a quasi enum because it has some meaningful non-named values.  This enum is used to describe the desired processing of a filtered
/// stream.
/// A value of 0 applies the first filter to the stream, if a first filter exists. (First filter is at index o.)
/// A value of 1 applies the first and second filter and etc.
/// Thus a value of MaxValue will always give the plaintext, because you cannot have more than MaxValue filters defined.
/// A value of -1 is a special value that gives the stream with any implicit encryption decrypted.
/// A value of MinValue gives the actual representation of the stream on the disk.
/// The overwhelming majority of uses of this field are for PlainText or for DiskRepresentation
/// </summary>
public enum StreamFormat
{
    /// <summary>
    /// Return the stream as it exists on the disk.
    /// </summary>
    DiskRepresentation = int.MinValue,
    /// <summary>
    /// Return to stream with implicit encryption decrypted
    /// </summary>
    ImplicitEncryption = -1,
    /// <summary>
    /// Return the plaintext value of the stream
    /// </summary>
    PlainText = int.MaxValue
}