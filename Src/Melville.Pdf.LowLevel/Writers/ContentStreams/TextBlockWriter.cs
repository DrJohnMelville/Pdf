using System;
using System.Buffers;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Wrappers.ContentValueStreamUnions;

namespace Melville.Pdf.LowLevel.Writers.ContentStreams;

/// <summary>
/// Convenience methods for generating text writing operations.
/// </summary>
public readonly partial struct TextBlockWriter : IDisposable, ITextObjectOperations
{
    private readonly ContentStreamWriter parent;

    [DelegateTo] private ITextObjectOperations Inner() => parent;

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
    public ValueTask ShowString(string decodedString) =>
        Inner().ShowString(decodedString.AsExtendedAsciiBytes());

    /// <summary>
    /// Convenience method to write a string after moving to the next line.
    /// </summary>
    /// <param name="decodedString">String to write</param>
    /// <returns>Valuetask signaling completion</returns>
    public ValueTask MoveToNextLineAndShowString(string decodedString) =>
        Inner().MoveToNextLineAndShowString(decodedString.AsExtendedAsciiBytes());

    /// <summary>
    /// Convenience method to write a string after moving to the next line with
    /// character and word spacing.
    /// </summary>
    /// <param name="wordSpace">Spacing between words.</param>
    /// <param name="charSpace">Spacing between characters.</param>
    /// <param name="decodedString">String to write</param>
    /// <returns>Valuetask signaling completion</returns>
    public ValueTask MoveToNextLineAndShowString(double wordSpace, double charSpace, string decodedString) =>
        Inner().MoveToNextLineAndShowString(
            wordSpace, charSpace, decodedString.AsExtendedAsciiBytes());

    /// <summary>
    /// Convenience method to convert.
    /// </summary>
    /// <param name="values">An array of numbers and strings to write as a spaced string.</param>
    public async ValueTask ShowSpacedString(params object[] values)
    {
        ContentStreamValueUnion[] items = ArrayPool<ContentStreamValueUnion>.Shared.Rent(values.Length);
        for (int i = 0; i < values.Length; i++)
        {
            items[i] = ValueFromObject(values[i]);
        }
        await Inner().ShowSpacedString(items.AsSpan(0, values.Length)).CA(); 
        ArrayPool<ContentStreamValueUnion>.Shared.Return(items);
    }

    private ContentStreamValueUnion ValueFromObject(object value) => value switch
    {
        string s => new ContentStreamValueUnion(s.AsExtendedAsciiBytes().AsMemory()),
        IConvertible c => new ContentStreamValueUnion(c.ToDouble(null), c.ToInt64(null)),
        byte[] d => new ContentStreamValueUnion(d.AsMemory()),
        var x => ValueFromObject(x.ToString() ?? "")
    };

    /// <summary>
    /// Closes out the text block.
    /// </summary>
    public void Dispose() => ((ITextBlockOperations)parent).EndTextObject();
}