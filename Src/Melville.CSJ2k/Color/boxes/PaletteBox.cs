/// <summary>**************************************************************************
/// 
/// $Id: PaletteBox.java,v 1.1 2002/07/25 14:50:47 grosbois Exp $
/// 
/// Copyright Eastman Kodak Company, 343 State Street, Rochester, NY 14650
/// $Date $
/// ***************************************************************************
/// </summary>
using System;
using ICCProfile = CoreJ2K.Icc.ICCProfile;
using io_RandomAccessIO = CoreJ2K.j2k.io.RandomAccessIO;

namespace CoreJ2K.Color.Boxes
{
	
	/// <summary> This class models the palette box contained in a JP2
	/// image.
	/// 
	/// </summary>
	/// <version> 	1.0
	/// </version>
	/// <author> 	Bruce A. Kern
	/// </author>
	public sealed class PaletteBox:JP2Box
	{
		/// <summary>Return the number of palette entries. </summary>
		public int NumEntries { get; private set; }

		/// <summary>Return the number of palette columns. </summary>
		public int NumColumns { get; private set; }

		private short[] bitdepth;
		private int[][] entries;
		
		/// <summary> Construct a PaletteBox from an input image.</summary>
		/// <param name="in">RandomAccessIO jp2 image
		/// </param>
		/// <param name="boxStart">offset to the start of the box in the image
		/// </param>
		/// <exception cref="IOException,">ColorSpaceException 
		/// </exception>
		public PaletteBox(io_RandomAccessIO in_Renamed, int boxStart):base(in_Renamed, boxStart)
		{
			readBox();
		}
		
		/// <summary>Analyze the box content. </summary>
		internal void  readBox()
		{
			var bfr = new byte[4];
			int i, j, b, m;
			//int entry;
			
			// Read the number of palette entries and columns per entry.
			in_Renamed.seek(dataStart);
			in_Renamed.readFully(bfr, 0, 3);
            NumEntries = ICCProfile.getShort(bfr, 0) & 0x0000ffff;
			NumColumns = bfr[2] & 0x0000ffff;
			
			// Read the bitdepths for each column
			bitdepth = new short[NumColumns];
			bfr = new byte[NumColumns];
			in_Renamed.readFully(bfr, 0, NumColumns);
			for (i = 0; i < NumColumns; ++i)
			{
				bitdepth[i] = (short) (bfr[i] & 0x00fff);
			}
			
			entries = new int[NumEntries * NumColumns][];
			
			bfr = new byte[2];
			for (i = 0; i < NumEntries; ++i)
			{
				entries[i] = new int[NumColumns];
				
				for (j = 0; j < NumColumns; ++j)
				{
					
					int bd = getBitDepth(j);
					var signed = isSigned(j);
					
					switch (getEntrySize(j))
					{
						
						case 1:  // 8 bit entries
							in_Renamed.readFully(bfr, 0, 1);
							b = bfr[0];
							break;
						
						
						case 2:  // 16 bits
							in_Renamed.readFully(bfr, 0, 2);
                            b = ICCProfile.getShort(bfr, 0);
							break;
						
						
						default: 
							throw new ColorSpaceException("palettes greater than 16 bits deep not supported");
						
					}
					
					if (signed)
					{
						// Do sign extension if high bit is set.
						if ((b & (1 << (bd - 1))) == 0)
						{
							// high bit not set.
							m = (1 << bd) - 1;
							entries[i][j] = m & b;
						}
						else
						{
							// high bit set.
                            // CONVERSION PROBLEM?
							m = unchecked((int)(0xffffffff << bd));
							entries[i][j] = m | b;
						}
					}
					else
					{
						// Clear all high bits.
						m = (1 << bd) - 1;
						entries[i][j] = m & b;
					}
				}
			}
		}
		
		/// <summary>Are entries signed predicate. </summary>
		public bool isSigned(int column)
		{
			return (bitdepth[column] & 0x80) == 1;
		}
		
		/// <summary>Are entries unsigned predicate. </summary>
		public bool isUnSigned(int column)
		{
			return !isSigned(column);
		}
		
		/// <summary>Return the bitdepth of palette entries. </summary>
		public short getBitDepth(int column)
		{
			return (short) ((bitdepth[column] & 0x7f) + 1);
		}
		
		/// <summary>Return an entry for a given index and column. </summary>
		public int getEntry(int column, int entry)
		{
			return entries[entry][column];
		}
		
		/// <summary>Return a suitable String representation of the class instance. </summary>
		public override string ToString()
		{
			var rep = new System.Text.StringBuilder("[PaletteBox ").Append("nentries= ").Append(Convert.ToString(NumEntries)).Append(", ncolumns= ").Append(Convert.ToString(NumColumns)).Append(", bitdepth per column= (");
			for (var i = 0; i < NumColumns; ++i)
				rep.Append(getBitDepth(i)).Append(isSigned(i)?"S":"U").Append(i < NumColumns - 1?", ":"");
			return rep.Append(")]").ToString();
		}
		
		private int getEntrySize(int column)
		{
			int bd = getBitDepth(column);
			return bd / 8 + (bd % 8) == 0?0:1;
		}
		
		/* end class PaletteBox */
		static PaletteBox()
		{
			{
				type = 0x70636c72;
			}
		}
	}
}