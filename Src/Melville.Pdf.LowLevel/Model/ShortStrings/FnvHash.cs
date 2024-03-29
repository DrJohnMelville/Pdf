﻿using System;

namespace Melville.Pdf.LowLevel.Model.ShortStrings;

// stolen from https://en.wikipedia.org/wiki/Fowler%E2%80%93Noll%E2%80%93Vo_hash_function
/// <summary>
/// This byte array hashing function is used in many parts of the renderer.  Most importantly PDF name lookup.
/// </summary>
public static class FnvHash
{
    /// <summary>
    /// Compute the FnvHash of a span, forcing each character to lower case.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static uint HashLowerCase(in ReadOnlySpan<byte> input)
    {
        var computer = new FnvComputer();
        foreach (var item in input)
        {
            computer.SingleLowerCaseHashStep(item);
        }
        return computer.HashAsUInt();
    }

    /// <summary>
    /// Hash a span of bytes -- returning an int.
    /// </summary>
    /// <param name="bytes">The bytes to hash.</param>
    /// <returns>The hash of the bytes casted to an int.</returns>
    public static int FnvHashAsInt(ReadOnlySpan<byte> bytes) => HashFromBytes(bytes).HashAsInt();


    /// <summary>
    /// Hash a span of bytes.
    /// </summary>
    /// <param name="bytes">The input data.</param>
    /// <returns>The hash of the input</returns>
    public static uint FnvHashAsUInt(ReadOnlySpan<byte> bytes) => 
        HashFromBytes(bytes).HashAsUInt();


    /// <summary>
    /// Hash a span of bytes.
    /// </summary>
    /// <param name="bytes">The input data.</param>
    /// <returns>The hash of the input, cast to an int.</returns>
    public static int FnvHashAsInt(ReadOnlySpan<char> bytes) => HashFromChars(bytes).HashAsInt();

    /// <summary>
    /// Hash a span of characters.
    /// </summary>
    /// <param name="chars">The input data.</param>
    /// <returns>The hash of the input</returns>
    public static uint FnvHashAsUInt(ReadOnlySpan<char> chars) => HashFromChars(chars).HashAsUInt();

    private static FnvComputer HashFromBytes(in ReadOnlySpan<byte> bytes)
    {
        var computer = new FnvComputer();
        foreach (var item in bytes)
        {
            computer.SingleHashStep(item);
        }

        return computer;
    }
    private static FnvComputer HashFromChars(ReadOnlySpan<char> chars)
    {
        var computer = new FnvComputer();
        foreach (var item in chars)
        {
            computer.SingleHashStep((byte)item);
        }

        return computer;
    }
}

/// <summary>
/// A small ref struct to compute FNV hashes
/// </summary>
public ref struct FnvComputer
{
    private const uint offsetBasis = 0x811c9dc5;
    private const uint prime = 0x01000193;
    private uint hashValue = offsetBasis;

    /// <summary>
    /// Create a FnvComputer
    /// </summary>
    public FnvComputer() {}

    /// <summary>
    /// Compute a single step of the FNV hash;
    /// </summary>
    /// <param name="item">The data for the FNV hash.</param>
    public void SingleHashStep(byte item)
    {
        unchecked
        {
            hashValue = (hashValue * prime) ^ item;
        }
    }


    /// <summary>
    /// Compute a single step of the FNV hash; using only lower case letters.
    /// </summary>
    /// <param name="item">The data for the FNV hash.</param>
    public void SingleLowerCaseHashStep(byte item) => SingleHashStep(ToLowerCase(item));

    private static byte ToLowerCase(byte input)
    {
        const byte aValue = (byte)'A';
        const byte zValue = (byte)'Z';
        const byte lowerAValue = (byte)'a';
        return input is >= aValue and <= zValue ? (byte)(input + (lowerAValue - aValue)) : input;
    }

    /// <summary>
    /// The current hash value as a UInt.
    /// </summary>
    /// <returns>The current hash value.</returns>
    public uint HashAsUInt() => hashValue;
    
    /// <summary>
    /// The current hash value as an int.
    /// </summary>
    /// <returns>The current hash value.</returns>
    public int HashAsInt()
    {
        unchecked
        {
            return (int)hashValue;
        }
    }

}