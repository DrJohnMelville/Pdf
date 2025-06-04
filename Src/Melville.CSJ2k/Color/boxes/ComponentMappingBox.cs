/// <summary>**************************************************************************
/// 
/// $Id: ComponentMappingBox.java,v 1.1 2002/07/25 14:50:46 grosbois Exp $
/// 
/// Copyright Eastman Kodak Company, 343 State Street, Rochester, NY 14650
/// $Date $
/// ***************************************************************************
/// </summary>
using System;
using System.Collections.Generic;
using ICCProfile = CoreJ2K.Icc.ICCProfile;
using io_RandomAccessIO = CoreJ2K.j2k.io.RandomAccessIO;

namespace CoreJ2K.Color.Boxes
{
	
	/// <summary> This class maps the components in the codestream
	/// to channels in the image.  It models the Component
	/// Mapping box in the JP2 header.
	/// 
	/// </summary>
	/// <version> 	1.0
	/// </version>
	/// <author> 	Bruce A. Kern
	/// </author>
	public sealed class ComponentMappingBox:JP2Box
	{
		public int NChannels { get; private set; }

		private List<byte[]> map = new List<byte[]>(10);
		
		/// <summary> Construct a ComponentMappingBox from an input image.</summary>
		/// <param name="in">RandomAccessIO jp2 image
		/// </param>
		/// <param name="boxStart">offset to the start of the box in the image
		/// </param>
		/// <exception cref="IOException,">ColorSpaceException 
		/// </exception>
		public ComponentMappingBox(io_RandomAccessIO in_Renamed, int boxStart):base(in_Renamed, boxStart)
		{
			readBox();
		}
		
		/// <summary>Analyze the box content. </summary>
		internal void  readBox()
		{
			NChannels = (boxEnd - dataStart) / 4;
			in_Renamed.seek(dataStart);
			for (var offset = dataStart; offset < boxEnd; offset += 4)
			{
				var mapping = new byte[4];
				in_Renamed.readFully(mapping, 0, 4);
				map.Add(mapping);
			}
		}
		
		/* Return the component mapped to the channel. */
		public int getCMP(int channel)
		{
			var mapping = map[channel];
			return ICCProfile.getShort(mapping, 0) & 0x0000ffff;
		}
		
		/// <summary>Return the channel type. </summary>
		public short getMTYP(int channel)
		{
			var mapping = map[channel];
			return (short) (mapping[2] & 0x00ff);
		}
		
		/// <summary>Return the palette index for the channel. </summary>
		public short getPCOL(int channel)
		{
			var mapping = map[channel];
			return (short) (mapping[3] & 0x000ff);
		}
		
		/// <summary>Return a suitable String representation of the class instance. </summary>
		public override string ToString()
		{
			var rep = new System.Text.StringBuilder("[ComponentMappingBox ").Append("  ");
			rep.Append("nChannels= ").Append(Convert.ToString(NChannels));
			System.Collections.IEnumerator Enum = map.GetEnumerator();
			//UPGRADE_TODO: Method 'java.util.Enumeration.hasMoreElements' was converted to 'System.Collections.IEnumerator.MoveNext' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javautilEnumerationhasMoreElements'"
			while (Enum.MoveNext())
			{
				//UPGRADE_TODO: Method 'java.util.Enumeration.nextElement' was converted to 'System.Collections.IEnumerator.Current' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javautilEnumerationnextElement'"
				var bfr = (byte[]) Enum.Current;
				rep.Append(Environment.NewLine).Append("  ").Append("CMP= ").Append(Convert.ToString(getCMP(bfr))).Append(", ");
				rep.Append("MTYP= ").Append(Convert.ToString(getMTYP(bfr))).Append(", ");
				rep.Append("PCOL= ").Append(Convert.ToString(getPCOL(bfr)));
			}
			rep.Append("]");
			return rep.ToString();
		}
		
		private int getCMP(byte[] mapping)
		{
			return ICCProfile.getShort(mapping, 0) & 0x0000ffff;
		}
		
		private short getMTYP(byte[] mapping)
		{
			return (short) (mapping[2] & 0x00ff);
		}
		
		private short getPCOL(byte[] mapping)
		{
			return (short) (mapping[3] & 0x000ff);
		}
		
		/* end class ComponentMappingBox */
		static ComponentMappingBox()
		{
			{
				type = 0x636d6170;
			}
		}
	}
}