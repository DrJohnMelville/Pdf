using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using Melville.Parsing.SequenceReaders;

namespace Melville.Icc.Model.Tags;

/// <summary>
/// This represents a 3x3 matrix with float sized values
/// </summary>
/// <param name="M11">Row 1 Column 1</param>
/// <param name="M12">Row 1 Column 2</param>
/// <param name="M13">Row 1 Column 3</param>
/// <param name="M21">Row 2 Column 1</param>
/// <param name="M22">Row 2 Column 2</param>
/// <param name="M23">Row 2 Column 3</param>
/// <param name="M31">Row 3 Column 1</param>
/// <param name="M32">Row 3 Column 2</param>
/// <param name="M33">Row 3 Column 3</param>
public record struct Matrix3x3(
    float M11, float M12, float M13, 
    float M21, float M22, float M23, 
    float M31, float M32, float M33
    )
{ 
    internal static readonly Matrix3x3 Identity = new Matrix3x3(1,0,0 ,0,1,0, 0,0,1);

    internal Matrix3x3(IReadOnlyList<double> c) :
        this(
            (float)c[0],
            (float)c[1],
            (float)c[2],
            (float)c[3],
            (float)c[4],
            (float)c[5],
            (float)c[6],
            (float)c[7],
            (float)c[8]
        )
    {
    }

    internal Matrix3x3(ref SequenceReader<byte> reader): this(
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

    [Pure]
    internal void PostMultiplyBy(in ReadOnlySpan<float> input, in Span<float> output)
    {
        Debug.Assert(input.Length == 3);
        Debug.Assert(output.Length == 3);
        (output[0], output[1], output[2]) = (
            M11 * input[0] + M12 * input[1] + M13 * input[2],
            M21 * input[0] + M22 * input[1] + M23 * input[2],
            M31 * input[0] + M32 * input[1] + M33 * input[2]
        );
    }

    /// <summary>
    /// Add two matrices
    /// </summary>
    /// <param name="a">Left matrix</param>
    /// <param name="b">Right matrix</param>
    /// <returns>the sum matrix</returns>
    public static Matrix3x3 operator +(in Matrix3x3 a, in Matrix3x3 b) =>
        new Matrix3x3(
            a.M11 + b.M11,
            a.M12 + b.M12,
            a.M13 + b.M13,
            a.M21 + b.M21,
            a.M22 + b.M22,
            a.M23 + b.M23,
            a.M31 + b.M31,
            a.M32 + b.M32,
            a.M33 + b.M33
        );

    /// <summary>
    /// Scalar multiplication of a matrix
    /// </summary>
    /// <param name="matrix">The matrix</param>
    /// <param name="scalar">The scalar</param>
    /// <returns>A matrix with every element multiplied by the scalar</returns>
    public static Matrix3x3 operator *(in Matrix3x3 matrix, float scalar) =>
      new (
           matrix.M11 * scalar, matrix.M12 * scalar, matrix.M13 * scalar,
           matrix.M21 * scalar, matrix.M22 * scalar, matrix.M23 * scalar,
           matrix.M31 * scalar, matrix.M32 * scalar, matrix.M33 * scalar
        );

    /// <summary>
    /// Multiply two matrices.  Matrix multiplication is not commutative, so order matters.
    /// </summary>
    /// <param name="a">The left matrix</param>
    /// <param name="b">The right matrix</param>
    /// <returns>The product matrix z*b</returns>
    public static Matrix3x3 operator *(in Matrix3x3 a, in Matrix3x3 b) =>
        new Matrix3x3(
            a.M11 * b.M11       + a.M12 * b. M21 +     a.M13 * b.M31,
            a.M11 * b.M12       + a.M12 * b. M22 +     a.M13 * b.M32,
            a.M11 * b.M13       + a.M12 * b. M23 +     a.M13 * b.M33,

            a.M21 * b.M11       + a.M22 * b. M21 +     a.M23 * b.M31,
            a.M21 * b.M12       + a.M22 * b. M22 +     a.M23 * b.M32,
            a.M21 * b.M13       + a.M22 * b. M23 +     a.M23 * b.M33,

            a.M31 * b.M11       + a.M32 * b. M21 +     a.M33 * b.M31,
            a.M31 * b.M12       + a.M32 * b. M22 +     a.M33 * b.M32,
            a.M31 * b.M13       + a.M32 * b. M23 +     a.M33 * b.M33
        );


    /// <inheritdoc />
    [Pure]
    public override string ToString() =>
        $"{M11,8:F3} {M12,8:F3} {M13,8:F3}\r\n" +
        $"{M21,8:F3} {M22,8:F3} {M23,8:F3}\r\n" +
        $"{M31,8:F3} {M32,8:F3} {M33,8:F3}\r\n";

    /// <summary>
    /// Determinant of the matrix.
    /// </summary>
    /// <returns>A scalar determinant</returns>
    [Pure]
    public float Determinant() =>
       (M11 * M22 * M33 +   M12 * M23 * M31 +    M13 * M21 * M32) - 
       (M13 * M22 * M31 +   M12 * M21 * M33 +    M11 * M23 * M32);

    /// <summary>
    /// Determines if the matrix has a multiplicative inverse.
    /// </summary>
    /// <returns>True if the matrix can be inverted, false otherwise</returns>
    [Pure]
    public bool HasInverse() => Determinant() != 0;

    /// <summary>
    /// Multiplicative inverse of the matrix.
    /// </summary>
    /// <returns>The inverse matrix</returns>
    /// <exception cref="DivideByZeroException">If the matrix does not have an inverse</exception>
    // special case for inversion of the 3x3 matrix using partial determinants
    // https://en.wikipedia.org/wiki/Invertible_matrix#Inversion_of_3_%C3%97_3_matrices
    [Pure]
    public Matrix3x3 Inverse() =>
        new Matrix3x3(
              M22*M33 - M23*M32,  -(M12*M33 - M13*M32),  M12*M23 - M13*M22, 
            -(M21*M33 - M23*M31),   M11*M33 - M13*M31, -(M11*M23 - M13*M21), 
              M21*M32 - M22*M31,  -(M11*M32 - M12*M31),  M11*M22 - M12*M21) 
        * (1f / Determinant());
}