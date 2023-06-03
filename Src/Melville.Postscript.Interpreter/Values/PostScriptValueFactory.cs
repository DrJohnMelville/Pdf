using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
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

        /// <summary>
        /// Create a string, name, or literal name
        /// </summary>
        /// <param name="data">Contents of the string</param>
        /// <param name="kind">kind of name to create</param>
        public static PostscriptValue CreateString(string data, StringKind kind)
        {
            Span<byte> buffer = stackalloc byte[Encoding.ASCII.GetByteCount(data)];
            Encoding.ASCII.GetBytes(data.AsSpan(), buffer);
            return CreateString(buffer, kind);
        }
        /// <summary>
        /// Create a string, name, or literal name
        /// </summary>
        /// <param name="data">Contents of the string</param>
        /// <param name="kind">kind of name to create</param>
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
            new(
                ReportAllocation(new PostscriptLongString(kind, data.ToArray())), 0);

        public static PostscriptValue CreateArray(params PostscriptValue[] values) =>
            new(WrapInPostScriptArray(values), 0);

        private static PostscriptArray WrapInPostScriptArray(PostscriptValue[] values) =>
            values.Length < 1 ? PostscriptArray.Empty : 
                ReportAllocation(new PostscriptArray(values));

        public static PostscriptValue CreateDictionary(params PostscriptValue[] values) => 
            new(WrapInDictionary(values), 0);

        private static IPostscriptValueStrategy<string> WrapInDictionary(PostscriptValue[] values) =>
            values.Length == 0 ? PostscriptShortDictionary.Empty:
                ReportAllocation(new PostscriptShortDictionary(values.ToList()));

        private static T ReportAllocation<T>(T item)
        {
            //right now this is a marker method.  Eventually to implement save and restore we have to
            // track all of the virtual memory allocations.  right now this method just holds onto 
            // all the places where we allocate memory.
            return item;
        }

    }
}