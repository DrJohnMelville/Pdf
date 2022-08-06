using System;

namespace Melville.Pdf.LowLevel.Model.Primitives;

// stolen from https://en.wikipedia.org/wiki/Fowler%E2%80%93Noll%E2%80%93Vo_hash_function
public static class FnvHash
{
    private const uint offsetBasis = 0x811c9dc5;
    private const uint prime = 0x01000193;

    public static uint HashString(string s)
    {
        var hash = offsetBasis;
        foreach (var character in s)
        {
            hash = SingleHashStep(hash, (byte)character);
        }

        return hash;
    }
    public static uint HashStringAsLowerCase(string s)
    {
        var hash = offsetBasis;
        foreach (var character in s)
        {
            hash = SingleHashStep(hash, (byte)Char.ToLower(character));
        }

        return hash;
    }
        
    public static int FnvHashAsInt( ReadOnlySpan<byte> bytes)
    {
        unchecked
        {
            return (int)FnvHashAsUint(bytes);
        }
    }
    public static int FnvHashAsInt( ReadOnlySpan<char> bytes)
    {
        unchecked
        {
            return (int)FnvHashAsUint(bytes);
        }
    }

    public static uint FnvHashAsUint(ReadOnlySpan<byte> bytes)
    {
        var hash = offsetBasis;
        foreach (var item in bytes)
        {
            hash = SingleHashStep(hash, item);
        }
        return hash;
    }
    public static uint FnvHashAsUint(ReadOnlySpan<char> chars)
    {
        var hash = offsetBasis;
        foreach (var item in chars)
        {
            hash = SingleHashStep(hash, (byte)item);
        }
        return hash;
    }

    public static uint EmptyStringHash() => offsetBasis;
    public static uint SingleHashStep(uint hash, byte item)
    {
        unchecked
        {
            return (hash * prime) ^ item;
        }
    }
}