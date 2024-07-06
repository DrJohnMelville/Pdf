using System.Runtime.InteropServices;

namespace Melville.Fonts.SfntParsers.TableDeclarations.CffGlyphs;

/// <summary>
/// This is a simple discriminated union that can hold an integer or a float.
/// </summary>
[StructLayout(LayoutKind.Explicit)]
public readonly struct DictValue
{
    [FieldOffset(0)] private readonly int intValue;
    [FieldOffset(0)] private readonly float floatValue;
    [FieldOffset(4)] private readonly bool isInt;

    /// <summary>
    /// Create a DictValue from an int.
    /// </summary>
    /// <param name="value">Value of the DictValue</param>
    public DictValue(int value)
    {
        intValue = value;
        isInt = true;
    }

    /// <summary>
    /// Create a DictValue from a float.
    /// </summary>
    /// <param name="value">Value of the DictValue</param>
    public DictValue(float value)
    {
        floatValue = value;
        isInt = false;
    }

    /// <summary>
    /// The value of the item as an int
    /// </summary>
    public int IntValue => isInt?intValue: Convert.ToInt32(floatValue);
    /// <summary>
    /// The value of the item as a float.
    /// </summary>
    public float FloatValue => isInt?Convert.ToSingle(intValue):floatValue;

    /// <inheritdoc />
    public override string ToString() => isInt?intValue.ToString():
        floatValue.ToString()+"f";
}