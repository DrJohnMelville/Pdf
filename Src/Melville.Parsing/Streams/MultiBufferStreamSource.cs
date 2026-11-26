using Melville.Hacks;
using Melville.INPC;
using Melville.Parsing.MultiplexSources;
using Melville.Parsing.ReaderExtensions;
using Melville.StreamInterfaces;
using Melville.StreamInterfaces.Memory;
using System.Runtime.CompilerServices;
using System.Text;

namespace Melville.Parsing.Streams;

/// <summary>
/// This is struct is used to allow many different types to convert implicitly to a multibufferstream
/// </summary>
[Union]
public readonly struct MultiBufferStreamSource : IUnion
{
    private readonly byte[] value;

    public object? Value => throw new NotImplementedException();

    public ReadOnlyMemory<byte> AsMemory() => value;
    public byte[] Buffer => value;

    /// <summary>
    /// Implicitly convert a value tp byte[] and store
    /// </summary>
    /// <param name="value">The value to store</param>
    public MultiBufferStreamSource(byte[] value) => this.value = value;

    /// <summary>
    /// Implicitly convert a value tp byte[] and store
    /// </summary>
    /// <param name="value">The value to store</param>
    public MultiBufferStreamSource(string value) : this(Encoding.ASCII.GetBytes(value))
    { }

    /// <summary>
    /// Implicitly convert a value tp byte[] and store
    /// </summary>
    /// <param name="value">The value to store</param>
    public MultiBufferStreamSource(IAsyncReader reader) : this(
        new AsyncReaderToByteArray(reader, false).Read().Item1)
    { }

    /// <summary>
    /// Implicitly convert a value to byte[] and store
    /// </summary>
    /// <param name="value">The value to store</param>
    public MultiBufferStreamSource(Stream reader) : this(
        AsMemoryStream(reader).ToArray())
    { }
    /// <summary>
    /// Implicitly convert a value to byte[] and store
    /// </summary>
    /// <param name="value">The value to store</param>
    public MultiBufferStreamSource(params Span<Stream> reader) : this(
        AsMemoryStream(reader).ToArray())
    { 
    }

    static MemoryStream AsMemoryStream(params Span<Stream> reader)
    {
        if (reader is [MemoryStream ms]) return ms;
        var ms2 = new MemoryStream();
        foreach (var item in reader)
        {
            item.CopyTo(ms2);
            item.Dispose();
        }
        ms2.Seek(0, SeekOrigin.Begin);
        return ms2;

    }

};
