using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.Streams.Bases;

namespace Melville.Pdf.LowLevel.Filters.FlateFilters;

/// <summary>
/// Compute an Adler 32 hash.
/// </summary>
public class Adler32Computer
{
    private ulong s1;
    private ulong s2;

    /// <summary>
    /// Construct the Adler32Computer
    /// </summary>
    /// <param name="priorAdler"></param>
    public Adler32Computer(uint priorAdler = 1)
    {
        s1 = priorAdler & 0xFFFF;
        s2 = (priorAdler >> 16) & 0xFFFF;
    }
    private const ulong BiggestUintPrime = 65521; /* largest prime smaller than 65536 */
    private const int minIterationstoFillUint = 5552;

    /// <summary>
    /// Add data to the hash.
    /// </summary>
    /// <param name="bytes">Data to add.</param>
    public void AddData(ReadOnlySpan<byte> bytes)
    {
        for (int i = 0; i < bytes.Length;)
        {
            var limit = Math.Min(bytes.Length, i + minIterationstoFillUint);
            for (; i < limit; i++)
            {
                s1 += bytes[i];
                s2 += s1;
            }
            s1 %= BiggestUintPrime;
            s2 %= BiggestUintPrime;
        }
    }
        
    /// <summary>
    /// Get the current value of the hash.
    /// </summary>
    public uint GetHash() =>(uint) ((s2 << 16) | s1);

    /// <summary>
    /// Put the hash into the span in big endian format;
    /// </summary>
    /// <param name="destination">4 byte span to receive tha hash value.</param>
    public void CopyHashToBigEndianSpan(Span<byte> destination)
    {
        var checksum = GetHash();
        for (var i = 3; i >= 0; i--)
        {
            destination[i] = (byte)checksum;
            checksum >>= 8;
        }

    }
}

internal class ReadAdlerStream : DefaultBaseStream
{
    public Adler32Computer Computer { get; }= new();
    private readonly Stream source;

    public ReadAdlerStream(Stream source): base(true, false, false)
    {
        this.source = source;
    }

    public override async ValueTask<int> ReadAsync(
        Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        var ret = await source.ReadAsync(buffer, cancellationToken).CA();
        Computer.AddData(buffer.Span[..ret]);
        return ret;
    }
}