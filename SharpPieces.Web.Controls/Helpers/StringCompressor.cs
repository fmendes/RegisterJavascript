using System;

namespace CamdenSoftware.Utilities
{
	/// <summary>
	/// Class for compressing/decompressing strings.
	/// </summary>
	///
	/// <remarks>
	/// <para>
	/// The StringCompressor class implements a form of text compression that I like to call HACC,
	/// for Half ASCII-Compatible Compression (say it fast).  Only the low-order 8 bits of a character
	/// are used for compression op-codes and length, so it is only half as good as it could be in
	/// UNICODE environments (like .NET).  The reason for this half-asciid approach is for
	/// compatibility with 8-bit character environments where a mechanism is already in place to convert
	/// strings between ASCII and UNICODE.  One such environment is the SynergyDE xfNetLink.NET platform,
	/// which interchanges .NET String types and DIBOL alphanumerics.  This compression algorithm was
	/// developed specifically for that platform.  The compressed data can represent embedded nulls, but
	/// it never contains a null character.  This also helps when transferring data between environments,
	/// but reduces the compression efficiency ever so slightly to allow for the representation of
	/// "duplicate 1 null".  Thus, lengths must be stored in a representation that allows for a minimum
	/// value of 1, whereas otherwise we could have imposed a minumum compression length of, say, 3, and
	/// thus have raised the ceiling on our length by 2 as well.
	/// </para>
	/// <para>
	/// The compressed data takes the form of a series of (op-code character possibly followed by data).
	/// The op-code itself is a number between 1-7, which is stored in the low-order 3 bits (0x07) of the
	/// character.  Most of the op-codes require a length.  If the length is in the range 1-16, then it is
	/// stored as 0-15 (reduced by 1) in bits 4-7 (0xF0) of the op-code byte.  If the length is in the
	/// range 17-4096, then it is reduced by 1 to a 12-bit number (16-4095) and the low-order 4 bits of
	/// that number are stored in bits 4-7.  Bit 3 (0x08) is set on in that case, and the remaining 8 bits
	/// of the length (0x0FF0) are shifted four bits (to 0xFF) and stored in the next character.  Any
	/// remaining data required for the operation is stored in the characters that follow.  Of course,
	/// if we could use all 16 bits of a UNICODE character, we could make this compression even better,
	/// but as noted above that wouldn't transfer well into ASCII environments.  The op-codes include
	/// "duplicate a character" (with specific op-codes for the more common spaces, zeroes, and nulls) and
	/// "copy a string", each taking a length.  Special op-codes mark the beginning and ending of
	/// compressed data, so that we can be sure that this is our data when decompressing.
	/// </para>
	/// <para>
	/// Implementing this class in C# yielded some interesting information regarding String performance.
	/// You may notice that I used the StringBuilder class when compressing and decompressing, and
	/// that I always try to set the Capacity for the StringBuilder object to a value that pretty well
	/// insures that it will not have to be expanded.  It turns out that either concatenating to a
	/// String object or expanding the Capacity of a StringBuilder object multiplies the processing
	/// time by a factor of about 10.  Furthermore, it is slightly more efficient to copy characters 
	/// one at a time into the StringBuilder object than to create a new String object (or a SubString)
	/// to append (roughly 1.125:1).
	/// </para>
	/// </remarks>
	public class StringCompressor
	{
		public const char Version = (char)0;		// Note: can only be in the range 0-15

		/// <summary>
		///  Exception class for an unrecognized compressed string
		/// </summary>
		public class CompressionException : System.ApplicationException
		{
			public ReasonCode Reason = ReasonCode.BadSource;	// Right now, there's only one reason

			public enum ReasonCode								// As stated by this enumeration
			{
				BadSource = -1				// Unrecognized compression opcode character
			}

			public CompressionException() : this("Unrecognized compressed data")
			{
			}

			public CompressionException(string text) : this(text, CompressionException.ReasonCode.BadSource)
			{
			}

			public CompressionException(string text, CompressionException.ReasonCode reason) : base(text)
			{
				Reason = reason;
			}
		}

		/// <summary>
		/// Class that encapsulates op-codes for the compression mechanism.
		/// </summary>
		protected class OpCode
		{
			// Constants for the op-code portion itself:
			public const char Prolog = (char)0x01;			// Prolog: identifies us and embeds version #
			public const char Duplicate = (char)0x02;		// Duplicate a specific character
			public const char Spaces = (char)0x03;			// Duplicate spaces (0x20)
			public const char Zeroes = (char)0x04;			// Duplicate zeroes (0x30)
			public const char Nulls = (char)0x05;			// Duplicate nulls (0x00)
			public const char Copy = (char)0x06;			// Copy a string
			public const char End = (char)0x07;				// End of compressed data

			public const char OpCodeMask = (char)0x07;		// Mask for opcodes

			public const char ExtendedLength = (char)0x08;	// Bit 0x08 indicates extended length
			// Extended length uses 12 bits (the high-order 4 bits of the opcode byte (low-order byte
			// of the opcode character) plus the low-order 8 bits of the next character) to represent
			// the length of the operation minus 1.  Otherwise, only the high-order 4 bits of the
			// opcode byte are used for the length minus 1.  Thus, operations for lengths of 1-16
			// characters do not use extended length, and operations of 17-4096 characters use extended
			// length.  If more than 4096 characters are required, the operation must be broken up into
			// multiple operations of shorter lengths.

			// Mask and shift to set the data portion of the opcode character (bits 0xF0).
			public static char SetData(char data)
			{
				return (char)(((data & 0x0F) << 4) & 0xF0);
			}

			// Mask and shift to get the data portion of the opcode character.
			public static char GetData(char opcode)
			{
				return (char)(((opcode & 0xF0) >> 4) & 0x0F);
			}
		}

		protected string m_compressed;			// Store the string in compressed form.
		protected int m_uncompressed_length;	// Length of uncompressed version.

		public StringCompressor() : this("")	// Default constructor sets the empty string
		{
		}

		// Constructor that supplies a string to compress.
		public StringCompressor(string strUncompressed)
		{
			m_compressed = StringCompressor.Compress(strUncompressed);	// Compress it
			m_uncompressed_length = strUncompressed.Length;				// Save the uncompressed length
		}

		// Static method for compressing a string.
		public static string Compress(string strUncompressed)
		{
			// Start with the prolog character, embedding the version number.  Use the uncompressed
			// length for the initial allocation, for speed.
			System.Text.StringBuilder compressed = new System.Text.StringBuilder("", strUncompressed.Length);
			compressed.Append((char)(OpCode.Prolog | OpCode.SetData(Version)));

			char cDup = '\0',				// Character being duplicated.
				cOp;						// Opcode character.
			bool bDup = false;				// Are we duplicating a character?
			int ndx,						// Index into uncompressed string
				first = 0;					// Index of first character of present operation

			if ((strUncompressed.Length > 0) && (strUncompressed[0] == '\0'))
			{
				bDup = true;				// Always duplicate nulls, even if only 1.
			}

			// Loop through the uncompressed string...
			for (ndx = 1; ndx <= strUncompressed.Length; ndx++)
			{
				if (bDup)					// Are we duplicating?
				{
					if ((ndx == strUncompressed.Length) || (strUncompressed[ndx] != cDup))	// Did we reach the end, or find a different character?
					{									// Yes, store the duplication operation
						switch (cDup)					// What character are we duplicating?
						{
							case ' ':
								cOp = OpCode.Spaces;	// ASCII space (0x20)
								break;

							case '0':
								cOp = OpCode.Zeroes;	// ASCII zero (0x30)
								break;

							case '\0':
								cOp = OpCode.Nulls;		// ASCII null (0x00)
								break;

							default:
								cOp = OpCode.Duplicate;	// Some other character
								break;
						}
						int dlen = ndx - first;			// Compute the length
						while (dlen > 0)				// Loop for segments larger than 4096 (0x1000)
						{
							int clen = System.Math.Min(dlen, 0x1000);	// Up to 4096 per operation
							StoreOperator(compressed, cOp, clen);		// Store the operation and length
							if (cOp == OpCode.Duplicate)				// If "some other character"
								compressed.Append(cDup);				// Store the character
							dlen -= clen;								// Reduce remaining length
						}
						first = ndx;					// Mark the beginning of the netx operation
						if ((ndx < strUncompressed.Length) && (strUncompressed[ndx] == '\0'))	// If a null, always duplicate
						{
							bDup = true;					// Even if only 1.
							cDup = '\0';
						}
						else								// Otherwise, assume "copy" until duplicates
							bDup = false;					//   are encountered.
					}
				}
				else									// We are copying a string of non-replicated characters
				{
					if ((ndx == strUncompressed.Length) || (strUncompressed[ndx] == strUncompressed[ndx-1]))	// Did we reach the end or find a duplicate?
					{
						int dlen = ndx - first;			// Yes, compute the length of the non-replicating string
						if (ndx < strUncompressed.Length)	// If we found a duplicate
							dlen--;							// don't include the duplicating character
						while (dlen > 0)				// Again, loop for segments larger than 4096
						{
							int clen = System.Math.Min(dlen, 0x1000);	// Up to 4096 per operation
							dlen -= clen;									// Reduce remaining length
							StoreOperator(compressed, OpCode.Copy, clen);	// Store the opcode and length
							while (clen-- > 0)								// Append the characters themselves
								compressed.Append(strUncompressed[first++]);
						}
						if (ndx < strUncompressed.Length)	// Do we have more?
						{
							bDup = true;					// We are now in duplication mode
							cDup = strUncompressed[ndx];	// Save the replicating character
							first = ndx-1;					// Save the index of the first such character
						}
					}
				}
			}
			compressed.Append(OpCode.End);				// Terminate the compressed data
			return compressed.ToString();				// Return the compressed string
		}

		// Static method for decompressing a string.
		public static string Decompress(string strCompressed)
		{
			return Decompress(strCompressed, -1);		// The uncompressed length is unknown.
		}

		// Static method for decompressing a string, supplying the uncompressed length.
		public static string Decompress(string strCompressed, int uncompressed_length)
		{
			// Use StringBuilder rather than String for a phenomenal performance difference.
			System.Text.StringBuilder uncompressed;
			if (uncompressed_length > 0)		// Is the uncompressed length known?
				uncompressed = new System.Text.StringBuilder("", uncompressed_length);
			else
				uncompressed = new System.Text.StringBuilder("");

			if (strCompressed[0] != (char)(OpCode.Prolog | OpCode.SetData(Version)))
				throw new CompressionException();		// This is not our compressed data.

			int dlen = 0;								// Length of a data segment.
			// Loop through the compressed string...
			for (int ndx = 1; ndx < strCompressed.Length; ndx++)
			{
				switch (GetOperator(strCompressed, ref ndx, ref dlen))	// Get opcode and length
				{
					case OpCode.Duplicate:				// Duplicate a specific character (in next char).
						char cDup = strCompressed[++ndx];
						while (dlen-- > 0)				// This is faster than creating a string object
							uncompressed.Append(cDup);	//   to append.
						break;

					case OpCode.Spaces:					// Duplicate spaces.
						uncompressed.Append(new string(' ', dlen));
						break;

					case OpCode.Zeroes:					// Duplicate zeroes.
						uncompressed.Append(new string('0', dlen));
						break;

					case OpCode.Nulls:					// Duplicate nulls.
						uncompressed.Append(new string('\0', dlen));
						break;

					case OpCode.Copy:					// Copy the following string.
						while (dlen-- > 0)				// This is faster than appending a SubString
							uncompressed.Append(strCompressed[++ndx]);
						break;

					case OpCode.End:					// End of compressed data.
						return uncompressed.ToString();	// Return the string.

					default:							// Oops -- this isn't our data after all.
						throw new CompressionException();
				}
			}
			throw new CompressionException();		// Exceeded length without OpCode.End; can't be our data.
		}

		// Property for the compressed string.
		public string Compressed
		{
			get { return m_compressed; }
			set { m_compressed = value; }
		}

		// Property for the decompressed string.
		public string Decompressed
		{
			get { return Decompress(m_compressed, m_uncompressed_length); }
			set { m_compressed = Compress(value); m_uncompressed_length = value.Length; }
		}

		// String representation of this.
		public override string ToString()
		{
			return Decompressed;		// We'll use the decompressed form for readability.
		}

		// Protected method for storing the op-code and length
		protected static void StoreOperator(System.Text.StringBuilder strCompressed, char cOpCode, int len)
		{
			len--;						// Subtract one (reduce 1-4096 to 0-4095)
										// Create the opcode and lower 4 bits of the length
			char cOp = (char)(cOpCode | OpCode.SetData((char)(len & 0x0F)));
			if ((len & 0x0FF0) != 0)	// Do we need more than 4 bits?
			{
				strCompressed.Append((char)(cOp | OpCode.ExtendedLength));	// Set the "extended length" bit on the opcode
				strCompressed.Append((char)(((len & 0x0FF0) >> 4) & 0xFF));	// And store the upper 8 bits of the length
			}
			else						// 4 bits was enough
				strCompressed.Append(cOp);	// Just store the opcode and length in one character
		}

		// Get the opcode and length from the compressed data stream.
		protected static char GetOperator(string strCompressed, ref int ndx, ref int dlen)
		{
			char cOp = (char)(strCompressed[ndx] & OpCode.OpCodeMask);	// Extract the opcode itself

			dlen = OpCode.GetData(strCompressed[ndx]);					// Get the lower 4 bits of the length
			if ((strCompressed[ndx] & OpCode.ExtendedLength) != 0)		// Is the "extended length" bit on?
			{
				dlen |= (((strCompressed[++ndx] & 0xFF) << 4) & 0x0FF0);	// Get the upper 8 bits of the length
			}															// from the next char and bump the index
			dlen++;						// Add one (inflate 0-4095 to 1-4096)

			return cOp;					// Return the opcode
		}
	}
}
