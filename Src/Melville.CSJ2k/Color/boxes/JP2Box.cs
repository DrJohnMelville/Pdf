/// <summary>**************************************************************************
/// 
/// $Id: JP2Box.java,v 1.1 2002/07/25 14:50:47 grosbois Exp $
/// 
/// Copyright Eastman Kodak Company, 343 State Street, Rochester, NY 14650
/// $Date $
/// ***************************************************************************
/// </summary>
using System;
using System.Collections.Generic;
using ColorSpaceException = Melville.CSJ2K.Color.ColorSpaceException;
using FileFormatBoxes = Melville.CSJ2K.j2k.fileformat.FileFormatBoxes;
using ICCProfile = Melville.CSJ2K.Icc.ICCProfile;
using ParameterList = Melville.CSJ2K.j2k.util.ParameterList;
using RandomAccessIO = Melville.CSJ2K.j2k.io.RandomAccessIO;
namespace Melville.CSJ2K.Color.Boxes
{
	
	/// <summary> The abstract super class modeling the aspects of
	/// a JP2 box common to all such boxes.
	/// 
	/// </summary>
	/// <version> 	1.0
	/// </version>
	/// <author> 	Bruce A. Kern
	/// </author>
	public abstract class JP2Box
	{
		/// <summary>Platform dependant line terminator </summary>
		//UPGRADE_NOTE: Final was removed from the declaration of 'eol '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
		public static readonly System.String eol = System.Environment.NewLine;
		/// <summary>Box type                           </summary>
		public static int type;
		
		/// <summary>Return a String representation of the Box type. </summary>
		public static System.String getTypeString(int t)
		{
			return BoxType.get_Renamed(t);
		}
		
		/// <summary>Length of the box.             </summary>
		public int length;
		/// <summary>input file                     </summary>
		protected internal RandomAccessIO in_Renamed;
		/// <summary>offset to start of box         </summary>
		protected internal int boxStart;
		/// <summary>offset to end of box           </summary>
		protected internal int boxEnd;
		/// <summary>offset to start of data in box </summary>
		protected internal int dataStart;
		
		/// <summary> Construct a JP2Box from an input image.</summary>
		/// <param name="in">RandomAccessIO jp2 image
		/// </param>
		/// <param name="boxStart">offset to the start of the box in the image
		/// </param>
		/// <exception cref="IOException,">ColorSpaceException 
		/// </exception>
		public JP2Box(RandomAccessIO in_Renamed, int boxStart)
		{
			byte[] boxHeader = new byte[16];
			
			this.in_Renamed = in_Renamed;
			this.boxStart = boxStart;
			
			this.in_Renamed.seek(this.boxStart);
			this.in_Renamed.readFully(boxHeader, 0, 8);
			
			this.dataStart = boxStart + 8;
            this.length = ICCProfile.getInt(boxHeader, 0);
			this.boxEnd = boxStart + length;
			if (length == 1)
				throw new ColorSpaceException("extended length boxes not supported");
		}
		
		
		/// <summary>Return the box type as a String. </summary>
		public virtual System.String getTypeString()
		{
			return BoxType.get_Renamed(JP2Box.type);
		}
		
		
		/// <summary>JP2 Box structure analysis help </summary>
		protected internal class BoxType:System.Collections.Generic.Dictionary<System.Int32, System.String>
		{

			private static System.Collections.Generic.Dictionary<System.Int32, System.String> map = new Dictionary<int, string>();
			
			private static void  put(int type, System.String desc)
			{
				map[(System.Int32) type] = desc;
			}
			
			public static System.String get_Renamed(int type)
			{
				return (System.String) map[(System.Int32) type];
			}
			
			/* end class BoxType */
			static BoxType()
			{
				{
					put(Melville.CSJ2K.j2k.fileformat.FileFormatBoxes.BITS_PER_COMPONENT_BOX, "BITS_PER_COMPONENT_BOX");
					put(Melville.CSJ2K.j2k.fileformat.FileFormatBoxes.CAPTURE_RESOLUTION_BOX, "CAPTURE_RESOLUTION_BOX");
					put(Melville.CSJ2K.j2k.fileformat.FileFormatBoxes.CHANNEL_DEFINITION_BOX, "CHANNEL_DEFINITION_BOX");
					put(Melville.CSJ2K.j2k.fileformat.FileFormatBoxes.COLOUR_SPECIFICATION_BOX, "COLOUR_SPECIFICATION_BOX");
					put(Melville.CSJ2K.j2k.fileformat.FileFormatBoxes.COMPONENT_MAPPING_BOX, "COMPONENT_MAPPING_BOX");
					put(Melville.CSJ2K.j2k.fileformat.FileFormatBoxes.CONTIGUOUS_CODESTREAM_BOX, "CONTIGUOUS_CODESTREAM_BOX");
					put(Melville.CSJ2K.j2k.fileformat.FileFormatBoxes.DEFAULT_DISPLAY_RESOLUTION_BOX, "DEFAULT_DISPLAY_RESOLUTION_BOX");
					put(Melville.CSJ2K.j2k.fileformat.FileFormatBoxes.FILE_TYPE_BOX, "FILE_TYPE_BOX");
					put(Melville.CSJ2K.j2k.fileformat.FileFormatBoxes.IMAGE_HEADER_BOX, "IMAGE_HEADER_BOX");
					put(Melville.CSJ2K.j2k.fileformat.FileFormatBoxes.INTELLECTUAL_PROPERTY_BOX, "INTELLECTUAL_PROPERTY_BOX");
					put(Melville.CSJ2K.j2k.fileformat.FileFormatBoxes.JP2_HEADER_BOX, "JP2_HEADER_BOX");
					put(Melville.CSJ2K.j2k.fileformat.FileFormatBoxes.JP2_SIGNATURE_BOX, "JP2_SIGNATURE_BOX");
					put(Melville.CSJ2K.j2k.fileformat.FileFormatBoxes.PALETTE_BOX, "PALETTE_BOX");
					put(Melville.CSJ2K.j2k.fileformat.FileFormatBoxes.RESOLUTION_BOX, "RESOLUTION_BOX");
					put(Melville.CSJ2K.j2k.fileformat.FileFormatBoxes.URL_BOX, "URL_BOX");
					put(Melville.CSJ2K.j2k.fileformat.FileFormatBoxes.UUID_BOX, "UUID_BOX");
					put(Melville.CSJ2K.j2k.fileformat.FileFormatBoxes.UUID_INFO_BOX, "UUID_INFO_BOX");
					put(Melville.CSJ2K.j2k.fileformat.FileFormatBoxes.UUID_LIST_BOX, "UUID_LIST_BOX");
					put(Melville.CSJ2K.j2k.fileformat.FileFormatBoxes.XML_BOX, "XML_BOX");
				}
			}
		}
		
		/* end class JP2Box */
	}
}