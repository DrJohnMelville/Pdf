// Copyright (c) 2007-2016 CSJ2K contributors.
// Licensed under the BSD 3-Clause License.

/*
 * JJ2000 COPYRIGHT:
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
 */

namespace CoreJ2K.j2k.util
{
    using System.IO;

    /// <summary> This class implements the MsgLogger interface for streams. Streams can
    /// be simple files, terminals, stdout, stderr, etc. The messages or simple
    /// strings are formatted using the linewidth given to the constructor.
    /// Messages are printed to the 'err' stream if they are of severity WARNING
    /// or ERROR, otherwise they are printed to the 'out' stream. Simple strings
    /// are always printed the 'out' stream.
    /// </summary>
    public abstract class StreamMsgLogger : IMsgLogger
    {
        /// <summary>The 'out' stream </summary>
        private StreamWriter out_Renamed;

        /// <summary>The 'err' stream </summary>
        private readonly StreamWriter err;

        /// <summary>The printer that formats the text </summary>
        private readonly MsgPrinter mp;

        /// <summary> Constructs a StreamMsgLogger that uses 'outstr' as the 'out' stream,
        /// and 'errstr' as the 'err' stream. Note that 'outstr' and 'errstr' can
        /// be System.out and System.err.
        /// </summary>
        /// <param name="outstr">Where to print simple strings and LOG and INFO messages.
        /// </param>
        /// <param name="errstr">Where to print WARNING and ERROR messages
        /// </param>
        /// <param name="lw">The line width to use in formatting
        /// </param>
        protected StreamMsgLogger(Stream outstr, Stream errstr, int lw)
        {
            var temp_writer = new StreamWriter(outstr, System.Text.Encoding.UTF8);
            temp_writer.AutoFlush = true;
            out_Renamed = temp_writer;
            var temp_writer2 = new StreamWriter(errstr, System.Text.Encoding.UTF8);
            temp_writer2.AutoFlush = true;
            err = temp_writer2;
            mp = new MsgPrinter(lw);
        }

        /// <summary> Constructs a StreamMsgLogger that uses 'outstr' as the 'out' stream,
        /// and 'errstr' as the 'err' stream. Note that 'outstr' and 'errstr' can
        /// be System.out and System.err.
        /// </summary>
        /// <param name="outstr">Where to print simple strings and LOG and INFO messages.
        /// </param>
        /// <param name="errstr">Where to print WARNING and ERROR messages
        /// </param>
        /// <param name="lw">The line width to use in formatting
        /// </param>
        protected StreamMsgLogger(StreamWriter outstr, StreamWriter errstr, int lw)
        {
            var temp_writer = new StreamWriter(outstr.BaseStream, outstr.Encoding);
            temp_writer.AutoFlush = true;
            out_Renamed = temp_writer;
            var temp_writer2 = new StreamWriter(errstr.BaseStream, errstr.Encoding);
            temp_writer2.AutoFlush = true;
            err = temp_writer2;
            mp = new MsgPrinter(lw);
        }

        /// <summary> Prints the message 'msg' to the output device, appending a newline,
        /// with severity 'sev'. The severity of the message is prepended to the
        /// message.
        /// </summary>
        /// <param name="sev">The message severity (LOG, INFO, etc.)
        /// </param>
        /// <param name="msg">The message to display
        /// </param>
        public virtual void printmsg(int sev, string msg)
        {
            StreamWriter lout;
            string prefix;

            switch (sev)
            {

                case MsgLogger_Fields.LOG:
                    prefix = "[LOG]: ";
                    lout = out_Renamed;
                    break;

                case MsgLogger_Fields.INFO:
                    prefix = "[INFO]: ";
                    lout = out_Renamed;
                    break;

                case MsgLogger_Fields.WARNING:
                    prefix = "[WARNING]: ";
                    lout = err;
                    break;

                case MsgLogger_Fields.ERROR:
                    prefix = "[ERROR]: ";
                    lout = err;
                    break;

                default:
                    throw new System.ArgumentException($"Severity {sev} not valid.");

            }

            mp.print(lout, 0, prefix.Length, $"{prefix}{msg}");
            lout.Flush();
        }

        /// <summary> Prints the string 'str' to the 'out' stream, appending a newline. The
        /// message is reformatted to the line width given to the constructors and
        /// using 'flind' characters to indent the first line and 'ind' characters
        /// to indent the second line. However, any newlines appearing in 'str' are
        /// respected. The output device may or may not display the string until
        /// flush() is called, depending on the autoflush state of the PrintWriter,
        /// to be sure flush() should be called to write the string to the
        /// device. This method just prints the string, the string does not make
        /// part of a "message" in the sense that noe severity is associated to it.
        /// </summary>
        /// <param name="str">The string to print
        /// </param>
        /// <param name="flind">Indentation of the first line
        /// </param>
        /// <param name="ind">Indentation of any other lines.
        /// </param>
        public virtual void println(string str, int flind, int ind)
        {
            mp.print(out_Renamed, flind, ind, str);
        }

        /// <summary> Writes any buffered data from the print() and println() methods to the
        /// device.
        /// </summary>
        public virtual void flush()
        {
            out_Renamed.Flush();
        }
    }
}
