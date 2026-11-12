using System;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Melville.Pdf.ImageExtractor;
using Melville.Pdf.Model.Renderers.Bitmaps;
using Melville.Pdf.Model.Renderers.Colors;
using Xunit;

namespace Melville.Pdf.DataModelTests.ImageExtractors
{
    public static class VerifyUniformBitmap
    {
        public static async ValueTask VerifyUniformAsync(
            this IPdfBitmap bmp, int width, int height, params byte[] data)
        {
            Assert.Equal(width, bmp.Width);
            Assert.Equal(height, bmp.Height);
            Assert.Equal(width * height, data.Length);
            var result = await bmp.AsDeviceColorArrayAsync();
            foreach (var (i,item) in data.Index())
            {
                Assert.Equal(new DeviceColor(item,item,item,item), result[i]);
            }
        }
    }
    public class TestBitmap : IExtractedBitmap
    {
        public int Width { get; }
        public int Height { get; }
        public bool DeclaredWithInterpolation { get; }
        public unsafe ValueTask RenderPbgraAsync(Memory<DeviceColor> target)
        {
            var buffer = target.Span;
            for (int i = 0; i < buffer.Length && i < data.Length; i++)
            {
                var value = data[i];
                buffer[i] = new DeviceColor(value, value, value, value);
            }
            return ValueTask.CompletedTask;
        }

        public int Page { get; }
        public Vector2 PositionBottomLeft { get; }
        public Vector2 PositionBottomRight { get; }
        public Vector2 PositionTopLeft { get; }
        public Vector2 PositionTopRight { get; }
        private readonly byte[] data;

        public TestBitmap(int width, int height, 
            float left, float top, float right, float bottom, int baseValue): this (
            width, height, false, 1, 
            (left, bottom), (right, bottom), (left, top), (right, top),
            Enumerable.Range(baseValue, width*height).Select(x=>(byte)x).ToArray()){}

        public TestBitmap(int width, int height, bool declaredWithInterpolation, int page,
            (float X, float Y) bottomLeft,
            (float X, float Y) bottomRight,
            (float X, float Y) topLeft,
            (float X, float Y) topRight,
            params byte[] data
        )
        {
            Width = width;
            Height = height;
            DeclaredWithInterpolation = declaredWithInterpolation;
            Page = page;
            PositionBottomLeft = CreateVector(bottomLeft);
            PositionBottomRight = CreateVector(bottomRight);
            PositionTopLeft = CreateVector(topLeft);
            PositionTopRight = CreateVector(topRight);
            if (height * width != data.Length)
                throw new InvalidDataException("Wrong number of pixels.");
            this.data = data;
        }

        private static Vector2 CreateVector((float X, float Y) bottomLeft)
        {
            return new Vector2(bottomLeft.X, bottomLeft.Y);
        }
    }
}