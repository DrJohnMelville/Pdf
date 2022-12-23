using System;

namespace Melville.JBig2.Segments;

internal enum CombinationOperator
{
    Or = 0,
    And = 1,
    Xor = 2,
    Xnor = 3,
    Replace = 4,
}

internal static class CombinatiorOperatorImplementation
{
    public static int Combine(this CombinationOperator operation, int prior, int copied) =>
        operation switch
        {
            CombinationOperator.Or => prior | copied,
            CombinationOperator.And => prior & copied,
            CombinationOperator.Xor => prior ^ copied,
            CombinationOperator.Xnor => ~(prior ^ copied),
            CombinationOperator.Replace => copied,
            _ => throw new ArgumentOutOfRangeException(nameof(operation), operation, null)
        };
    public static byte Combine(this CombinationOperator operation, byte prior, byte copied) =>
        operation switch
        {
            CombinationOperator.Or => (byte)(prior | copied),
            CombinationOperator.And => (byte)(prior & copied),
            CombinationOperator.Xor => (byte)(prior ^ copied),
            CombinationOperator.Xnor => (byte)(~(prior ^ copied)),
            CombinationOperator.Replace => copied,
            _ => throw new ArgumentOutOfRangeException(nameof(operation), operation, null)
        };

    public static ulong Combine(this CombinationOperator operation, ulong prior, ulong copied) =>
        operation switch
        {
            CombinationOperator.Or => prior | copied,
            CombinationOperator.And => prior & copied,
            CombinationOperator.Xor => prior ^ copied,
            CombinationOperator.Xnor => ~(prior ^ copied),
            CombinationOperator.Replace => copied,
            _ => throw new ArgumentOutOfRangeException(nameof(operation), operation, null)
        };

}