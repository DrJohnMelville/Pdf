using System.Numerics;
using System.Runtime.CompilerServices;

namespace Melville.JpegLibrary.Maths;

    internal static class JpegMathHelper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int RoundToInt32(float value) => (int)MathF.Round(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short RoundToInt16(float value) => (short)MathF.Round(value);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Clamp(int value, int min, int max) => Math.Clamp(value, min, max);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Clamp(float value, float min, float max) => Math.Clamp(value, min, max);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Log2(uint value) => BitOperations.Log2(value);
    }
