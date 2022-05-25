using System;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.Segments;

public enum CombinationOperator
{
    Or = 0,
    And = 1,
    Xor = 2,
    Xnor = 3,
    Replace = 4,
}

public static class CombinatiorOperatorImplementation
{
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
}