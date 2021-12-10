using System.Buffers;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using Melville.Icc.Parser;

namespace Melville.Icc.Model.Tags;

public readonly struct Matrix3x3
{
    public static readonly Matrix3x3 Identity = new Matrix3x3(1,0,0 ,0,1,0, 0,0,1);
    public float M11 { get; }
    public float M12 { get; }
    public float M13 { get; }
    public float M21 { get; }
    public float M22 { get; }
    public float M23 { get; }
    public float M31 { get; }
    public float M32 { get; }
    public float M33 { get; }

    public Matrix3x3(
        float m11, float m12, float m13, float m21, float m22, float m23, float m31, float m32, float m33)
    {
        M11 = m11;
        M12 = m12;
        M13 = m13;
        M21 = m21;
        M22 = m22;
        M23 = m23;
        M31 = m31;
        M32 = m32;
        M33 = m33;
    }
    
    public Matrix3x3(ref SequenceReader<byte> reader): this(
        reader.Reads15Fixed16(),
        reader.Reads15Fixed16(),
        reader.Reads15Fixed16(),
        reader.Reads15Fixed16(),
        reader.Reads15Fixed16(),
        reader.Reads15Fixed16(),
        reader.Reads15Fixed16(),
        reader.Reads15Fixed16(),
        reader.Reads15Fixed16()
        ){}

    public void PostMultiplyBy(in ReadOnlySpan<float> input, in Span<float> output)
    {
        Debug.Assert(input.Length == 3);
        Debug.Assert(output.Length == 3);
        (output[0], output[1], output[2]) = (
            M11 * input[0] + M12 * input[1] + M13 * input[2],
            M21 * input[0] + M22 * input[1] + M23 * input[2],
            M31 * input[0] + M32 * input[1] + M33 * input[2]
        );
    }
}

public readonly struct AugmentedMatrix3x3
{
    public static readonly AugmentedMatrix3x3 Identity = new(Matrix3x3.Identity, 0,0,0);
    public Matrix3x3 Kernel { get; }
    public float TranslateX { get; }
    public float TranslateY { get; }
    public float TranslateZ { get; }

    public AugmentedMatrix3x3(Matrix3x3 kernel, float translateX, float translateY, float translateZ)
    {
        Kernel = kernel;
        TranslateX = translateX;
        TranslateY = translateY;
        TranslateZ = translateZ;
    }
    
    public AugmentedMatrix3x3(ref SequenceReader<byte> reader):
        this (new Matrix3x3(ref reader),
            reader.Reads15Fixed16(), reader.Reads15Fixed16(), reader.Reads15Fixed16()){}

    public void PostMultiplyBy(in ReadOnlySpan<float> input, in Span<float> output)
    {
        Kernel.PostMultiplyBy(input, output);
        output[0] += TranslateX;
        output[1] += TranslateY;
        output[2] += TranslateZ;
    }
}