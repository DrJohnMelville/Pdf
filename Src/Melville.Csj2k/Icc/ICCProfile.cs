/// <summary>
/// **************************************************************************
/// 
/// $Id: ICCProfile.java,v 1.1 2002/07/25 14:56:55 grosbois Exp $
/// 
/// Copyright Eastman Kodak Company, 343 State Street, Rochester, NY 14650
/// $Date $
/// ***************************************************************************
/// </summary>

namespace Melville.CSJ2K.Icc
{
    using System;
    using System.Text;

    using ParameterList = Melville.CSJ2K.j2k.util.ParameterList;
    using ColorSpace = Melville.CSJ2K.Color.ColorSpace;
    using ICCProfileHeader = Melville.CSJ2K.Icc.Types.ICCProfileHeader;
    using ICCTag = Melville.CSJ2K.Icc.Tags.ICCTag;
    using ICCTagTable = Melville.CSJ2K.Icc.Tags.ICCTagTable;
    using ICCCurveType = Melville.CSJ2K.Icc.Tags.ICCCurveType;
    using ICCXYZType = Melville.CSJ2K.Icc.Tags.ICCXYZType;
    using XYZNumber = Melville.CSJ2K.Icc.Types.XYZNumber;
    using ICCProfileVersion = Melville.CSJ2K.Icc.Types.ICCProfileVersion;
    using ICCDateTime = Melville.CSJ2K.Icc.Types.ICCDateTime;
    using FacilityManager = Melville.CSJ2K.j2k.util.FacilityManager;


    /// <summary>  This class models the ICCProfile file.  This file is a binary file which is divided 
    /// into two parts, an ICCProfileHeader followed by an ICCTagTable. The header is a 
    /// straightforward list of descriptive parameters such as profile size, version, date and various
    /// more esoteric parameters.  The tag table is a structured list of more complexly aggragated data
    /// describing things such as ICC curves, copyright information, descriptive text blocks, etc.
    /// 
    /// Classes exist to model the header and tag table and their various constituent parts the developer
    /// is refered to these for further information on the structure and contents of the header and tag table.
    /// 
    /// </summary>
    /// <seealso cref="jj2000.j2k.icc.types.ICCProfileHeader">
    /// </seealso>
    /// <seealso cref="jj2000.j2k.icc.tags.ICCTagTable">
    /// </seealso>
    /// <version> 	1.0
    /// </version>
    /// <author> 	Bruce A. Kern
    /// </author>

    public class ICCProfile
    {
        private int ProfileClass => Header!.dwProfileClass;

        private int PCSType => Header!.dwPCSType;

        private int ProfileSignature => Header!.dwProfileSignature;

        private int DeviceAttributesReserved
        {
            get
            {
                return Header!.dwDeviceAttributesReserved;
            }

            set
            {
                Header!.dwDeviceAttributesReserved = value;
            }

        }

        /// <summary> Access the profile header</summary>
        /// <returns> ICCProfileHeader
        /// </returns>
        public ICCProfileHeader? Header { get; private set; } = null;

        /// <summary> Access the profile tag table</summary>
        /// <returns> ICCTagTable
        /// </returns>
        public ICCTagTable TagTable => tags;

        private static readonly string eol = Environment.NewLine;

        /// <summary>Gray index. </summary>
        // Renamed for convenience:
        public const int GRAY = 0;

        /// <summary>RGB index.  </summary>
        public const int RED = 0;

        /// <summary>RGB index.  </summary>
        public const int GREEN = 1;

        /// <summary>RGB index.  </summary>
        public const int BLUE = 2;
        
        /* JP2 Box structure analysis help */

        /// <summary> Create an ICCProfileVersion from byte [] input</summary>
        /// <param name="data">array containing the ICCProfileVersion representation
        /// </param>
        /// <param name="offset">start of the rep in the array
        /// </param>
        /// <returns>  the created ICCProfileVersion
        /// </returns>
        public static ICCProfileVersion getICCProfileVersion(byte[] data, int offset)
        {
            byte major = data[offset];
            byte minor = data[offset + BitReaders.byte_size];
            byte resv1 = data[offset + 2 * BitReaders.byte_size];
            byte resv2 = data[offset + 3 * BitReaders.byte_size];
            return new ICCProfileVersion(major, minor, resv1, resv2);
        }
        
        /// <summary>signature    </summary>
        // Define the set of standard signature and type values
        // Because of the endian issues and byte swapping, the profile codes must
        // be stored in memory and be addressed by address. As such, only those
        // codes required for Restricted ICC use are defined here

        public static readonly int kdwProfileSignature;

        /// <summary>profile type </summary>
        public static readonly int kdwInputProfile;

        /// <summary>tag type     </summary>
        public static readonly int kdwDisplayProfile;

        /// <summary>tag type     </summary>
        public static readonly int kdwXYZData;

        /// <summary>tag signature </summary>
        public static readonly int kdwGrayTRCTag;

        /// <summary>tag signature </summary>
        public static readonly int kdwRedColorantTag;

        /// <summary>tag signature </summary>
        public static readonly int kdwGreenColorantTag;

        /// <summary>tag signature </summary>
        public static readonly int kdwBlueColorantTag;

        /// <summary>tag signature </summary>
        public static readonly int kdwRedTRCTag;

        /// <summary>tag signature </summary>
        public static readonly int kdwGreenTRCTag;

        /// <summary>tag signature </summary>
        public static readonly int kdwBlueTRCTag;


        private ICCTagTable? tags = null;

        private byte[]? profile = null;

        //private byte[] data = null;
        private ParameterList? pl = null;

        /// <summary> ParameterList constructor </summary>
        /// <param name="csb">provides colorspace information
        /// </param>
        protected internal ICCProfile(ColorSpace csm)
        {
            this.pl = csm.pl;
            profile = csm.ICCProfile;
            initProfile(profile);
        }

        /// <summary> Read the header and tags into memory and verify
        /// that the correct type of profile is being used. for encoding.
        /// </summary>
        /// <param name="data">ICCProfile
        /// </param>
        /// <exception cref="ICCProfileInvalidException">for bad signature and class and bad type
        /// </exception>
        private void initProfile(byte[] data)
        {
            Header = new ICCProfileHeader(data);
            tags = ICCTagTable.createInstance(data);


            // Verify that the data pointed to by icc is indeed a valid profile    
            // and that it is possibly of one of the Restricted ICC types. The simplest way to check    
            // this is to verify that the profile signature is correct, that it is an input profile,    
            // and that the PCS used is XYX.    

            // However, a common error in profiles will be to create Monitor profiles rather    
            // than input profiles. If this is the only error found, it's still useful to let this  
            // go through with an error written to stderr.  

            if (ProfileClass == kdwDisplayProfile)
            {
                string message = "NOTE!! Technically, this profile is a Display profile, not an"
                                        + " Input Profile, and thus is not a valid Restricted ICC profile."
                                        + " However, it is quite possible that this profile is usable as"
                                        + " a Restricted ICC profile, so this code will ignore this state"
                                        + " and proceed with processing.";

                FacilityManager.getMsgLogger().printmsg(CSJ2K.j2k.util.MsgLogger_Fields.WARNING, message);
            }

            if ((ProfileSignature != kdwProfileSignature)
                || ((ProfileClass != kdwInputProfile) && (ProfileClass != kdwDisplayProfile)) || (PCSType != kdwXYZData))
            {
                throw new ICCProfileInvalidException();
            }
        }


        /// <summary>Provide a suitable string representation for the class </summary>
        public override string ToString()
        {
            StringBuilder rep = new StringBuilder("[ICCProfile:");
            StringBuilder body = new StringBuilder();
            body.Append(eol).Append(Header);
            body.Append(eol).Append(eol).Append(tags);
            rep.Append(ColorSpace.indent("  ", body));
            return rep.Append("]").ToString();
        }


        /// <summary> Create a two character hex representation of a byte</summary>
        /// <param name="i">byte to represent
        /// </param>
        /// <returns> representation
        /// </returns>
        public static string toHexString(byte i)
        {
            string rep = (i >= 0 && i < 16 ? "0" : "") + System.Convert.ToString((int)i, 16);
            if (rep.Length > 2) rep = rep.Substring(rep.Length - 2);
            return rep;
        }

        /// <summary> Create a 4 character hex representation of a short</summary>
        /// <param name="i">short to represent
        /// </param>
        /// <returns> representation
        /// </returns>
        public static string toHexString(short i)
        {
            string rep;

            if (i >= 0 && i < 0x10) rep = "000" + System.Convert.ToString((int)i, 16);
            else if (i >= 0 && i < 0x100) rep = "00" + System.Convert.ToString((int)i, 16);
            else if (i >= 0 && i < 0x1000) rep = "0" + System.Convert.ToString((int)i, 16);
            else rep = "" + System.Convert.ToString((int)i, 16);

            if (rep.Length > 4) rep = rep.Substring(rep.Length - 4);
            return rep;
        }


        /// <summary> Create a 8 character hex representation of a int</summary>
        /// <param name="i">int to represent
        /// </param>
        /// <returns> representation
        /// </returns>
        public static string toHexString(int i)
        {
            string rep;

            if (i >= 0 && i < 0x10) rep = "0000000" + Convert.ToString((int)i, 16);
            else if (i >= 0 && i < 0x100) rep = "000000" + Convert.ToString((int)i, 16);
            else if (i >= 0 && i < 0x1000) rep = "00000" + Convert.ToString((int)i, 16);
            else if (i >= 0 && i < 0x10000) rep = "0000" + Convert.ToString((int)i, 16);
            else if (i >= 0 && i < 0x100000) rep = "000" + Convert.ToString((int)i, 16);
            else if (i >= 0 && i < 0x1000000) rep = "00" + Convert.ToString((int)i, 16);
            else if (i >= 0 && i < 0x10000000) rep = "0" + Convert.ToString((int)i, 16);
            else rep = "" + Convert.ToString((int)i, 16);

            if (rep.Length > 8) rep = rep.Substring(rep.Length - 8);
            return rep;
        }

        public static string ToString(byte[] data)
        {

            int i, row, col, rem, rows, cols;

            StringBuilder rep = new StringBuilder();
            StringBuilder rep0 = null;
            StringBuilder rep1 = null;
            StringBuilder rep2 = null;

            cols = 16;
            rows = data.Length / cols;
            rem = data.Length % cols;

            byte[] lbytes = new byte[8];
            for (row = 0, i = 0; row < rows; ++row)
            {
                rep1 = new StringBuilder();
                rep2 = new StringBuilder();

                for (i = 0; i < 8; ++i) lbytes[i] = 0;
                byte[] tbytes = Encoding.UTF8.GetBytes(Convert.ToString(row * 16, 16));
                for (int t = 0, l = lbytes.Length - tbytes.Length; t < tbytes.Length; ++l, ++t) lbytes[l] = tbytes[t];

                rep0 = new StringBuilder(new string(SupportClass.ToCharArray(lbytes)));

                for (col = 0; col < cols; ++col)
                {
                    byte b = data[i++];
                    rep1.Append(toHexString(b)).Append(i % 2 == 0 ? " " : "");
                    if ((char.IsLetter((char)b) || ((char)b).CompareTo('$') == 0 || ((char)b).CompareTo('_') == 0)) rep2.Append((char)b);
                    else rep2.Append(".");
                }
                rep.Append(rep0).Append(" :  ").Append(rep1).Append(":  ").Append(rep2).Append(eol);
            }

            rep1 = new System.Text.StringBuilder();
            rep2 = new System.Text.StringBuilder();

            for (i = 0; i < 8; ++i) lbytes[i] = 0;
            byte[] tbytes2 = System.Text.Encoding.UTF8.GetBytes(System.Convert.ToString(row * 16, 16));
            for (int t = 0, l = lbytes.Length - tbytes2.Length; t < tbytes2.Length; ++l, ++t) lbytes[l] = tbytes2[t];

            rep0 = new StringBuilder(System.Text.Encoding.UTF8.GetString(lbytes, 0, lbytes.Length));

            for (col = 0; col < rem; ++col)
            {
                byte b = data[i++];
                rep1.Append(toHexString(b)).Append(i % 2 == 0 ? " " : "");
                if ((char.IsLetter((char)b) || ((char)b).CompareTo('$') == 0 || ((char)b).CompareTo('_') == 0)) rep2.Append((char)b);
                else rep2.Append(".");
            }
            for (col = rem; col < 16; ++col) rep1.Append("  ").Append(col % 2 == 0 ? " " : "");

            rep.Append(rep0).Append(" :  ").Append(rep1).Append(":  ").Append(rep2).Append(eol);

            return rep.ToString();
        }

        /// <summary> Parse this ICCProfile into a RestrictedICCProfile
        /// which is appropriate to the data in this profile.
        /// Either a MonochromeInputRestrictedProfile or 
        /// MatrixBasedRestrictedProfile is returned
        /// </summary>
        /// <returns> RestrictedICCProfile
        /// </returns>
        /// <exception cref="ICCProfileInvalidException">no curve data
        /// </exception>
        public RestrictedICCProfile parse()
        {
            // The next step is to determine which Restricted ICC type is used by this profile.
            // Unfortunately, the only way to do this is to look through the tag table for
            // the tags required by the two types.

            // First look for the gray TRC tag. If the profile is indeed an input profile, and this
            // tag exists, then the profile is a Monochrome Input profile
            ICCTag grayTag;
            if (tags.TryGetValue(kdwGrayTRCTag, out grayTag))
            {
                return RestrictedICCProfile.createInstance((ICCCurveType)grayTag);
            }

            // If it wasn't a Monochrome Input profile, look for the Red Colorant tag. If that
            // tag is found and the profile is indeed an input profile, then this profile is
            // a Three-Component Matrix-Based Input profile
            ICCCurveType rTRCTag = (ICCCurveType)tags[(System.Int32)kdwRedTRCTag];


            if (rTRCTag != null)
            {
                ICCCurveType gTRCTag = (ICCCurveType)tags[kdwGreenTRCTag];
                ICCCurveType bTRCTag = (ICCCurveType)tags[kdwBlueTRCTag];
                ICCXYZType rColorantTag = (ICCXYZType)tags[kdwRedColorantTag];
                ICCXYZType gColorantTag = (ICCXYZType)tags[kdwGreenColorantTag];
                ICCXYZType bColorantTag = (ICCXYZType)tags[kdwBlueColorantTag];
                return RestrictedICCProfile.createInstance(
                    rTRCTag,
                    gTRCTag,
                    bTRCTag,
                    rColorantTag,
                    gColorantTag,
                    bColorantTag);
            }

            throw new ICCProfileInvalidException("curve data not found in profile");
        }


        /* end class ICCProfile */

        static ICCProfile()
        {
            kdwProfileSignature = GetTagInt("acsp");
            kdwInputProfile = GetTagInt("scnr");
            kdwDisplayProfile = GetTagInt("mntr");
            kdwXYZData = GetTagInt("XYZ ");
            kdwGrayTRCTag = GetTagInt("kTRC");
            kdwRedColorantTag = GetTagInt("rXYZ");
            kdwGreenColorantTag = GetTagInt("gXYZ");
            kdwBlueColorantTag = GetTagInt("bXYZ");
            kdwRedTRCTag = GetTagInt("rTRC");
            kdwGreenTRCTag = GetTagInt("gTRC");
            kdwBlueTRCTag = GetTagInt("bTRC");
        }

        private static int GetTagInt(string tag)
        {
            byte[] tagBytes = Encoding.UTF8.GetBytes(tag);
            Array.Reverse(tagBytes);
            return BitConverter.ToInt32(tagBytes, 0);
        }
    }
}
