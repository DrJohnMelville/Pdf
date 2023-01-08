﻿#region MIT License
/*Copyright (c) 2012-2013 Robert Rouhani <robert.rouhani@gmail.com>

SharpFont based on Tao.FreeType, Copyright (c) 2003-2007 Tao Framework Team

Permission is hereby granted, free of charge, to any person obtaining a copy of
this software and associated documentation files (the "Software"), to deal in
the Software without restriction, including without limitation the rights to
use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
of the Software, and to permit persons to whom the Software is furnished to do
so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.*/
#endregion

using System;

namespace Melville.SharpFont
{
	/// <summary>
	/// The mode how the values of <see cref="Glyph.GetCBox"/> are returned.
	/// </summary>
	
	public enum GlyphBBoxMode : uint
	{
		/// <summary>Return unscaled font units.</summary>
		Unscaled = 0,

		/// <summary>Return unfitted 26.6 coordinates.</summary>
		Subpixels = 0,

		/// <summary>Return grid-fitted 26.6 coordinates.</summary>
		Gridfit = 1,

		/// <summary>Return coordinates in integer pixels.</summary>
		Truncate = 2,

		/// <summary>Return grid-fitted pixel coordinates.</summary>
		Pixels = 3
	}
}
