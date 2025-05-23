﻿using System.Collections;
using System.Diagnostics.Contracts;
using Melville.Fonts.SfntParsers.TableDeclarations.CMaps;
using Melville.Fonts.SfntParsers.TableDeclarations.Heads;

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
    public abstract ValueTask<ICMapSource> GetCmapSourceAsync();

    /// <inheritdoc />
    public abstract ValueTask<IGlyphSource> GetGlyphSourceAsync();

    /// <inheritdoc/>
    public abstract ValueTask<string[]> GlyphNamesAsync();

    /// <inheritdoc />
    public abstract ValueTask<IGlyphWidthSource> GlyphWidthSourceAsync();

    /// <inheritdoc />
    public abstract ValueTask<string> FontFamilyNameAsync();

    /// <inheritdoc />
    public abstract ValueTask<MacStyles> GetFontStyleAsync();

    /// <inheritdoc />
    public abstract CidToGlyphMappingStyle TypeGlyphMapping { get; }
}