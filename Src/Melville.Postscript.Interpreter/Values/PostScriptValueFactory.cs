using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Melville.Postscript.Interpreter.InterpreterState;
using Melville.Postscript.Interpreter.Values.Composites;
using Melville.Postscript.Interpreter.Values.Execution;
using Melville.Postscript.Interpreter.Values.Numbers;
using Melville.Postscript.Interpreter.Values.Strings;

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

        public static PostscriptValue Create(IExternalFunction action) =>
            new(action, 0);


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
        public static PostscriptValue CreateString(string data, StringKind kind) =>
            CreateString(data.AsSpan(), kind);

        /// <summary>
        /// Create a string, name, or literal name
        /// </summary>
        /// <param name="data">Contents of the string</param>
        /// <param name="kind">kind of name to create</param>
        public static PostscriptValue CreateString(
            ReadOnlySpan<char> data, StringKind kind)
        {
            Span<byte> buffer = stackalloc byte[Encoding.ASCII.GetByteCount(data)];
            Encoding.ASCII.GetBytes(data, buffer);
            return CreateString(buffer, kind);

        }


        /// <summary>
        /// Create a string, name, or literal name
        /// </summary>
        /// <param name="data">Contents of the string</param>
        /// <param name="kind">kind of name to create</param>
        public static PostscriptValue CreateString(in ReadOnlySpan<byte> data, StringKind kind)
        {
            if (data.Length > IByteStringSource.ShortStringLimit) 
                return CreateLongString(data.ToArray(), kind);
;            Int128 value = 0;
            for (int i = data.Length -1; i >= 0; i--)
            {
                var character = data[i];
                if (character is 0 or > 127) return CreateLongString(data.ToArray(), kind);
                SevenBitStringEncoding.AddOneCharacter(ref value, character);
            }

            return new PostscriptValue(kind.ShortStringStraegy, value);
        }

        public static PostscriptValue CreateLongString(byte[] data, StringKind kind) => 
            new(
                ReportAllocation(new PostscriptLongString(kind, data)), 0);

        public static PostscriptValue CreateArray(params PostscriptValue[] values) =>
            new(WrapInPostScriptArray(values), 0);

        private static PostscriptArray WrapInPostScriptArray(PostscriptValue[] values) =>
            values.Length < 1 ? PostscriptArray.Empty : 
                ReportAllocation(new PostscriptArray(values));

        public static PostscriptValue CreateDictionary(params PostscriptValue[] values) => 
            new(WrapInDictionary(values), 0);

        private static IPostscriptValueStrategy<string> WrapInDictionary(PostscriptValue[] values) =>
            values.Length switch
            {
                0 => PostscriptShortDictionary.Empty,
                < 40 => ReportAllocation(new PostscriptShortDictionary(values.ToList())),
                _ => ConstructLongDictionary(values)
            };
        
        public static PostscriptValue CreateLongDictionary(params PostscriptValue[] parameters) =>
            new(ConstructLongDictionary(parameters), 0);

        private static PostscriptLongDictionary ConstructLongDictionary(PostscriptValue[] parameters) => 
            new PostscriptLongDictionary(ArrayToDictionary(parameters));

        private static Dictionary<PostscriptValue, PostscriptValue> ArrayToDictionary(PostscriptValue[] parameters)
        {
            var dict = new Dictionary<PostscriptValue, PostscriptValue>();
            for (var i = 1; i < parameters.Length; i += 2)
            {
                dict[parameters[i - 1]] = parameters[i];
            }
            return dict;
        }

        private static T ReportAllocation<T>(T item)
        {
            //right now this is a marker method.  Eventually to implement save and restore we have to
            // track all of the virtual memory allocations.  right now this method just holds onto 
            // all the places where we allocate memory.
            return item;
        }
    }
}