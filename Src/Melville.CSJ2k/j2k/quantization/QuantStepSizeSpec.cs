/* 
* CVS identifier:
* 
* $Id: QuantStepSizeSpec.java,v 1.12 2001/10/24 12:05:04 grosbois Exp $
* 
* Class:                   QuantStepSizeSpec
* 
* Description:    Quantization base normalized step size specifications
* 
* 
* 
* COPYRIGHT:
* 
* This software module was originally developed by Rapha�l Grosbois and
* Diego Santa Cruz (Swiss Federal Institute of Technology-EPFL); Joel
* Askel�f (Ericsson Radio Systems AB); and Bertrand Berthelot, David
* Bouchard, F�lix Henry, Gerard Mozelle and Patrice Onno (Canon Research
* Centre France S.A) in the course of development of the JPEG2000
* standard as specified by ISO/IEC 15444 (JPEG 2000 Standard). This
* software module is an implementation of a part of the JPEG 2000
* Standard. Swiss Federal Institute of Technology-EPFL, Ericsson Radio
* Systems AB and Canon Research Centre France S.A (collectively JJ2000
* Partners) agree not to assert against ISO/IEC and users of the JPEG
* 2000 Standard (Users) any of their rights under the copyright, not
* including other intellectual property rights, for this software module
* with respect to the usage by ISO/IEC and Users of this software module
* or modifications thereof for use in hardware or software products
* claiming conformance to the JPEG 2000 Standard. Those intending to use
* this software module in hardware or software products are advised that
* their use may infringe existing patents. The original developers of
* this software module, JJ2000 Partners and ISO/IEC assume no liability
* for use of this software module or modifications thereof. No license
* or right to this software module is granted for non JPEG 2000 Standard
* conforming products. JJ2000 Partners have full right to use this
* software module for his/her own purpose, assign or donate this
* software module to any third party and to inhibit third parties from
* using this software module for non JPEG 2000 Standard conforming
* products. This copyright notice must be included in all copies or
* derivative works of this software module.
* 
* Copyright (c) 1999/2000 JJ2000 Partners.
* */
using System;
using CoreJ2K.j2k.util;

namespace CoreJ2K.j2k.quantization
{
	
	/// <summary> This class extends ModuleSpec class in order to hold specifications about
	/// the quantization base normalized step size to use in each tile-component.
	/// 
	/// </summary>
	/// <seealso cref="ModuleSpec" />
	public sealed class QuantStepSizeSpec:ModuleSpec
	{
		
		/// <summary> Constructs an empty 'QuantStepSizeSpec' with specified number of
		/// tile and components. This constructor is called by the decoder.
		/// 
		/// </summary>
		/// <param name="nt">Number of tiles
		/// 
		/// </param>
		/// <param name="nc">Number of components
		/// 
		/// </param>
		/// <param name="type">the type of the specification module i.e. tile specific,
		/// component specific or both.
		/// 
		/// </param>
		public QuantStepSizeSpec(int nt, int nc, byte type):base(nt, nc, type)
		{
		}
		
		/// <summary> Constructs a new 'QuantStepSizeSpec' for the specified number of
		/// components and tiles and the arguments of "-Qstep" option.
		/// 
		/// </summary>
		/// <param name="nt">The number of tiles
		/// 
		/// </param>
		/// <param name="nc">The number of components
		/// 
		/// </param>
		/// <param name="type">the type of the specification module i.e. tile specific,
		/// component specific or both.
		/// 
		/// </param>
		/// <param name="pl">The ParameterList
		/// 
		/// </param>
		public QuantStepSizeSpec(int nt, int nc, byte type, ParameterList pl):base(nt, nc, type)
		{
			
			var param = pl.getParameter("Qstep");
			if (param == null)
			{
				throw new ArgumentException("Qstep option not specified");
			}
			
			// Parse argument
			var stk = new SupportClass.Tokenizer(param);
			string word; // current word
			var curSpecType = SPEC_DEF; // Specification type of the
			// current parameter
			bool[] tileSpec = null; // Tiles concerned by the specification
			bool[] compSpec = null; // Components concerned by the specification
			float value_Renamed; // value of the current step size
			
			while (stk.HasMoreTokens())
			{
				word = stk.NextToken().ToLower();
				
				switch (word[0])
				{
					
					case 't':  // Tiles specification
						tileSpec = parseIdx(word, nTiles);
						curSpecType = curSpecType == SPEC_COMP_DEF ? SPEC_TILE_COMP : SPEC_TILE_DEF;
						break;
					
					case 'c':  // Components specification
						compSpec = parseIdx(word, nComp);
						curSpecType = curSpecType == SPEC_TILE_DEF ? SPEC_TILE_COMP : SPEC_COMP_DEF;
						break;
					
					default:  // Step size value
						try
						{
							//UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
							value_Renamed = float.Parse(word);
						}
						catch (FormatException)
						{
							throw new ArgumentException($"Bad parameter for -Qstep option : {word}");
						}
						
						//UPGRADE_TODO: The equivalent in .NET for method 'java.lang.Float.floatValue' may return a different value. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1043'"
						if (value_Renamed <= 0.0f)
						{
							//UPGRADE_TODO: The equivalent in .NET for method 'java.lang.Float.toString' may return a different value. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1043'"
							throw new ArgumentException($"Normalized base step must be positive : {value_Renamed}");
						}
						
						
						if (curSpecType == SPEC_DEF)
						{
							setDefault(value_Renamed);
						}
						else if (curSpecType == SPEC_TILE_DEF)
						{
							for (var i = tileSpec.Length - 1; i >= 0; i--)
								if (tileSpec[i])
								{
									setTileDef(i, value_Renamed);
								}
						}
						else if (curSpecType == SPEC_COMP_DEF)
						{
							for (var i = compSpec.Length - 1; i >= 0; i--)
								if (compSpec[i])
								{
									setCompDef(i, value_Renamed);
								}
						}
						else
						{
							for (var i = tileSpec.Length - 1; i >= 0; i--)
							{
								for (var j = compSpec.Length - 1; j >= 0; j--)
								{
									if (tileSpec[i] && compSpec[j])
									{
										setTileCompVal(i, j, value_Renamed);
									}
								}
							}
						}
						
						// Re-initialize
						curSpecType = SPEC_DEF;
						tileSpec = null;
						compSpec = null;
						break;
					
				}
			}
			
			// Check that default value has been specified
			if (getDefault() == null)
			{
				var ndefspec = 0;
				for (var t = nt - 1; t >= 0; t--)
				{
					for (var c = nc - 1; c >= 0; c--)
					{
						if (specValType[t][c] == SPEC_DEF)
						{
							ndefspec++;
						}
					}
				}
				
				// If some tile-component have received no specification, it takes
				// the default value defined in ParameterList
				if (ndefspec != 0)
				{
					//UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
					setDefault(float.Parse(pl.DefaultParameterList.getParameter("Qstep")));
				}
				else
				{
					// All tile-component have been specified, takes the first
					// tile-component value as default.
					setDefault(getTileCompVal(0, 0));
					switch (specValType[0][0])
					{
						
						case SPEC_TILE_DEF: 
							for (var c = nc - 1; c >= 0; c--)
							{
								if (specValType[0][c] == SPEC_TILE_DEF)
									specValType[0][c] = SPEC_DEF;
							}
							tileDef[0] = null;
							break;
						
						case SPEC_COMP_DEF: 
							for (var t = nt - 1; t >= 0; t--)
							{
								if (specValType[t][0] == SPEC_COMP_DEF)
									specValType[t][0] = SPEC_DEF;
							}
							compDef[0] = null;
							break;
						
						case SPEC_TILE_COMP: 
							specValType[0][0] = SPEC_DEF;
							tileCompVal["t0c0"] = null;
							break;
						}
				}
			}
		}
	}
}