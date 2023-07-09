using System;
using System.Runtime.InteropServices;
using Melville.INPC;

namespace Melville.Postscript.Interpreter.Values
{
    [StructLayout(LayoutKind.Explicit)]
    public unsafe partial struct MementoUnion: IEquatable<MementoUnion>
    {
        [MacroCode("""
        /// <summary>
        /// The value as an ~0~
        /// </summary>
        [FieldOffset(0)] public readonly ~0~ ~0~;
        /// <summary>
        /// Create a MementoUnion from a ~0~;
        /// </summary>
        /// <param name="value">The value to set the memento to</param>
        public MementoUnion(~0~ value) => ~0~ = value;
        """)]
        [MacroItem("Int128")]
        [MacroItem("UInt128")]
        partial void Items128Bit();

        [MacroItem("UInt64")]
        [MacroItem("Int64")]
        [MacroItem("Double")]
        [MacroCode("""
        /// <summary>
        /// The value as 2 ~0~s
        /// </summary>
        [FieldOffset(0)] public fixed ~0~ ~0~s[2];
        
        /// <summary>
        /// Create a MementoUnion from 2 ~0~s
        /// </summary>
        /// <param name="a">The first value</param>
        /// <param name="b">The second value</param>
        public MementoUnion(~0~ a, ~0~ b ) {~0~s[0] = a; ~0~s[1] = b;}

        """)]
        partial void Items64Bit();

        [MacroItem("UInt32")]
        [MacroItem("Int32")]
        [MacroItem("Single")]
        [MacroCode("""
        /// <summary>
        /// The value as 4 ~0~s
        /// </summary>
        [FieldOffset(0)] public fixed ~0~ ~0~s[4];

        /// <summary>
        /// Create a MementoUnion from 4 ~0~s
        /// </summary>
        /// <param name="a">The first value.</param>
        /// <param name="b">The second value.</param>
        /// <param name="c">The third value.</param>
        /// <param name="d">The fourth  value.</param>
        public MementoUnion(~0~ a, ~0~ b , ~0~ c , ~0~ d ) 
        {~0~s[0] = a; ~0~s[1] = b; ~0~s[2] = a; ~0~s[3] = b;}
        """)]
        partial void Items32Bit();

        /// <summary>
        /// Create a memento from a boolean values
        /// </summary>
        /// <param name="value">The boolean value to store</param>
        public MementoUnion(bool value) => Bools[0] = value;

        /// <summary>
        /// The value as 15 booleans
        /// </summary>
        [FieldOffset(0)] public fixed bool Bools[16];

        public bool Equals(MementoUnion other)
        {
            return UInt128.Equals(other.UInt128);
        }

        public override bool Equals(object? obj)
        {
            return obj is MementoUnion other && Equals(other);
        }

        public override int GetHashCode()
        {
            return UInt128.GetHashCode();
        }

        public static bool operator ==(MementoUnion a, MementoUnion b) =>
            a.Equals(b);
        public static bool operator !=(MementoUnion a, MementoUnion b) =>
            !a.Equals(b);
    }
}