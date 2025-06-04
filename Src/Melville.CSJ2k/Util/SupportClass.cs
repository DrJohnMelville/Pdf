//
// In order to convert some functionality to Visual C#, the Java Language Conversion Assistant
// creates "support classes" that duplicate the original functionality.  
//
// Support classes replicate the functionality of the original code, but in some cases they are 
// substantially different architecturally. Although every effort is made to preserve the 
// original architecture of the application in the converted project, the user should be aware that 
// the primary goal of these support classes is to replicate functionality, and that at times 
// the architecture of the resulting solution may differ somewhat.
//

using System;
using CoreJ2K.Util;
using CoreJ2K.j2k.util;

/// <summary>
/// Contains conversion support elements such as classes, interfaces and static methods.
/// </summary>
internal class SupportClass
{
	/// <summary>
	/// Converts an array of sbytes to an array of bytes
	/// </summary>
	/// <param name="sbyteArray">The array of sbytes to be converted</param>
	/// <returns>The new array of bytes</returns>
	public static byte[] ToByteArray(sbyte[] sbyteArray)
	{
		byte[] byteArray = null;

		if (sbyteArray != null)
		{
			byteArray = new byte[sbyteArray.Length];
			for(var index=0; index < sbyteArray.Length; index++)
				byteArray[index] = (byte) sbyteArray[index];
		}
		return byteArray;
	}

	/// <summary>
	/// Converts a string to an array of bytes
	/// </summary>
	/// <param name="sourceString">The string to be converted</param>
	/// <returns>The new array of bytes</returns>
	public static byte[] ToByteArray(string sourceString)
	{
		return System.Text.Encoding.UTF8.GetBytes(sourceString);
	}

	/// <summary>
	/// Converts a array of object-type instances to a byte-type array.
	/// </summary>
	/// <param name="tempObjectArray">Array to convert.</param>
	/// <returns>An array of byte type elements.</returns>
	public static byte[] ToByteArray(object[] tempObjectArray)
	{
		byte[] byteArray = null;
		if (tempObjectArray != null)
		{
			byteArray = new byte[tempObjectArray.Length];
			for (var index = 0; index < tempObjectArray.Length; index++)
				byteArray[index] = (byte)tempObjectArray[index];
		}
		return byteArray;
	}

	/*******************************/

	/// <summary>
	/// Writes the exception stack trace to the received stream
	/// </summary>
	/// <param name="throwable">Exception to obtain information from</param>
	public static void WriteStackTrace(Exception throwable)
	{
		FacilityManager.getMsgLogger().printmsg(MsgLogger_Fields.ERROR, throwable.StackTrace);
	}

	/*******************************/
	/// <summary>
	/// Receives a byte array and returns it transformed in an sbyte array
	/// </summary>
	/// <param name="byteArray">Byte array to process</param>
	/// <returns>The transformed array</returns>
	public static sbyte[] ToSByteArray(byte[] byteArray)
	{
		sbyte[] sbyteArray = null;
		if (byteArray != null)
		{
			sbyteArray = new sbyte[byteArray.Length];
			for(var index=0; index < byteArray.Length; index++)
				sbyteArray[index] = (sbyte) byteArray[index];
		}
		return sbyteArray;
	}

	/*******************************/
	/// <summary>
	/// Converts an array of sbytes to an array of chars
	/// </summary>
	/// <param name="sByteArray">The array of sbytes to convert</param>
	/// <returns>The new array of chars</returns>
	public static char[] ToCharArray(sbyte[] sByteArray) 
	{
		return System.Text.Encoding.UTF8.GetChars(ToByteArray(sByteArray));
	}

	/// <summary>
	/// Converts an array of bytes to an array of chars
	/// </summary>
	/// <param name="byteArray">The array of bytes to convert</param>
	/// <returns>The new array of chars</returns>
	public static char[] ToCharArray(byte[] byteArray) 
	{
		return System.Text.Encoding.UTF8.GetChars(byteArray);
	}

	/*******************************/
	/// <summary>
	/// This method returns the literal value received
	/// </summary>
	/// <param name="literal">The literal to return</param>
	/// <returns>The received value</returns>
	public static long Identity(long literal)
	{
		return literal;
	}

	/// <summary>
	/// This method returns the literal value received
	/// </summary>
	/// <param name="literal">The literal to return</param>
	/// <returns>The received value</returns>
	public static ulong Identity(ulong literal)
	{
		return literal;
	}

	/// <summary>
	/// This method returns the literal value received
	/// </summary>
	/// <param name="literal">The literal to return</param>
	/// <returns>The received value</returns>
	public static float Identity(float literal)
	{
		return literal;
	}

	/// <summary>
	/// This method returns the literal value received
	/// </summary>
	/// <param name="literal">The literal to return</param>
	/// <returns>The received value</returns>
	public static double Identity(double literal)
	{
		return literal;
	}

	/*******************************/
	/// <summary>
	/// Provides support functions to create read-write random acces files and write functions
	/// </summary>
	public class RandomAccessFileSupport
	{
		/// <summary>
		/// Creates a new random acces stream with read-write or read rights
		/// </summary>
		/// <param name="fileName">A relative or absolute path for the file to open</param>
		/// <param name="mode">Mode to open the file in</param>
		/// <returns>The new System.IO.FileStream</returns>
		public static System.IO.Stream CreateRandomAccessFile(string fileName, string mode)
		{
			return FileStreamFactory.New(fileName, mode);
		}

		/// <summary>
		/// Creates a new random acces stream with read-write or read rights
		/// </summary>
		/// <param name="fileName">File infomation for the file to open</param>
		/// <param name="mode">Mode to open the file in</param>
		/// <returns>The new System.IO.FileStream</returns>
		public static System.IO.Stream CreateRandomAccessFile(IFileInfo fileName, string mode)
		{
			return CreateRandomAccessFile(fileName.FullName, mode);
		} 

		/// <summary>
		/// Writes the data to the specified file stream
		/// </summary>
		/// <param name="data">Data to write</param>
		/// <param name="fileStream">File to write to</param>
		public static void WriteBytes(string data,System.IO.Stream fileStream)
		{
			var index = 0;
			var length = data.Length;

			while(index < length)
				fileStream.WriteByte((byte)data[index++]);	
		}

		/// <summary>
		/// Writes the received string to the file stream
		/// </summary>
		/// <param name="data">String of information to write</param>
		/// <param name="fileStream">File to write to</param>
		public static void WriteChars(string data,System.IO.Stream fileStream)
		{
			WriteBytes(data, fileStream);	
		}

		/// <summary>
		/// Writes the received data to the file stream
		/// </summary>
		/// <param name="sByteArray">Data to write</param>
		/// <param name="fileStream">File to write to</param>
		public static void WriteRandomFile(sbyte[] sByteArray,System.IO.Stream fileStream)
		{
			var byteArray = ToByteArray(sByteArray);
			fileStream.Write(byteArray, 0, byteArray.Length);
		}
	}

	/*******************************/
	/// <summary>
	/// Performs an unsigned bitwise right shift with the specified number
	/// </summary>
	/// <param name="number">Number to operate on</param>
	/// <param name="bits">Ammount of bits to shift</param>
	/// <returns>The resulting number from the shift operation</returns>
	public static int URShift(int number, int bits)
	{
        if ( number >= 0)
			return number >> bits;
		else
			return (number >> bits) + (2 << ~bits);
	}

	/// <summary>
	/// Performs an unsigned bitwise right shift with the specified number
	/// </summary>
	/// <param name="number">Number to operate on</param>
	/// <param name="bits">Ammount of bits to shift</param>
	/// <returns>The resulting number from the shift operation</returns>
	public static int URShift(int number, long bits)
	{
		return URShift(number, (int)bits);
	}

	/// <summary>
	/// Performs an unsigned bitwise right shift with the specified number
	/// </summary>
	/// <param name="number">Number to operate on</param>
	/// <param name="bits">Ammount of bits to shift</param>
	/// <returns>The resulting number from the shift operation</returns>
	public static long URShift(long number, int bits)
	{
		if (number >= 0)
			return number >> bits;
		else
			return (number >> bits) + (2L << ~bits);
	}

	/// <summary>
	/// Performs an unsigned bitwise right shift with the specified number
	/// </summary>
	/// <param name="number">Number to operate on</param>
	/// <param name="bits">Ammount of bits to shift</param>
	/// <returns>The resulting number from the shift operation</returns>
	public static long URShift(long number, long bits)
	{
		return URShift(number, (int)bits);
	}

	/*******************************/
	/// <summary>Reads a number of characters from the current source Stream and writes the data to the target array at the specified index.</summary>
	/// <param name="sourceStream">The source Stream to read from.</param>
	/// <param name="target">Contains the array of characteres read from the source Stream.</param>
	/// <param name="start">The starting index of the target array.</param>
	/// <param name="count">The maximum number of characters to read from the source Stream.</param>
	/// <returns>The number of characters read. The number will be less than or equal to count depending on the data available in the source Stream. Returns -1 if the end of the stream is reached.</returns>
	public static int ReadInput(System.IO.Stream sourceStream, sbyte[] target, int start, int count)
	{
		// Returns 0 bytes if not enough space in target
		if (target.Length == 0)
			return 0;

		var receiver = new byte[target.Length];
		var bytesRead   = sourceStream.Read(receiver, start, count);

		// Returns -1 if EOF
		if (bytesRead == 0)	
			return -1;
                
		for(var i = start; i < start + bytesRead; i++)
			target[i] = (sbyte)receiver[i];
                
		return bytesRead;
	}

	/// <summary>Reads a number of characters from the current source TextReader and writes the data to the target array at the specified index.</summary>
	/// <param name="sourceTextReader">The source TextReader to read from</param>
	/// <param name="target">Contains the array of characteres read from the source TextReader.</param>
	/// <param name="start">The starting index of the target array.</param>
	/// <param name="count">The maximum number of characters to read from the source TextReader.</param>
	/// <returns>The number of characters read. The number will be less than or equal to count depending on the data available in the source TextReader. Returns -1 if the end of the stream is reached.</returns>
	public static int ReadInput(System.IO.TextReader sourceTextReader, sbyte[] target, int start, int count)
	{
		// Returns 0 bytes if not enough space in target
		if (target.Length == 0) return 0;

		var charArray = new char[target.Length];
		var bytesRead = sourceTextReader.Read(charArray, start, count);

		// Returns -1 if EOF
		if (bytesRead == 0) return -1;

		for(var index=start; index<start+bytesRead; index++)
			target[index] = (sbyte)charArray[index];

		return bytesRead;
	}

	/*******************************/
	/// <summary>
	/// The class performs token processing in strings
	/// </summary>
	public class Tokenizer: System.Collections.IEnumerator
	{
		/// Position over the string
		private long currentPos = 0;

		/// Include delimiters in the results.
		private bool includeDelims = false;

		/// Char representation of the String to tokenize.
		private char[] chars = null;
			
		//The tokenizer uses the default delimiter set: the space character, the tab character, the newline character, and the carriage-return character and the form-feed character
		private string delimiters = " \t\n\r\f";		

		/// <summary>
		/// Initializes a new class instance with a specified string to process
		/// </summary>
		/// <param name="source">String to tokenize</param>
		public Tokenizer(string source)
		{			
			chars = source.ToCharArray();
		}

		/// <summary>
		/// Initializes a new class instance with a specified string to process
		/// and the specified token delimiters to use
		/// </summary>
		/// <param name="source">String to tokenize</param>
		/// <param name="delimiters">String containing the delimiters</param>
		public Tokenizer(string source, string delimiters):this(source)
		{			
			this.delimiters = delimiters;
		}


		/// <summary>
		/// Initializes a new class instance with a specified string to process, the specified token 
		/// delimiters to use, and whether the delimiters must be included in the results.
		/// </summary>
		/// <param name="source">String to tokenize</param>
		/// <param name="delimiters">String containing the delimiters</param>
		/// <param name="includeDelims">Determines if delimiters are included in the results.</param>
		public Tokenizer(string source, string delimiters, bool includeDelims):this(source,delimiters)
		{
			this.includeDelims = includeDelims;
		}	


		/// <summary>
		/// Returns the next token from the token list
		/// </summary>
		/// <returns>The string value of the token</returns>
		public string NextToken()
		{				
			return NextToken(delimiters);
		}

		/// <summary>
		/// Returns the next token from the source string, using the provided
		/// token delimiters
		/// </summary>
		/// <param name="delimiter">String containing the delimiters to use</param>
		/// <returns>The string value of the token</returns>
		public string NextToken(string delimiter)
		{
			//According to documentation, the usage of the received delimiters should be temporary (only for this call).
			//However, it seems it is not true, so the following line is necessary.
			this.delimiters = delimiter;

			//at the end 
			if (currentPos == chars.Length)
				throw new ArgumentOutOfRangeException();
			//if over a delimiter and delimiters must be returned
			else if (   (Array.IndexOf(delimiters.ToCharArray(),chars[currentPos]) != -1)
				     && includeDelims )                	
				return $"{chars[currentPos++]}";
			//need to get the token wo delimiters.
			else
				return nextToken(delimiters.ToCharArray());
		}

		//Returns the nextToken wo delimiters
		private string nextToken(char[] delimiter)
		{
			var token="";
			var pos = currentPos;

			//skip possible delimiters
			while (Array.IndexOf(delimiter,chars[currentPos]) != -1)
				//The last one is a delimiter (i.e there is no more tokens)
				if (++currentPos == chars.Length)
				{
					currentPos = pos;
					throw new ArgumentOutOfRangeException();
				}
			
			//getting the token
			while (Array.IndexOf(delimiter,chars[currentPos]) == -1)
			{
				token+=chars[currentPos];
				//the last one is not a delimiter
				if (++currentPos == chars.Length)
					break;
			}
			return token;
		}

				
		/// <summary>
		/// Determines if there are more tokens to return from the source string
		/// </summary>
		/// <returns>True or false, depending if there are more tokens</returns>
		public bool HasMoreTokens()
		{
			//keeping the current pos
			var pos = currentPos;
			
			try
			{
				NextToken();
			}
			catch (ArgumentOutOfRangeException)
			{				
				return false;
			}
			finally
			{
				currentPos = pos;
			}
			return true;
		}

		/// <summary>
		/// Remaining tokens count
		/// </summary>
		public int Count
		{
			get
			{
				//keeping the current pos
				var pos = currentPos;
				var i = 0;
			
				try
				{
					while (true)
					{
						NextToken();
						i++;
					}
				}
				catch (ArgumentOutOfRangeException)
				{				
					currentPos = pos;
					return i;
				}
			}
		}

		/// <summary>
		///  Performs the same action as NextToken.
		/// </summary>
		public object Current => NextToken();
		
		/// <summary>
		///  Performs the same action as HasMoreTokens.
		/// </summary>
		/// <returns>True or false, depending if there are more tokens</returns>
		public bool MoveNext()
		{
			return HasMoreTokens();
		}
		
		/// <summary>
		/// Does nothing.
		/// </summary>
		public void  Reset()
		{
			/* noop */
		}			
	}
	/*******************************/
	/// <summary>
	/// This class provides auxiliar functionality to read and unread characters from a string into a buffer.
	/// </summary>
	private class BackStringReader : System.IO.StringReader
	{
		private char[] buffer;
		private int position = 1;

		/// <summary>
		/// Constructor. Calls the base constructor.
		/// </summary>
		/// <param name="s">The buffer from which chars will be read.</param>
		public BackStringReader(string s) : base (s)
		{
			buffer = new char[position];
		}


		/// <summary>
		/// Reads a character.
		/// </summary>
		/// <returns>The character read.</returns>
		public override int Read()
		{
			if (position >= 0 && position < buffer.Length)
				return buffer[position++];
			return base.Read();
		}

		/// <summary>
		/// Reads an amount of characters from the buffer and copies the values to the array passed.
		/// </summary>
		/// <param name="array">Array where the characters will be stored.</param>
		/// <param name="index">The beginning index to read.</param>
		/// <param name="count">The number of characters to read.</param>
		/// <returns>The number of characters read.</returns>
		public override int Read(char[] array, int index, int count)
		{
			var readLimit = buffer.Length - position;

			if (count <= 0)
				return 0;

			if (readLimit > 0)
			{
				if (count < readLimit)
					readLimit = count;
				Array.Copy(buffer, position, array, index, readLimit);
				count -= readLimit;
				index += readLimit;
				position += readLimit;
			}

			if (count > 0)
			{
				count = base.Read(array, index, count);
				if (count == -1)
				{
					if (readLimit == 0)
						return -1;
					return readLimit;
				}
				return readLimit + count;
			}
			return readLimit;
		}

		/// <summary>
		/// Unreads a character.
		/// </summary>
		/// <param name="unReadChar">The character to be unread.</param>
		public void UnRead(int unReadChar)
		{
			position--;
			buffer[position] = (char) unReadChar;
		}

		/// <summary>
		/// Unreads an amount of characters by moving these to the buffer.
		/// </summary>
		/// <param name="array">The character array to be unread.</param>
		/// <param name="index">The beginning index to unread.</param>
		/// <param name="count">The number of characters to unread.</param>
		public void UnRead(char[] array, int index, int count)
		{
			Move(array, index, count);
		}

		/// <summary>
		/// Unreads an amount of characters by moving these to the buffer.
		/// </summary>
		/// <param name="array">The character array to be unread.</param>
		public void UnRead(char[] array)
		{
			Move(array, 0, array.Length - 1);
		}

		/// <summary>
		/// Moves the array of characters to the buffer.
		/// </summary>
		/// <param name="array">Array of characters to move.</param>
		/// <param name="index">Offset of the beginning.</param>
		/// <param name="count">Amount of characters to move.</param>
		private void Move(char[] array, int index, int count)
		{
			for (var arrayPosition = index + count; arrayPosition >= index; arrayPosition--)
				UnRead(array[arrayPosition]);
		}
	}

	/*******************************/

	/// <summary>
	/// The StreamTokenizerSupport class takes an input stream and parses it into "tokens".
	/// The stream tokenizer can recognize identifiers, numbers, quoted strings, and various comment styles. 
	/// </summary>
	public class StreamTokenizerSupport
	{

		/// <summary>
		/// Internal constants and fields
		/// </summary>

		private const string TOKEN	= "Token[";
		private const string NOTHING	= "NOTHING";
		private const string NUMBER	= "number=";
		private const string EOF		= "EOF";
		private const string EOL		= "EOL";
		private const string QUOTED	= "quoted string=";
		private const string LINE	= "], Line ";
		private const string DASH	= "-.";
		private const string DOT		= ".";
		
		private const int TT_NOTHING		= - 4;
		
		private const sbyte ORDINARYCHAR	= 0x00;
		private const sbyte WORDCHAR		= 0x01;
		private const sbyte WHITESPACECHAR	= 0x02;
		private const sbyte COMMENTCHAR		= 0x04;
		private const sbyte QUOTECHAR		= 0x08;
		private const sbyte NUMBERCHAR		= 0x10;
		
		private const int STATE_NEUTRAL		= 0;
		private const int STATE_WORD		= 1;
		private const int STATE_NUMBER1		= 2;
		private const int STATE_NUMBER2		= 3;
		private const int STATE_NUMBER3		= 4;
		private const int STATE_NUMBER4		= 5;
		private const int STATE_STRING		= 6;
		private const int STATE_LINECOMMENT	= 7;
		private const int STATE_DONE_ON_EOL = 8;

		private const int STATE_PROCEED_ON_EOL			= 9;
		private const int STATE_POSSIBLEC_COMMENT		= 10;
		private const int STATE_POSSIBLEC_COMMENT_END	= 11;
		private const int STATE_C_COMMENT				= 12;
		private const int STATE_STRING_ESCAPE_SEQ		= 13;
		private const int STATE_STRING_ESCAPE_SEQ_OCTAL	= 14;
		
		private const int STATE_DONE		= 100;
		
		private sbyte[] attribute			= new sbyte[256];
		private bool eolIsSignificant		= false;
		private bool slashStarComments	= false;
		private bool slashSlashComments	= false;
		private bool lowerCaseMode		= false;
		private bool pushedback			= false;
		private int lineno				= 1;

		private BackReader inReader;
		private BackStringReader inStringReader;
		private BackInputStream inStream;
		private System.Text.StringBuilder buf;


		/// <summary>
		/// Indicates that the end of the stream has been read.
		/// </summary>
		public const int TT_EOF		= - 1;

		/// <summary>
		/// Indicates that the end of the line has been read.
		/// </summary>
		public const int TT_EOL		= '\n';

		/// <summary>
		/// Indicates that a number token has been read.
		/// </summary>
		public const int TT_NUMBER	= - 2;

		/// <summary>
		/// Indicates that a word token has been read.
		/// </summary>
		public const int TT_WORD	= - 3;

		/// <summary>
		/// If the current token is a number, this field contains the value of that number.
		/// </summary>
		public double nval;

		/// <summary>
		/// If the current token is a word token, this field contains a string giving the characters of the word 
		/// token.
		/// </summary>
		public string sval;

		/// <summary>
		/// After a call to the nextToken method, this field contains the type of the token just read.
		/// </summary>
		public int ttype;


		/// <summary>
		/// Internal methods
		/// </summary>

		private int read()
		{
			if (inReader != null)
				return inReader.Read();
			else if (inStream != null)
				return inStream.Read();
			else
				return inStringReader.Read();
		}
		
		private void unread(int ch)
		{
			if (inReader != null) 
				inReader.UnRead(ch);
			else if (inStream != null)
				inStream.UnRead(ch);
			else
				inStringReader.UnRead(ch);
		}

		private void init()
		{
			buf = new System.Text.StringBuilder();
			ttype = TT_NOTHING;
			
			WordChars('A', 'Z');
			WordChars('a', 'z');
			WordChars(160, 255);
			WhitespaceChars(0x00, 0x20);
			CommentChar('/');
			QuoteChar('\'');
			QuoteChar('\"');
			ParseNumbers();
		}

		private void setAttributes(int low, int hi, sbyte attrib)
		{
			var l = Math.Max(0, low);
			var h = Math.Min(255, hi);
			for (var i = l; i <= h; i++)
				attribute[i] = attrib;
		}
		
		private bool isWordChar(int data)
		{
			var ch = (char) data;
			return (data != - 1 && (ch > 255 || attribute[ch] == WORDCHAR || attribute[ch] == NUMBERCHAR));
		}
		
		/// <summary>
		/// Creates a StreamToknizerSupport that parses the given string.
		/// </summary>
		/// <param name="reader">The System.IO.StringReader that contains the String to be parsed.</param>
		public StreamTokenizerSupport(System.IO.StringReader reader)
		{
			var s = "";
			for (var i = reader.Read(); i != -1 ; i = reader.Read())
			{
				s += (char) i;
			}
			reader.Dispose();
            		inStringReader = new BackStringReader(s);
			init();
		}		

		/// <summary>
		/// Creates a StreamTokenizerSupport that parses the given stream.
		/// </summary>
		/// <param name="reader">Reader to be parsed.</param>
		public StreamTokenizerSupport(System.IO.StreamReader reader)
		{
			inReader = new BackReader(new System.IO.StreamReader(reader.BaseStream, reader.CurrentEncoding).BaseStream, 2, reader.CurrentEncoding);
			init();
		}
		
		/// <summary>
		/// Creates a StreamTokenizerSupport that parses the given stream.
		/// </summary>
		/// <param name="stream">Stream to be parsed.</param>
		public StreamTokenizerSupport(System.IO.Stream stream)
		{
			inStream = new BackInputStream(stream, 2);
			init();
		}
		
		/// <summary>
		/// Specified that the character argument starts a single-line comment.
		/// </summary>
		/// <param name="ch">The character.</param>
		public virtual void CommentChar(int ch)
		{
			if (ch >= 0 && ch <= 255)
				attribute[ch] = COMMENTCHAR;
		}
		
		/// <summary>
		/// Determines whether or not ends of line are treated as tokens.
		/// </summary>
		/// <param name="flag">True indicates that end-of-line characters are separate tokens; False indicates 
		/// that end-of-line characters are white space.</param>
		public virtual void  EOLIsSignificant(bool flag)
		{
			eolIsSignificant = flag;
		}

		/// <summary>
		/// Return the current line number.
		/// </summary>
		/// <returns>Current line number</returns>
		public virtual int Lineno()
		{
			return lineno;
		}
		
		/// <summary>
		/// Determines whether or not word token are automatically lowercased.
		/// </summary>
		/// <param name="flag">True indicates that all word tokens should be lowercased.</param>
		public virtual void LowerCaseMode(bool flag)
		{
			lowerCaseMode = flag;
		}
		
		/// <summary>
		/// Parses the next token from the input stream of this tokenizer.
		/// </summary>
		/// <returns>The value of the ttype field.</returns>
		public virtual int NextToken()
		{
			var prevChar = (char) (0);
			var ch = (char) (0);
			var qChar = (char) (0);
			var octalNumber = 0;
			int state;

			if (pushedback)
			{
				pushedback = false;
				return ttype;
			}
			
			ttype = TT_NOTHING;
			state = STATE_NEUTRAL;
			nval = 0.0;
			sval = null;
			buf.Length = 0;

			do 
			{
				var data = read();
				prevChar = ch;
				ch = (char) data;
				
				switch (state)
				{
					case STATE_NEUTRAL:
					{
						if (data == - 1)
						{
							ttype = TT_EOF;
							state = STATE_DONE;
						}
						else if (ch > 255)
						{
							buf.Append(ch);
							ttype = TT_WORD;
							state = STATE_WORD;
						}
						else if (attribute[ch] == COMMENTCHAR)
						{
							state = STATE_LINECOMMENT;
						}
						else if (attribute[ch] == WORDCHAR)
						{
							buf.Append(ch);
							ttype = TT_WORD;
							state = STATE_WORD;
						}
						else if (attribute[ch] == NUMBERCHAR)
						{
							ttype = TT_NUMBER;
							buf.Append(ch);
							if (ch == '-')
								state = STATE_NUMBER1;
							else if (ch == '.')
								state = STATE_NUMBER3;
							else
								state = STATE_NUMBER2;
						}
						else if (attribute[ch] == QUOTECHAR)
						{
							qChar = ch;
							ttype = ch;
							state = STATE_STRING;
						}
						else if ((slashSlashComments || slashStarComments) && ch == '/')
							state = STATE_POSSIBLEC_COMMENT;
						else if (attribute[ch] == ORDINARYCHAR)
						{
							ttype = ch;
							state = STATE_DONE;
						}
						else if (ch == '\n' || ch == '\r')
						{
							lineno++;
							if (eolIsSignificant)
							{
								ttype = TT_EOL;
								if (ch == '\n')
									state = STATE_DONE;
								else if (ch == '\r')
									state = STATE_DONE_ON_EOL;
							}
							else if (ch == '\r')
								state = STATE_PROCEED_ON_EOL;
						}
						break;
					}
					case STATE_WORD: 
					{
						if (isWordChar(data))
							buf.Append(ch);
						else
						{
							if (data != - 1)
								unread(ch);
							sval = buf.ToString();
							state = STATE_DONE;
						}
						break;
					}
					case STATE_NUMBER1: 
					{
						if (data == - 1 || attribute[ch] != NUMBERCHAR || ch == '-')
						{
							if ( attribute[ch] == COMMENTCHAR && char.IsNumber(ch) )
							{
								buf.Append(ch);
								state = STATE_NUMBER2;
							}
							else
							{
								if (data != - 1)
									unread(ch);
								ttype = '-';
								state = STATE_DONE;
							}
						}
						else
						{
							buf.Append(ch);
							state = ch == '.' ? STATE_NUMBER3 : STATE_NUMBER2;
						}
						break;
					}
					case STATE_NUMBER2: 
					{
						if (data == - 1 || attribute[ch] != NUMBERCHAR || ch == '-')
						{
							if (char.IsNumber(ch) && attribute[ch] == WORDCHAR)
							{
								buf.Append(ch);
							}
							else if (ch == '.' && attribute[ch] == WHITESPACECHAR)
							{
								buf.Append(ch);
							}

							else if ( (data != -1) && (attribute[ch] == COMMENTCHAR && char.IsNumber(ch) ))
							{
								buf.Append(ch);
							}
							else
							{
								if (data != - 1)
									unread(ch);
								try
								{
									nval = double.Parse(buf.ToString());
								}
								catch (FormatException) {}
								state = STATE_DONE;
							}
						}
						else
						{
							buf.Append(ch);
							if (ch == '.')
								state = STATE_NUMBER3;
						}
						break;
					}
					case STATE_NUMBER3: 
					{
						if (data == - 1 || attribute[ch] != NUMBERCHAR || ch == '-' || ch == '.')
						{
							if ( attribute[ch] == COMMENTCHAR && char.IsNumber(ch))
							{
								buf.Append(ch);
							}
							else 
							{
								if (data != - 1)
									unread(ch);
								var str = buf.ToString();
								if (str.Equals(DASH))
								{
									unread('.');
									ttype = '-';
								}
								else if (str.Equals(DOT) && !(WORDCHAR != attribute[prevChar]))
									ttype = '.';
								else
								{
									try
									{
										nval = double.Parse(str);
									}
									catch (FormatException){}
								}
								state = STATE_DONE;
							}
						}
						else
						{
							buf.Append(ch);
							state = STATE_NUMBER4;
						}
						break;
					}
					case STATE_NUMBER4: 
					{
						if (data == - 1 || attribute[ch] != NUMBERCHAR || ch == '-' || ch == '.')
						{
							if (data != - 1)
								unread(ch);
							try
							{
								nval = double.Parse(buf.ToString());
							}
							catch (FormatException) {}
							state = STATE_DONE;
						}
						else
							buf.Append(ch);
						break;
					}
					case STATE_LINECOMMENT: 
					{
						if (data == - 1)
						{
							ttype = TT_EOF;
							state = STATE_DONE;
						}
						else if (ch == '\n' || ch == '\r')
						{
							unread(ch);
							state = STATE_NEUTRAL;
						}
						break;
					}
					case STATE_DONE_ON_EOL: 
					{
						if (ch != '\n' && data != - 1)
							unread(ch);
						state = STATE_DONE;
						break;
					}
					case STATE_PROCEED_ON_EOL: 
					{
						if (ch != '\n' && data != - 1)
							unread(ch);
						state = STATE_NEUTRAL;
						break;
					}
					case STATE_STRING: 
					{
						if (data == - 1 || ch == qChar || ch == '\r' || ch == '\n')
						{
							sval = buf.ToString();
							if (ch == '\r' || ch == '\n')
								unread(ch);
							state = STATE_DONE;
						}
						else if (ch == '\\')
							state = STATE_STRING_ESCAPE_SEQ;
						else
							buf.Append(ch);
						break;
					}
					case STATE_STRING_ESCAPE_SEQ: 
					{
						if (data == - 1)
						{
							sval = buf.ToString();
							state = STATE_DONE;
							break;
						}
						
						state = STATE_STRING;
						if (ch == 'a')
							buf.Append(0x7);
						else if (ch == 'b')
							buf.Append('\b');
						else if (ch == 'f')
							buf.Append(0xC);
						else if (ch == 'n')
							buf.Append('\n');
						else if (ch == 'r')
							buf.Append('\r');
						else if (ch == 't')
							buf.Append('\t');
						else if (ch == 'v')
							buf.Append(0xB);
						else if (ch >= '0' && ch <= '7')
						{
							octalNumber = ch - '0';
							state = STATE_STRING_ESCAPE_SEQ_OCTAL;
						}
						else
							buf.Append(ch);
						break;
					}
					case STATE_STRING_ESCAPE_SEQ_OCTAL: 
					{
						if (data == - 1 || ch < '0' || ch > '7')
						{
							buf.Append((char) octalNumber);
							if (data == - 1)
							{
								sval = buf.ToString();
								state = STATE_DONE;
							}
							else
							{
								unread(ch);
								state = STATE_STRING;
							}
						}
						else
						{
							var temp = octalNumber * 8 + (ch - '0');
							if (temp < 256)
								octalNumber = temp;
							else
							{
								buf.Append((char) octalNumber);
								buf.Append(ch);
								state = STATE_STRING;
							}
						}
						break;
					}
					case STATE_POSSIBLEC_COMMENT: 
					{
						if (ch == '*')
							state = STATE_C_COMMENT;
						else if (ch == '/')
							state = STATE_LINECOMMENT;
						else
						{
							if (data != - 1)
								unread(ch);
							ttype = '/';
							state = STATE_DONE;
						}
						break;
					}
					case STATE_C_COMMENT: 
					{
						if (ch == '*')
							state = STATE_POSSIBLEC_COMMENT_END;
						if (ch == '\n')
							lineno++;
						else if (data == - 1)
						{
							ttype = TT_EOF;
							state = STATE_DONE;
						}
						break;
					}
					case STATE_POSSIBLEC_COMMENT_END: 
					{
						if (data == - 1)
						{
							ttype = TT_EOF;
							state = STATE_DONE;
						}
						else if (ch == '/')
							state = STATE_NEUTRAL;
						else if (ch != '*')
							state = STATE_C_COMMENT;
						break;
					}
				}
			}
			while (state != STATE_DONE);
			
			if (ttype == TT_WORD && lowerCaseMode)
			{
				sval = sval?.ToLower();
			}

			return ttype;
		}

		/// <summary>
		/// Specifies that the character argument is "ordinary" in this tokenizer.
		/// </summary>
		/// <param name="ch">The character.</param>
		public virtual void OrdinaryChar(int ch)
		{
			if (ch >= 0 && ch <= 255)
				attribute[ch] = ORDINARYCHAR;
		}
		
		/// <summary>
		/// Specifies that all characters c in the range low less-equal c less-equal high are "ordinary" in this 
		/// tokenizer.
		/// </summary>
		/// <param name="low">Low end of the range.</param>
		/// <param name="hi">High end of the range.</param>
		public virtual void OrdinaryChars(int low, int hi)
		{
			setAttributes(low, hi, ORDINARYCHAR);
		}
		
		/// <summary>
		/// Specifies that numbers should be parsed by this tokenizer.
		/// </summary>
		public virtual void ParseNumbers()
		{
			for (int i = '0'; i <= '9'; i++)
				attribute[i] = NUMBERCHAR;
			attribute['.'] = NUMBERCHAR;
			attribute['-'] = NUMBERCHAR;
		}
		
		/// <summary>
		/// Causes the next call to the nextToken method of this tokenizer to return the current value in the 
		/// ttype field, and not to modify the value in the nval or sval field.
		/// </summary>
		public virtual void PushBack()
		{
			if (ttype != TT_NOTHING)
				pushedback = true;
		}

		/// <summary>
		/// Specifies that matching pairs of this character delimit string constants in this tokenizer.
		/// </summary>
		/// <param name="ch">The character.</param>
		public virtual void QuoteChar(int ch)
		{
			if (ch >= 0 && ch <= 255)
				attribute[ch] = QUOTECHAR;
		}
		
		/// <summary>
		/// Resets this tokenizer's syntax table so that all characters are "ordinary." See the ordinaryChar 
		/// method for more information on a character being ordinary.
		/// </summary>
		public virtual void ResetSyntax()
		{
			OrdinaryChars(0x00, 0xff);
		}
		
		/// <summary>
		/// Determines whether or not the tokenizer recognizes C++-style comments.
		/// </summary>
		/// <param name="flag">True indicates to recognize and ignore C++-style comments.</param>
		public virtual void SlashSlashComments(bool flag)
		{
			slashSlashComments = flag;
		}
		
		/// <summary>
		/// Determines whether or not the tokenizer recognizes C-style comments.
		/// </summary>
		/// <param name="flag">True indicates to recognize and ignore C-style comments.</param>
		public virtual void SlashStarComments(bool flag)
		{
			slashStarComments = flag;
		}
		
		/// <summary>
		/// Returns the string representation of the current stream token.
		/// </summary>
		/// <returns>A String representation of the current stream token.</returns>
		public override string ToString()
		{
			var buffer = new System.Text.StringBuilder(TOKEN);
			
			switch (ttype)
			{
				case TT_NOTHING:
				{
					buffer.Append(NOTHING);
					break;
				}
				case TT_WORD: 
				{
					buffer.Append(sval);
					break;
				}
				case TT_NUMBER: 
				{
					buffer.Append(NUMBER);
					buffer.Append(nval);
					break;
				}
				case TT_EOF: 
				{
					buffer.Append(EOF);
					break;
				}
				case TT_EOL: 
				{
					buffer.Append(EOL);
					break;
				}
			}

			if (ttype > 0)
			{
				if (attribute[ttype] == QUOTECHAR)
				{
					buffer.Append(QUOTED);
					buffer.Append(sval);
				}
				else
				{
					buffer.Append('\'');
					buffer.Append((char) ttype);
					buffer.Append('\'');
				}
			}

			buffer.Append(LINE);
			buffer.Append(lineno);
			return buffer.ToString();
		}

		/// <summary>
		/// Specifies that all characters c in the range low less-equal c less-equal high are white space 
		/// characters.
		/// </summary>
		/// <param name="low">The low end of the range.</param>
		/// <param name="hi">The high end of the range.</param>
		public virtual void WhitespaceChars(int low, int hi)
		{
			setAttributes(low, hi, WHITESPACECHAR);
		}
		
		/// <summary>
		/// Specifies that all characters c in the range low less-equal c less-equal high are word constituents.
		/// </summary>
		/// <param name="low">The low end of the range.</param>
		/// <param name="hi">The high end of the range.</param>
		public virtual void WordChars(int low, int hi)
		{
			setAttributes(low, hi, WORDCHAR);
		}
	}


	/*******************************/
	/// <summary>
	/// This class provides functionality to reads and unread characters into a buffer.
	/// </summary>
	public class BackReader : System.IO.StreamReader
	{
		private char[] buffer;
		private int position = 1;
		//private int markedPosition;

		/// <summary>
		/// Constructor. Calls the base constructor.
		/// </summary>
		/// <param name="streamReader">The buffer from which chars will be read.</param>
		/// <param name="size">The size of the Back buffer.</param>
		/// <param name="encoding">Character encoding of the buffer</param>
		public BackReader(System.IO.Stream streamReader, int size, System.Text.Encoding encoding) : base(streamReader,encoding)
		{
			buffer = new char[size];
			position = size;
		}

		/// <summary>
		/// Constructor. Calls the base constructor.
		/// </summary>
		/// <param name="streamReader">The buffer from which chars will be read.</param>
		/// <param name="encoding">character encoding for the buffer</param>
		public BackReader(System.IO.Stream streamReader, System.Text.Encoding encoding) : base(streamReader, encoding)
		{
			buffer = new char[position];
		}

		/// <summary>
		/// Checks if this stream support mark and reset methods.
		/// </summary>
		/// <remarks>
		/// This method isn't supported.
		/// </remarks>
		/// <returns>Always false.</returns>
		public bool MarkSupported()
		{
			return false;
		}

		/// <summary>
		/// Marks the element at the corresponding position.
		/// </summary>
		/// <remarks>
		/// This method isn't supported.
		/// </remarks>
		public void Mark(int pos)
		{
			throw new System.IO.IOException("Mark operations are not allowed");			
		}

		/// <summary>
		/// Resets the current stream.
		/// </summary>
		/// <remarks>
		/// This method isn't supported.
		/// </remarks>
		public void Reset()
		{
			throw new System.IO.IOException("Mark operations are not allowed");
		}

		/// <summary>
		/// Reads a character.
		/// </summary>
		/// <returns>The character read.</returns>
		public override int Read()
		{
			if (position >= 0 && position < buffer.Length)
				return buffer[position++];
			return base.Read();
		}

		/// <summary>
		/// Reads an amount of characters from the buffer and copies the values to the array passed.
		/// </summary>
		/// <param name="array">Array where the characters will be stored.</param>
		/// <param name="index">The beginning index to read.</param>
		/// <param name="count">The number of characters to read.</param>
		/// <returns>The number of characters read.</returns>
		public override int Read(char[] array, int index, int count)
		{
			var readLimit = buffer.Length - position;

			if (count <= 0)
				return 0;

			if (readLimit > 0)
			{
				if (count < readLimit)
					readLimit = count;
				Array.Copy(buffer, position, array, index, readLimit);
				count -= readLimit;
				index += readLimit;
				position += readLimit;
			}

			if (count > 0)
			{
				count = base.Read(array, index, count);
				if (count == -1)
				{
					if (readLimit == 0)
						return -1;
					return readLimit;
				}
				return readLimit + count;
			}
			return readLimit;
		}

		/// <summary>
		/// Checks if this buffer is ready to be read.
		/// </summary>
		/// <returns>True if the position is less than the length, otherwise false.</returns>
		public bool IsReady()
		{
			return (position >= buffer.Length || BaseStream.Position >= BaseStream.Length);
		}

		/// <summary>
		/// Unreads a character.
		/// </summary>
		/// <param name="unReadChar">The character to be unread.</param>
		public void UnRead(int unReadChar)
		{
			position--;
			buffer[position] = (char) unReadChar;
		}

		/// <summary>
		/// Unreads an amount of characters by moving these to the buffer.
		/// </summary>
		/// <param name="array">The character array to be unread.</param>
		/// <param name="index">The beginning index to unread.</param>
		/// <param name="count">The number of characters to unread.</param>
		public void UnRead(char[] array, int index, int count)
		{
			Move(array, index, count);
		}

		/// <summary>
		/// Unreads an amount of characters by moving these to the buffer.
		/// </summary>
		/// <param name="array">The character array to be unread.</param>
		public void UnRead(char[] array)
		{
			Move(array, 0, array.Length - 1);
		}

		/// <summary>
		/// Moves the array of characters to the buffer.
		/// </summary>
		/// <param name="array">Array of characters to move.</param>
		/// <param name="index">Offset of the beginning.</param>
		/// <param name="count">Amount of characters to move.</param>
		private void Move(char[] array, int index, int count)
		{
			for (var arrayPosition = index + count; arrayPosition >= index; arrayPosition--)
				UnRead(array[arrayPosition]);
		}
	}


	/*******************************/
	/// <summary>
	/// Provides functionality to read and unread from a Stream.
	/// </summary>
	public class BackInputStream : System.IO.BinaryReader
	{
		private byte[] buffer;
		private int position = 1;

		/// <summary>
		/// Creates a BackInputStream with the specified stream and size for the buffer.
		/// </summary>
		/// <param name="streamReader">The stream to use.</param>
		/// <param name="size">The specific size of the buffer.</param>
		public BackInputStream(System.IO.Stream streamReader, int size) : base(streamReader)
		{
			buffer = new byte[size];
			position = size;
		}

		/// <summary>
		/// Creates a BackInputStream with the specified stream.
		/// </summary>
		/// <param name="streamReader">The stream to use.</param>
		public BackInputStream(System.IO.Stream streamReader) : base(streamReader)
		{
			buffer = new byte[position];
		}

		/// <summary>
		/// Checks if this stream support mark and reset methods.
		/// </summary>
		/// <returns>Always false, these methods aren't supported.</returns>
		public bool MarkSupported()
		{	
			return false;
		}

		/// <summary>
		/// Reads the next bytes in the stream.
		/// </summary>
		/// <returns>The next byte readed</returns>
		public override int Read()
		{
			if (position >= 0 && position < buffer.Length)
				return buffer[position++];
			return base.Read();
		}

		/// <summary>
		/// Reads the amount of bytes specified from the stream.
		/// </summary>
		/// <param name="array">The buffer to read data into.</param>
		/// <param name="index">The beginning point to read.</param>
		/// <param name="count">The number of characters to read.</param>
		/// <returns>The number of characters read into buffer.</returns>
		public virtual int Read(sbyte[] array, int index, int count)
		{
			var byteCount = 0;
			var readLimit = count + index;
			var aux = ToByteArray(array);

			for (byteCount = 0; position < buffer.Length && index < readLimit; byteCount++)
				aux[index++] = buffer[position++];

			if (index < readLimit)
				byteCount += base.Read(aux, index, readLimit - index);

			for(var i = 0; i < aux.Length;i++)
				array[i] = (sbyte)aux[i];

			return byteCount;
		}

		/// <summary>
		/// Unreads a byte from the stream.
		/// </summary>
		/// <param name="element">The value to be unread.</param>
		public void UnRead(int element)
		{
			position--;
			if (position >= 0)
				buffer[position] = (byte) element;
		}

		/// <summary>
		/// Unreads an amount of bytes from the stream.
		/// </summary>
		/// <param name="array">The byte array to be unread.</param>
		/// <param name="index">The beginning index to unread.</param>
		/// <param name="count">The number of bytes to be unread.</param>
		public void UnRead(byte[] array, int index, int count)
		{
			Move(array, index, count);
		}

		/// <summary>
		/// Unreads an array of bytes from the stream.
		/// </summary>
		/// <param name="array">The byte array to be unread.</param>
		public void UnRead(byte[] array)
		{
			Move(array, 0, array.Length - 1);
		}

		/// <summary>
		/// Skips the specified number of bytes from the underlying stream.
		/// </summary>
		/// <param name="numberOfBytes">The number of bytes to be skipped.</param>
		/// <returns>The number of bytes actually skipped</returns>
		public long Skip(long numberOfBytes)
		{
			return BaseStream.Seek(numberOfBytes, System.IO.SeekOrigin.Current) - BaseStream.Position;
		}

		/// <summary>
		/// Moves data from the array to the buffer field.
		/// </summary>
		/// <param name="array">The array of bytes to be unread.</param>
		/// <param name="index">The beginning index to unread.</param>
		/// <param name="count">The amount of bytes to be unread.</param>
		private void Move(byte[] array, int index, int count)
		{
			for (var arrayPosition = index + count;  arrayPosition >= index; arrayPosition--)
				UnRead(array[arrayPosition]);
		}
	}

	/*******************************/
	/// <summary>
	/// SupportClass for the Stack class.
	/// </summary>
	public class StackSupport
	{
		/// <summary>
		/// Removes the element at the top of the stack and returns it.
		/// </summary>
		/// <param name="stack">The stack where the element at the top will be returned and removed.</param>
		/// <returns>The element at the top of the stack.</returns>
		public static T Pop<T>(System.Collections.Generic.List<T> stack)
		{
			var obj = stack[stack.Count - 1];
			stack.RemoveAt(stack.Count - 1);

			return obj;
		}
	}


}
