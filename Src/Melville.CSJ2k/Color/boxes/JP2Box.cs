/// <summary>**************************************************************************
/// 
/// $Id: JP2Box.java,v 1.1 2002/07/25 14:50:47 grosbois Exp $
/// 
/// Copyright Eastman Kodak Company, 343 State Street, Rochester, NY 14650
/// $Date $
/// ***************************************************************************
/// </summary>

using System.Collections.Generic;
using FileFormatBoxes = CoreJ2K.j2k.fileformat.FileFormatBoxes;
using ICCProfile = CoreJ2K.Icc.ICCProfile;
using io_RandomAccessIO = CoreJ2K.j2k.io.RandomAccessIO;

namespace CoreJ2K.Color.Boxes
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
		/// <summary>Box type                           </summary>
		public static int type;
		
		/// <summary>Return a String representation of the Box type. </summary>
		public static string getTypeString(int t)
		{
			return BoxType.get_Renamed(t);
		}
		
		/// <summary>Length of the box.             </summary>
		public int length;
		/// <summary>input file                     </summary>
		protected internal io_RandomAccessIO in_Renamed;
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
		public JP2Box(io_RandomAccessIO in_Renamed, int boxStart)
		{
			var boxHeader = new byte[16];
			
			this.in_Renamed = in_Renamed;
			this.boxStart = boxStart;
			
			this.in_Renamed.seek(this.boxStart);
			this.in_Renamed.readFully(boxHeader, 0, 8);
			
			dataStart = boxStart + 8;
            length = ICCProfile.getInt(boxHeader, 0);
			boxEnd = boxStart + length;
			if (length == 1)
				throw new ColorSpaceException("extended length boxes not supported");
		}
		
		
		/// <summary>Return the box type as a String. </summary>
		public virtual string getTypeString()
		{
			return BoxType.get_Renamed(type);
		}
		
		
		/// <summary>JP2 Box structure analysis help </summary>
		protected internal class BoxType:Dictionary<int, string>
		{

			private static Dictionary<int, string> map = new Dictionary<int, string>();
			
			private static void  put(int type, string desc)
			{
				map[type] = desc;
			}
			
			public static string get_Renamed(int type)
			{
				return map[type];
			}
			
			/* end class BoxType */
			static BoxType()
			{
				{
					put(FileFormatBoxes.BITS_PER_COMPONENT_BOX, "BITS_PER_COMPONENT_BOX");
					put(FileFormatBoxes.CAPTURE_RESOLUTION_BOX, "CAPTURE_RESOLUTION_BOX");
					put(FileFormatBoxes.CHANNEL_DEFINITION_BOX, "CHANNEL_DEFINITION_BOX");
					put(FileFormatBoxes.COLOUR_SPECIFICATION_BOX, "COLOUR_SPECIFICATION_BOX");
					put(FileFormatBoxes.COMPONENT_MAPPING_BOX, "COMPONENT_MAPPING_BOX");
					put(FileFormatBoxes.CONTIGUOUS_CODESTREAM_BOX, "CONTIGUOUS_CODESTREAM_BOX");
					put(FileFormatBoxes.DEFAULT_DISPLAY_RESOLUTION_BOX, "DEFAULT_DISPLAY_RESOLUTION_BOX");
					put(FileFormatBoxes.FILE_TYPE_BOX, "FILE_TYPE_BOX");
					put(FileFormatBoxes.IMAGE_HEADER_BOX, "IMAGE_HEADER_BOX");
					put(FileFormatBoxes.INTELLECTUAL_PROPERTY_BOX, "INTELLECTUAL_PROPERTY_BOX");
					put(FileFormatBoxes.JP2_HEADER_BOX, "JP2_HEADER_BOX");
					put(FileFormatBoxes.JP2_SIGNATURE_BOX, "JP2_SIGNATURE_BOX");
					put(FileFormatBoxes.PALETTE_BOX, "PALETTE_BOX");
					put(FileFormatBoxes.RESOLUTION_BOX, "RESOLUTION_BOX");
					put(FileFormatBoxes.URL_BOX, "URL_BOX");
					put(FileFormatBoxes.UUID_BOX, "UUID_BOX");
					put(FileFormatBoxes.UUID_INFO_BOX, "UUID_INFO_BOX");
					put(FileFormatBoxes.UUID_LIST_BOX, "UUID_LIST_BOX");
					put(FileFormatBoxes.XML_BOX, "XML_BOX");
				}
			}
		}
		
		/* end class JP2Box */
	}
}