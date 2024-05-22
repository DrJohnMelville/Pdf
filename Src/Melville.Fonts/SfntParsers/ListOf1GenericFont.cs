using System.Collections;
using Melville.Fonts.SfntParsers.TableDeclarations.CMaps;

namespace Melville.Fonts.SfntParsers;

/// <summary>
/// This is a basis of generic fonts that makes it possible to return a single font
/// (the overwhelming majority) without allocating another object
/// </summary>
public abstract class ListOf1GenericFont: IGenericFont, IReadOnlyList<IGenericFont>
{
    /// <inheritdoc />
    public IEnumerator<IGenericFont> GetEnumerator()
    {
        yield return this;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <inheritdoc />
    public int Count => 1;

    /// <inheritdoc />
    public IGenericFont this[int index] => this;

    /// <inheritdoc />
    public abstract ValueTask<ICMapSource> ParseCMapsAsync();
}