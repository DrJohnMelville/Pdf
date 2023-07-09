using System;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Model.Conventions;

namespace Melville.Pdf.LowLevel.Writers.ContentStreams;

/// <summary>
/// Convenience methods for generating text writing operations.
/// </summary>
public readonly partial struct TextBlockWriter : IDisposable, ITextObjectOperations
{
    private readonly ContentStreamWriter parent;

    [DelegateTo]
    private ITextObjectOperations Inner() => parent;

    internal TextBlockWriter(ContentStreamWriter parent)
    {
        this.parent = parent;
        ((ITextBlockOperations)parent).BeginTextObject();
    }

    /// <summary>
    /// Convenience method to write an extended ASCII string.
    /// </summary>
    /// <param name="decodedString">String to write</param>
    /// <returns>Valuetask signaling completion</returns>
    public ValueTask ShowStringAsync(string decodedString) =>
        Inner().ShowStringAsync(decodedString.AsExtendedAsciiBytes());

    /// <summary>
    /// Convenience method to write a string after moving to the next line.
    /// </summary>
    /// <param name="decodedString">String to write</param>
    /// <returns>Valuetask signaling completion</returns>
    public ValueTask MoveToNextLineAndShowStringAsync(string decodedString) =>
        Inner().MoveToNextLineAndShowStringAsync(decodedString.AsExtendedAsciiBytes());

    /// <summary>
    /// Convenience method to write a string after moving to the next line with
    /// character and word spacing.
    /// </summary>
    /// <param name="wordSpace">Spacing between words.</param>
    /// <param name="charSpace">Spacing between characters.</param>
    /// <param name="decodedString">String to write</param>
    /// <returns>Valuetask signaling completion</returns>
    public ValueTask MoveToNextLineAndShowStringAsync(double wordSpace, double charSpace, string decodedString) =>
        Inner().MoveToNextLineAndShowStringAsync(
            wordSpace, charSpace, decodedString.AsExtendedAsciiBytes());

    /// <summary>
    /// Convenience method to convert.
    /// </summary>
    /// <param name="values">An array of numbers and strings to write as a spaced string.</param>
    public async ValueTask ShowSpacedStringAsync(params object[] values)
    {
        var builder = Inner().GetSpacedStringBuilder();
        foreach (var value in values)
        {
            await WriteObjectAsync(value, builder).CA();
        }
        await builder.DoneWritingAsync().CA();
    }

    private static ValueTask WriteObjectAsync(
        object value, ISpacedStringBuilder builder) => value switch
    {
        string s => builder.SpacedStringComponentAsync(s.AsExtendedAsciiBytes().AsMemory()),
        IConvertible c => builder.SpacedStringComponentAsync(c.ToDouble(null)),
        byte[] arr => builder.SpacedStringComponentAsync(arr),
        var unknownType => WriteObjectAsync(unknownType.ToString() ?? "", builder)
    };

    /// <summary>
    /// Closes out the text block.
    /// </summary>
    public void Dispose() => ((ITextBlockOperations)parent).EndTextObject();
}