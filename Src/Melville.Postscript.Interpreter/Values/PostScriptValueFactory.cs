using System;
using System.Text;

namespace Melville.Postscript.Interpreter.Values
{
    internal static class PostscriptValueFactory
    {
        /// <summary>
        /// Create a PostscriptValue representing a long.
        /// </summary>
        /// <param name="value">The long to encode</param>
        public static PostscriptValue Create(long value) => new(PostscriptInteger.Instance, value);

        /// <summary>
        /// Create a PostscriptValue representing a double.
        /// </summary>
        /// <param name="value">The double to encode</param>
        public static PostscriptValue Create(double value) =>
            new(PostscriptDouble.Instance, BitConverter.DoubleToInt64Bits(value));

        /// <summary>
        /// Create a PostscriptValue representing a boolean.
        /// </summary>
        /// <param name="value">The double to encode</param>
        public static PostscriptValue Create(bool value) => 
            new(PostscriptBoolean.Instance, value ? 1 : 0);


        /// <summary>
        /// Create a PostScriptvalue with Null values.
        /// </summary>
        public static PostscriptValue CreateNull() => new(PostscriptNull.Instance, 0);

        /// <summary>
        /// Create a PostscriptValue for the Mark object.
        /// </summary>
        public static PostscriptValue CreateMark() => new (PostscriptMark.Instance,0);

        public static PostscriptValue CreateString(string data, StringKind kind)
        {
            Span<byte> buffer = stackalloc byte[Encoding.ASCII.GetByteCount(data)];
            Encoding.ASCII.GetBytes(data.AsSpan(), buffer);
            return CreateString(buffer, kind);
        }
        public static PostscriptValue CreateString(in ReadOnlySpan<byte> data, StringKind kind)
        {
            if (data.Length > 18) return CreateLongString(data, kind);
;            Int128 value = 0;
            for (int i = data.Length -1; i >= 0; i--)
            {
                var character = data[i];
                if (character < 127) return CreateLongString(data, kind);
                SevenBitStringEncoding.AddOneCharacter(ref value, character);
            }

            return new PostscriptValue(PostscriptShortString.InstanceForKind(kind), value);
        }

        private static PostscriptValue CreateLongString(ReadOnlySpan<byte> data, StringKind kind) => 
            new(new PostscriptLongString(kind, data.ToArray()), 0);

    }
}