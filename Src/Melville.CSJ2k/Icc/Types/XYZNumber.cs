/// <summary>**************************************************************************
/// 
/// $Id: XYZNumber.java,v 1.1 2002/07/25 14:56:31 grosbois Exp $
/// 
/// Copyright Eastman Kodak Company, 343 State Street, Rochester, NY 14650
/// $Date $
/// ***************************************************************************
/// </summary>
using System;

namespace CoreJ2K.Icc.Types
{
	
	/// <summary> A convientient representation for the contents of the
	/// ICCXYZTypeTag class.
	/// 
	/// </summary>
	/// <seealso cref="j2k.icc.tags.ICCXYZType" />
	/// <version> 	1.0
	/// </version>
	/// <author> 	Bruce A. Kern
	/// </author>
	public class XYZNumber
	{
		public static readonly int size;
		
		/// <summary>x value </summary>
		public int dwX;
		/// <summary>y value </summary>
		// X tristimulus value
		public int dwY;
		/// <summary>z value </summary>
		// Y tristimulus value
		public int dwZ; // Z tristimulus value
		
		/// <summary>Construct from constituent parts. </summary>
		public XYZNumber(int x, int y, int z)
		{
			dwX = x; dwY = y; dwZ = z;
		}
		
		/// <summary>Normalization utility </summary>
		public static int DoubleToXYZ(double x)
		{
			//UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
			return (int) Math.Floor(x * 65536.0 + 0.5);
		}
		
		/// <summary>Normalization utility </summary>
		public static double XYZToDouble(int x)
		{
			return x / 65536.0;
		}
		
		/// <summary>Write to a file </summary>
		//UPGRADE_TODO: Class 'java.io.RandomAccessFile' was converted to 'System.IO.FileStream' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javaioRandomAccessFile'"
		public virtual void  write(System.IO.Stream raf)
		{
			System.IO.BinaryWriter temp_BinaryWriter;
			temp_BinaryWriter = new System.IO.BinaryWriter(raf);
			temp_BinaryWriter.Write(dwX);
			System.IO.BinaryWriter temp_BinaryWriter2;
			temp_BinaryWriter2 = new System.IO.BinaryWriter(raf);
			temp_BinaryWriter2.Write(dwY);
			System.IO.BinaryWriter temp_BinaryWriter3;
			temp_BinaryWriter3 = new System.IO.BinaryWriter(raf);
			temp_BinaryWriter3.Write(dwZ);
		}
		
		/// <summary>String representation of class instance. </summary>
		public override string ToString()
		{
			return $"[{dwX}, {dwY}, {dwZ}]";
		}
		
		
		/* end class XYZNumber */
		static XYZNumber()
		{
			size = 3 * ICCProfile.int_size;
		}
	}
}