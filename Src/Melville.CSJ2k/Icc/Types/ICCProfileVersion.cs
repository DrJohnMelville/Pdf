/// <summary>**************************************************************************
/// 
/// $Id: ICCProfileVersion.java,v 1.1 2002/07/25 14:56:31 grosbois Exp $
/// 
/// Copyright Eastman Kodak Company, 343 State Street, Rochester, NY 14650
/// $Date $
/// ***************************************************************************
/// </summary>

namespace CoreJ2K.Icc.Types
{
	
	/// <summary> This class describes the ICCProfile Version as contained in
	/// the header of the ICC Profile.
	/// 
	/// </summary>
	/// <seealso cref="j2k.icc.ICCProfile" />
	/// <seealso cref="j2k.icc.types.ICCProfileHeader" />
	/// <version> 	1.0
	/// </version>
	/// <author> 	Bruce A. Kern
	/// </author>
	public class ICCProfileVersion
	{
		/// <summary>Field size </summary>
		public static readonly int size;
		
		/// <summary>Major revision number in binary coded decimal </summary>
		public byte uMajor;
		/// <summary>Minor revision in high nibble, bug fix revision           
		/// in low nibble, both in binary coded decimal   
		/// </summary>
		public byte uMinor;
		
		private byte reserved1;
		private byte reserved2;
		
		/// <summary>Construct from constituent parts. </summary>
		public ICCProfileVersion(byte major, byte minor, byte res1, byte res2)
		{
			uMajor = major; uMinor = minor; reserved1 = res1; reserved2 = res2;
		}
		
		/// <summary>Construct from file content. </summary>
		//UPGRADE_TODO: Class 'java.io.RandomAccessFile' was converted to 'System.IO.FileStream' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javaioRandomAccessFile'"
		public virtual void  write(System.IO.Stream raf)
		{
			raf.WriteByte(uMajor); raf.WriteByte(uMinor); raf.WriteByte(reserved1); raf.WriteByte(reserved2);
		}
		
		/// <summary>String representation of class instance. </summary>
		public override string ToString()
		{
			return $"Version {uMajor}.{uMinor}";
		}
		
		/* end class ICCProfileVersion */
		static ICCProfileVersion()
		{
			size = 4 * ICCProfile.byte_size;
		}
	}
}