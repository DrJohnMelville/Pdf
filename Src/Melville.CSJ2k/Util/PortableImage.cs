// Copyright (c) 2007-2016 CSJ2K contributors.
// Copyright (c) 2025 Sjofn LLC.
// Licensed under the BSD 3-Clause License.

using System;
using System.Collections.Generic;
using System.Linq;

namespace CoreJ2K.Util
{
    public sealed class PortableImage : IImage
    {
        #region FIELDS

        private readonly double[] byteScaling;

        #endregion

        #region CONSTRUCTORS

        internal PortableImage(int width, int height, int numberOfComponents, IEnumerable<int> bitsUsed)
        {
            Width = width;
            Height = height;
            NumberOfComponents = numberOfComponents;
            byteScaling = bitsUsed.Select(b => 255.0 / (1 << b)).ToArray();

            Data = new int[numberOfComponents * width * height];
        }

        #endregion

        #region PROPERTIES

        public int Width { get; }

        public int Height { get; }

        public int NumberOfComponents { get; }

        internal int[] Data { get; }

        #endregion

        #region METHODS

        public T As<T>()
        {
            var image = ImageFactory.New(Width, Height, NumberOfComponents, 
                ToBytes(Width, Height, NumberOfComponents, byteScaling, Data));
            return image.As<T>();
        }

        public int[] GetComponent(int number)
        {
            if (number < 0 || number >= NumberOfComponents)
            {
                throw new ArgumentOutOfRangeException(nameof(number));
            }

            var length = Width * Height;
            var component = new int[length];

            for (int i = number, k = 0; k < length; i += NumberOfComponents, ++k)
            {
                component[k] = Data[i];
            }

            return component;
        }

        internal void FillRow(int rowIndex, int lineIndex, int rowWidth, int[] rowValues)
        {
            Array.Copy(
                rowValues,
                0,
                Data,
                NumberOfComponents * (rowIndex + lineIndex * rowWidth),
                rowValues.Length);
        }

        private static byte[] ToBytes(int width, int height, int numberOfComponents, 
            IReadOnlyList<double> byteScaling, IReadOnlyList<int> data)
        {
            var count = numberOfComponents * width * height;
            var bytes = new byte[count];

            for (var component = 0; component < numberOfComponents; ++component)
            {
                for (int i = 0, j = 0; i < count; ++i)
                {
                    var b = (byte)(byteScaling[component] * data[i]);
                    bytes[j++] = b;
                }
            }

            return bytes;
        }

        #endregion
    }
}
