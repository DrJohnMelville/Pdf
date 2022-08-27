using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Melville.JpegLibrary.Decoder;

namespace Melville.JpegLibrary.Readers;

public struct JpegReader
{
    private ReadOnlySequence<byte> _data;
    private int _initialLength;

    public JpegReader(ReadOnlySequence<byte> data)
    {
        _data = data;
        _initialLength = checked((int)data.Length);
    }

    public JpegReader(ReadOnlyMemory<byte> data)
    {
        _data = new ReadOnlySequence<byte>(data);
        _initialLength = data.Length;
    }

    public bool IsEmpty => _data.IsEmpty;
    public int RemainingByteCount => (int)_data.Length;
    public int ConsumedByteCount => _initialLength - (int)_data.Length;
    public ReadOnlySequence<byte> RemainingBytes => _data;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool TryPeekToBuffer(Span<byte> buffer)
    {
        ReadOnlySpan<byte> span = _data.FirstSpan;

        if (span.Length >= buffer.Length)
        {
            span.Slice(0, buffer.Length).CopyTo(buffer);
            return true;
        }

        return TryPeekToBufferSlow(span, buffer);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private bool TryPeekToBufferSlow(ReadOnlySpan<byte> firstSpan, Span<byte> buffer)
    {
        Debug.Assert(firstSpan.Length < buffer.Length);

        firstSpan.CopyTo(buffer);
        buffer = buffer.Slice(firstSpan.Length);

        ReadOnlySequence<byte> remaining = _data.Slice(firstSpan.Length);
        if (remaining.Length >= buffer.Length)
        {
            remaining.Slice(0, buffer.Length).CopyTo(buffer);
            return true;
        }

        return false;
    }

    [SkipLocalsInit]
    public bool TryReadStartOfImageMarker()
    {
        Span<byte> buffer = stackalloc byte[2];
        if (!TryPeekToBuffer(buffer))
        {
            return false;
        }

        if (buffer[0] == (byte)JpegMarker.Padding && buffer[1] == (byte)JpegMarker.StartOfImage)
        {
            _data = _data.Slice(2);
            return true;
        }

        return false;
    }

    [SkipLocalsInit]
    public bool TryReadMarker(out JpegMarker marker)
    {
        Span<byte> buffer = stackalloc byte[2];

        while (TryPeekToBuffer(buffer))
        {
            byte b1 = buffer[0];
            byte b2 = buffer[1];

            if (b1 == (byte)JpegMarker.Padding)
            {
                if (b2 == (byte)JpegMarker.Padding)
                {
                    _data = _data.Slice(1);
                    continue;
                }
                else if (b2 == 0)
                {
                    _data = _data.Slice(2);
                    continue;
                }

                _data = _data.Slice(2);
                marker = (JpegMarker)b2;
                return true;
            }

            SequencePosition? position = _data.PositionOf((byte)JpegMarker.Padding);
            if (!position.HasValue)
            {
                _data = default;
                marker = default;
                return false;
            }

            _data = _data.Slice(position.GetValueOrDefault());
        }

        marker = default;
        return false;
    }

    [SkipLocalsInit]
    public bool TryReadLength(out ushort length)
    {
        Span<byte> buffer = stackalloc byte[2];
        if (!TryPeekToBuffer(buffer))
        {
            length = default;
            return false;
        }

        length = (ushort)(buffer[0] << 8 | buffer[1] - 2);
        _data = _data.Slice(2);
        return true;
    }

    [SkipLocalsInit]
    public bool TryPeekLength(out ushort length)
    {
        Span<byte> buffer = stackalloc byte[2];
        if (!TryPeekToBuffer(buffer))
        {
            length = default;
            return false;
        }

        length = (ushort)(buffer[0] << 8 | buffer[1] - 2);
        return true;
    }

    public bool TryReadBytes(int length, out ReadOnlySequence<byte> bytes)
    {
        ReadOnlySequence<byte> buffer = _data;
        if (buffer.Length < length)
        {
            bytes = default;
            return false;
        }

        bytes = buffer.Slice(0, length);
        _data = buffer.Slice(length);
        return true;
    }

    public bool TryPeekBytes(int length, out ReadOnlySequence<byte> bytes)
    {
        ReadOnlySequence<byte> buffer = _data;
        if (buffer.Length < length)
        {
            bytes = default;
            return false;
        }

        bytes = buffer.Slice(0, length);
        return true;
    }

    public bool TryAdvance(int length)
    {
        if (_data.Length < length)
        {
            return false;
        }

        _data = _data.Slice(length);
        return true;
    }
}