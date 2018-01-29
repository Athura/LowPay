using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Security.Cryptography;
using System.Text;

namespace RTS.Service.Common
{
	#region Exceptions
	/// <summary>
	/// Represents a general-purpose exception when encoding or decoding a SecurityToken
	/// </summary>
	public class SecurityTokenException : Exception { public SecurityTokenException(string msg) : base(msg) { } }
	/// <summary>
	/// Represents an attempt to decode a SecurityToken that is invalid or tampered.
	/// </summary>
	public class SecurityTokenExpiredException : SecurityTokenException { public SecurityTokenExpiredException(string msg) : base(msg) { } }
	/// <summary>
	/// Represents an attempt to decode a SecurityToken that is expired.
	/// </summary>
	public class SecurityTokenInvalidException : SecurityTokenException { public SecurityTokenInvalidException(string msg) : base(msg) { } }
	#endregion

	/// <summary>
	/// This class provides a method of encoding and decoding security tokens,
	/// which provide storage for a small set of string key/value pairs, with
	/// shared-secret authentication (users cannot alter information in the token,
	/// can be signed/validated by any party having the key), and encryption (users
	/// cannot read the content of the token), optional expiration (tokens not
	/// accepted after expiration date), and automatic compression.  The SecurityToken
	/// does NOT provide replay protection, so a valid token can be used repeatedly
	/// during the window before it expires.  As authentication and encryption are
	/// shared-secret only, any party that has a key that allows it to decrypt/verify
	/// a token can also encrypt/create one, and if more than two parties have the
	/// key, it is not possible to cryptographically prove which other party generated
	/// a token.  Set the Key and optional Expires, store string key/value pairs in
	/// Data, then use ToString to encode the token.  Convert the string back to a
	/// token on the receiving end with FromString, which also requires the original
	/// key (cannot be not included in the token).
	/// </summary>
	public class SecurityToken
	{
		#region SecurityTokenFlags
		/// <summary>
		/// Used internally to represent certain encoding
		/// variables within the packed security token.
		/// </summary>
		[Flags]
		protected enum SecurityTokenFlags
		{
			None = 0,
			Deflated = 1,
			ExpireDate = 2,
			EncryptedCBC = 4,
			EncryptedCTS = 8
		}
		#endregion

		#region HMAC_SIZE
		/// <summary>
		/// Size of the included HMAC, in bytes.  Since this class is hard-coded to use
		/// a SHA1 hash, this is hard-coded to 160 bits (20 bytes).
		/// </summary>
		public const int HMAC_SIZE = 20;
		#endregion

		#region Epoch
		/// <summary>
		/// The Unix Epoch, on which all expiration dates are based when encoded in
		/// a token as a uint32 number of seconds after this date.
		/// </summary>
		public static DateTime Epoch { get { return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc); } }
		#endregion

		#region Data
		/// <summary>
		/// Payload data to be secured by this token.  This data will not be strongly
		/// protected against readability by a 3rd party (no privacy) but will be strongly
		/// protected against unauthorized alteration (authentication).  Any number of
		/// key/value pairs may be included, though each key and each value must be no more
		/// than UInt16.MaxValue (65535) bytes long in UTF-8 encoding.
		/// </summary>
		public Dictionary<string, string> Data { get; protected set; }
		#endregion

		#region Expires
		/// <summary>
		/// Optional expiration date (UTC) of this token.  Default is DateTime.MaxValue, meaning that
		/// the token never expires.  DateTime.MaxValue also means that no expiration is encoded
		/// into the token, saving a little space.
		/// </summary>
		/// <example>Token.Expires = DateTime.UtcNow.AddMinutes(2);</example>
		public DateTime Expires { get; set; }
		#endregion

		#region Key
		/// <summary>
		/// A shared secret used to digitally sign this token.  All parties that need to be able
		/// to either sign or validate tokens must know this key, and any party that knows this
		/// key will be able to both sign and validate tokens.  A token must be signed and validated
		/// with the exact same key.  Note that this is now the same value from which the encryption
		/// key is derived, so parties that cannot validate/sign a token cannot create/read one.
		/// </summary>
		public string Key { get; set; }
		#endregion

		#region Constructor
		/// <summary>
		/// </summary>
		public SecurityToken()
		{
			Expires = DateTime.MaxValue;
			Data = new Dictionary<string, string>();
		}
		#endregion

		#region CreateCipher

		private static MemoryCache _cipherCache = new MemoryCache("SecurityTokenCiphers");

		/// <summary>
		/// Creates the symmetric cipher used for built-in encryption.  Currently, this is
		/// an AES-256 cipher, keyed with the SHA-256 digest of the provided key.
		/// </summary>
		private static SymmetricAlgorithm CreateCipher(string key)
		{
			string staticKey = string.Format("{0}||||{1}", System.Threading.Thread.CurrentThread.ManagedThreadId, key);

			SymmetricAlgorithm Cipher = null;
			lock ( _cipherCache )
			{
				if ( !_cipherCache.Contains(staticKey) )
				{
					try { Cipher = new AesCryptoServiceProvider(); }
					catch { Cipher = new AesManaged(); }

					try { Cipher.Key = new SHA256CryptoServiceProvider().ComputeHash(Encoding.UTF8.GetBytes(key)); }
					catch { Cipher.Key = new SHA256Managed().ComputeHash(Encoding.UTF8.GetBytes(key)); }

					_cipherCache.Add(staticKey, Cipher, DateTimeOffset.UtcNow.AddMinutes(60));
				}

				return (SymmetricAlgorithm)_cipherCache.Get(staticKey);
			}
			//         SymmetricAlgorithm Cipher = null;

			//try { Cipher = new AesCryptoServiceProvider(); }
			//catch { Cipher = new AesManaged(); }

			//try { Cipher.Key = new SHA256CryptoServiceProvider().ComputeHash(Encoding.UTF8.GetBytes(key)); }
			//catch { Cipher.Key = new SHA256Managed().ComputeHash(Encoding.UTF8.GetBytes(key)); }

			//return Cipher;
		}
		#endregion

		#region ToString
		/// <summary>
		/// Converts the token to its string representation, including packing, compressing, encrypting,
		/// and digitally signing the token's contents and encoding to a web-safe Base64 variant.
		/// </summary>
		public override string ToString()
		{
			SecurityTokenFlags Flags = SecurityTokenFlags.None;

			// Pack the key/value pairs in the dictionary into a single byte array, each
			// value being encoded in UTF-8 (full Unicode support, with efficient storage
			// for US-ASCII data).  Note that each string is limited to 65535 chars, to
			// save the extra 4 bytes per pair for encoding the extra zeros for a 32-bit
			// value, considering that any value that large is excessive for the intended
			// application.
			List<byte> InnerPayload = new List<byte>();
			foreach ( string S in Data.Keys.OrderBy(K => K).SelectMany(K => new string[] { K, Data[K] }) )
			{
				byte[] StrBytes = Encoding.UTF8.GetBytes(S);
				if ( StrBytes.Length > UInt16.MaxValue )
					throw new SecurityTokenException("Each key/value in a " + typeof(SecurityToken).Name + " cannot "
						+ "exceed " + UInt16.MaxValue.ToString() + " bytes in UTF-8 representation.");
				InnerPayload.AddRange(BitConverter.GetBytes((UInt16)StrBytes.Length).EndianFlip());
				InnerPayload.AddRange(StrBytes);
			}

			// If an expiration date is set and the value is valid, set the expiration flag and
			// encode an expiration date at the end of the payload.  Expiration date is encoded
			// as a 32-bit unsigned Unix date with second precision, for simplicity.
			if ( (Expires < DateTime.MaxValue) && (Expires >= Epoch) )
			{
				Flags |= SecurityTokenFlags.ExpireDate;
				InnerPayload.AddRange(BitConverter.GetBytes((UInt32)(Expires - Epoch).TotalSeconds).EndianFlip());
			}

			// Automatically attempt to compress the payload.  If the payload successfully compresses to a
			// smaller size than the original, set the compression flag and use the compressed payload.
			// This may make the encoded token at least look a little smaller, though compression ratios
			// will be unimpressive on data this small.  Note that compression is done before adding the
			// validation HMAC, as (1) HMAC data doesn't compress well, and (2) we want to be able to validate
			// the compressed payload before attempting to engage the decompressor on the other side, as piping
			// untrusted data into a decompressor may be a security or DoS risk.
			List<byte> PackedPayload = InnerPayload.DeflateCompress(Ionic.Zlib.CompressionLevel.BestCompression).ToList();
			if ( PackedPayload.Count < InnerPayload.Count )
			{
				InnerPayload = PackedPayload;
				Flags |= SecurityTokenFlags.Deflated;
			}

			// Encrypt the inner payload with the same key as used for signing.  Try to use CTS, if the data
			// is long enough, as it doesn't add any overhead, but fall back on CBC w/ PKCS7 padding for very
			// short messages.
			SymmetricAlgorithm Alg = CreateCipher(Key);
			if ( InnerPayload.Count >= (Alg.BlockSize / 8) )
			{
				InnerPayload = Alg.CTSEncrypt(InnerPayload.ToArray()).ToList();
				Flags |= SecurityTokenFlags.EncryptedCTS;
			}
			else
			{
				InnerPayload = Alg.CBCEncrypt(InnerPayload.ToArray()).ToList();
				Flags |= SecurityTokenFlags.EncryptedCBC;
			}

			// Reverse the encrypted content and re-encrypt it, to cause any plaintext change to propagate
			// to every bit of output.  This should theoretically provide at least a little hardening against
			// a chosen-plaintext attack.  We can always use CTS mode here, as the content at this point is
			// guaranteed to be long enough for CTS mode (a previous CBC encryption would have expanded it).
			InnerPayload.Reverse();
			InnerPayload = Alg.CTSEncrypt(InnerPayload.ToArray()).ToList();

			// Concatenate the message flags, content, and validation HMAC and return the result encoded
			// in a format appropriate for use in a URL QueryString.
			List<byte> Final = new List<byte>();
			Final.Add((byte)Flags);
			Final.AddRange(InnerPayload);
			Final.AddRange(Final.ToArray().GetHMACSHA1Bytes(Key));
			return Final.ToArray().Base64WebSafeEncode();
		}
		#endregion

		#region FromString
		/// <summary>
		/// Parses a string containing a token and returns the token it contains,
		/// if that token is valid.
		/// </summary>
		/// <param name="key">The shared secret key with which the token was originally signed.</param>
		/// <param name="token">The original token string representation.</param>
		public static SecurityToken FromString(string key, string token)
		{
			// Decode the incoming string and do a basic sanity check.
			byte[] RawData = token.Base64WebSafeDecode();
			if ( RawData.Length < (HMAC_SIZE + 1) )
				throw new SecurityTokenInvalidException("String is too small to be a valid " + typeof(SecurityToken).Name);

			// Recompute the HMAC using the provided key (as the key is a secret and
			// cannot be encoded into the token itself), and make sure it matches.  If
			// not, then the token is corrupt and cannot be trusted.
			byte[] RawHMAC = RawData.Skip(RawData.Length - HMAC_SIZE).ToArray();
			RawData = RawData.Take(RawData.Length - HMAC_SIZE).ToArray();
			if ( RawData.GetHMACSHA1Bytes(key).Base64Encode() != RawHMAC.Base64Encode() )
				throw new SecurityTokenInvalidException("HMAC validation failed.");

			// Extract the flags from the payload.
			SecurityTokenFlags Flags = (SecurityTokenFlags)RawData.First();
			RawData = RawData.Skip(1).ToArray();

			// If data was encrypted, decrypt it.
			SymmetricAlgorithm Alg = CreateCipher(key);
			if ( (Flags & SecurityTokenFlags.EncryptedCTS) != 0 )
				RawData = Alg.CTSDecrypt(Alg.CTSDecrypt(RawData).Reverse().ToArray());
			else if ( (Flags & SecurityTokenFlags.EncryptedCBC) != 0 )
				RawData = Alg.CBCDecrypt(Alg.CTSDecrypt(RawData).Reverse().ToArray());

			// If the data was originally deflated, decompress it.
			if ( (Flags & SecurityTokenFlags.Deflated) != 0 )
				RawData = RawData.DeflateDecompress().ToArray();

			// If the data contains an expiration date, then decode the expiration date
			// and make sure the token has not expired.
			DateTime Exp = DateTime.MaxValue;
			if ( (Flags & SecurityTokenFlags.ExpireDate) != 0 )
			{
				Exp = Epoch.AddSeconds(BitConverter.ToUInt32(RawData.Skip(RawData.Length - sizeof(UInt32)).EndianFlip().ToArray(), 0));
				if ( Exp < DateTime.UtcNow )
					throw new SecurityTokenExpiredException("Token has expired.");
				RawData = RawData.Take(RawData.Length - sizeof(UInt32)).ToArray();
			}

			// The remaining data is the key/value pair data, packed as an even number of
			// strings, each prefixed with a big-endian uint16 length specifier, followed
			// by that many bytes of UTF-8-encoded string data.  After unpacking strings,
			// rebuild the original dictionary.
			Queue<string> Values = new Queue<string>();
			while ( RawData.Length > 0 )
			{
				ushort StrLen = BitConverter.ToUInt16(RawData.Take(2).EndianFlip().ToArray(), 0);
				Values.Enqueue(Encoding.UTF8.GetString(RawData.Skip(2).Take(StrLen).ToArray()));
				RawData = RawData.Skip(StrLen + 2).ToArray();
			}
			Dictionary<string, string> NewData = new Dictionary<string, string>();
			while ( Values.Count > 0 )
				NewData[Values.Dequeue()] = Values.Dequeue();

			// Return a security token containing the original expiration, key, and
			// payload data.  Note that if any of the checks above fails (payload validation,
			// matching key, expiration) then an exception will be thrown instead of
			// returning any token.
			return new SecurityToken
			{
				Expires = Exp,
				Key = key,
				Data = NewData
			};
		}
		#endregion

		#region Equals
		/// <summary>
		/// Determines if two tokens are equal by comparing their string equivalents.
		/// </summary>
		public override bool Equals(object obj)
		{
			return this.ToString().Equals(obj.ToString());
		}
		#endregion

		#region GetHashCode
		/// <summary>
		/// Compute a hash code from a token by its string equivalent.
		/// </summary>
		public override int GetHashCode()
		{
			return this.ToString().GetHashCode();
		}
		#endregion
	}
}
