namespace Melville.Fonts.SfntParsers.TableParserParts;

/// <summary>
/// This attribute marks a field as corresponding to a SFnt spec field, the generator
/// will generate a parser that reads this field.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class SFntFieldAttribute : Attribute
{
    /// <summary>
    /// This is a string of c# code that computes the length of the array variable.
    /// </summary>
    public string Count { get; set; }

    /// <summary>
    /// Create a SnftField Attribute
    /// </summary>
    /// <param name="count">A string of c# code that computes the length of the array variable.</param>
    public SFntFieldAttribute(string count)
    {
        Count = count;
    }

    /// <summary>
    /// Create a SfntField atribute
    /// </summary>
    public SFntFieldAttribute(): this("") { }
}