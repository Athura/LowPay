using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
//using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using Ionic.Zlib;

namespace RTS.Service.Common
{
	public static class CommonExtensions
	{
		#region GetUTF8Bytes
		/// <summary>
		/// Converts a string to its UTF-8 byte representation.
		/// </summary>
		public static byte[] GetUTF8Bytes(this string stringValue)
		{
			return Encoding.UTF8.GetBytes(stringValue);
		}
		#endregion

		#region FromUTF8Bytes
		/// <summary>
		/// Converts UTF-8 byte representation to strong.
		/// </summary>
		public static string FromUTF8Bytes(this byte[] bytes)
		{
			return Encoding.UTF8.GetString(bytes);
		}
		#endregion

		#region ToHexString
		/// <summary>
		/// Converts a byte array to a hex string.
		/// </summary>
		public static string ToHexString(this IEnumerable<byte> bytes)
		{
			return new string(bytes.SelectMany(x => x.ToString("x2").ToCharArray()).ToArray());
		}
		#endregion

		#region Standard Base64 Extensions
		/// <summary>
		/// Base64-encodes a byte array.
		/// </summary>
		public static string Base64Encode(this byte[] input)
		{
			return Convert.ToBase64String(input);
		}
		/// <summary>
		/// Base64-decodes a byte array.
		/// </summary>
		public static byte[] Base64Decode(this string input)
		{
			return Convert.FromBase64String(input);
		}
		#endregion

		#region Base64WebSafeEncode
		/// <summary>
		/// Converts a byte array into a "web-safe" Base64 encoding.  This
		/// is the same as standard Base64 encoding, but with the /+= replaced
		/// with -._, making it safe for insertion into a URL without encoding,
		/// and protecting against tripping the ASP.NET "dangerous request"
		/// exception triggered by input values matching m/on[a-z]*=/i, which
		/// can happen if you put regular Base64 in any form variable, cookie,
		/// querystring, etc.  Use Base64WebSafeDecode to reverse the transformation.
		/// </summary>
		/// <param name="input">Bytes to encode.</param>
		/// <returns>Bytes encoded as a string.</returns>
		public static string Base64WebSafeEncode(this byte[] input)
		{
			if ( input == null )
				return null;
			return Convert.ToBase64String(input)
				.Replace("/", "-")
				.Replace("+", ".")
				.Replace("=", "_");
		}
		#endregion

		#region Base64WebSafeDecode
		/// <summary>
		/// Converts a string that was encoded with Base64WebSafeEncode back to
		/// the original byte array.
		/// </summary>
		/// <param name="input">String to decode.</param>
		/// <returns>Bytes that were encoded in the string.</returns>
		public static byte[] Base64WebSafeDecode(this string input)
		{
			if ( input == null )
				return null;
			return Convert.FromBase64String(input
				.Replace("-", "/")
				.Replace(".", "+")
				.Replace("_", "="));
		}
		#endregion

		#region Byte[] Hash Functions
		/// <summary>
		/// Get the MD5 hash as bytes.
		/// </summary>
		public static byte[] GetMD5Bytes(this byte[] binaryValue)
		{
			return new MD5CryptoServiceProvider().ComputeHash(binaryValue);
		}
		/// <summary>
		/// Get the SHA1 hash as bytes.
		/// </summary>
		public static byte[] GetSHA1Bytes(this byte[] binaryValue)
		{
			return new SHA1CryptoServiceProvider().ComputeHash(binaryValue);
		}
		/// <summary>
		/// Get the keyed HMAC SHA1 hash as bytes.
		/// </summary>
		public static byte[] GetHMACSHA1Bytes(this byte[] binaryValue, byte[] hMACKey)
		{
			return new HMACSHA1(hMACKey, false).ComputeHash(binaryValue);
		}
		/// <summary>
		/// Get the keyed HMAC SHA1 hash as bytes.
		/// </summary>
		public static byte[] GetHMACSHA1Bytes(this byte[] binaryValue, string hMACKey)
		{
			return new HMACSHA1(GetUTF8Bytes(hMACKey), false).ComputeHash(binaryValue);
		}
		#endregion

		#region String Hash Functions
		/// <summary>
		/// Get the MD5 hash as bytes.
		/// </summary>
		public static byte[] GetMD5Bytes(this string stringValue)
		{
			return new MD5CryptoServiceProvider().ComputeHash(GetUTF8Bytes(stringValue));
		}
		/// <summary>
		/// Get the SHA1 hash as bytes.
		/// </summary>
		public static byte[] GetSHA1Bytes(this string stringValue)
		{
			return new SHA1CryptoServiceProvider().ComputeHash(GetUTF8Bytes(stringValue));
		}
		/// <summary>
		/// Get the keyed HMAC SHA1 hash as bytes.
		/// </summary>
		public static byte[] GetHMACSHA1Bytes(this string stringValue, byte[] hMACKey)
		{
			return new HMACSHA1(hMACKey, false).ComputeHash(GetUTF8Bytes(stringValue));
		}
		/// <summary>
		/// Get the keyed HMAC SHA1 hash as bytes.
		/// </summary>
		public static byte[] GetHMACSHA1Bytes(this string stringValue, string hMACKey)
		{
			return new HMACSHA1(GetUTF8Bytes(hMACKey), false).ComputeHash(GetUTF8Bytes(stringValue));
		}
		#endregion

		#region Stream Hash Functions
		/// <summary>
		/// Get the MD5 hash as bytes.
		/// </summary>
		public static byte[] GetMD5Bytes(this Stream streamValue)
		{
			return new MD5CryptoServiceProvider().ComputeHash(streamValue);
		}
		/// <summary>
		/// Get the SHA1 hash as bytes.
		/// </summary>
		public static byte[] GetSHA1Bytes(this Stream streamValue)
		{
			return new SHA1CryptoServiceProvider().ComputeHash(streamValue);
		}
		/// <summary>
		/// Get the keyed HMAC SHA1 hash as bytes.
		/// </summary>
		public static byte[] GetHMACSHA1Bytes(this Stream streamValue, byte[] hMACKey)
		{
			return new HMACSHA1(hMACKey, false).ComputeHash(streamValue);
		}
		/// <summary>
		/// Get the keyed HMAC SHA1 hash as bytes.
		/// </summary>
		public static byte[] GetHMACSHA1Bytes(this Stream streamValue, string hMACKey)
		{
			return new HMACSHA1(GetUTF8Bytes(hMACKey), false).ComputeHash(streamValue);
		}
		#endregion

		#region CBCEncrypt
		/// <summary>
		/// Encrypts plaintext with a symmetric block cipher in CBC mode, with
		/// PKCS7 padding.  Up to one block size of overhead is added to the
		/// ciphertext.  Works with input of any size.
		/// </summary>
		/// <param name="alg">Symmetric algorithm with which to encrypt.</param>
		/// <param name="plaintext">Content to be encrypted.</param>
		/// <returns>Encrypted content.</returns>
		public static byte[] CBCEncrypt(this SymmetricAlgorithm alg, byte[] plaintext)
		{
			byte[] OldIV = alg.IV;
			CipherMode OldMode = alg.Mode;
			PaddingMode OldPad = alg.Padding;
			try
			{
				alg.IV = Enumerable.Repeat((byte)0, alg.IV.Length).ToArray();
				alg.Mode = CipherMode.CBC;
				alg.Padding = PaddingMode.PKCS7;
				return alg.CreateEncryptor().TransformFinalBlock(plaintext, 0, plaintext.Length);
			}
			finally
			{
				alg.IV = OldIV;
				alg.Mode = OldMode;
				alg.Padding = OldPad;
			}
		}
		#endregion

		#region CBCDecrypt
		/// <summary>
		/// Decrypts data encrypted with CBCEncrypt.
		/// </summary>
		/// <param name="alg">Symmetric algorithm with which to decrypt.</param>
		/// <param name="ciphertext">Content to be decrypted.</param>
		/// <returns>Decrypted content.</returns>
		public static byte[] CBCDecrypt(this SymmetricAlgorithm alg, byte[] ciphertext)
		{
			byte[] OldIV = alg.IV;
			CipherMode OldMode = alg.Mode;
			PaddingMode OldPad = alg.Padding;
			try
			{
				alg.IV = Enumerable.Repeat((byte)0, alg.IV.Length).ToArray();
				alg.Mode = CipherMode.CBC;
				alg.Padding = PaddingMode.PKCS7;
				return alg.CreateDecryptor().TransformFinalBlock(ciphertext, 0, ciphertext.Length);
			}
			finally
			{
				alg.IV = OldIV;
				alg.Mode = OldMode;
				alg.Padding = OldPad;
			}
		}
		#endregion

		#region CTSEncrypt
		/// <summary>
		/// Encrypts data with the given symmetric block cipher in CBC mode with
		/// ciphertext stealing.  Ciphertext stealing is only enabled if the input
		/// is not an exact multiple of the cipher block size, otherwise plain
		/// unpadded CBC is used.  A minimum of one block size of input is required.
		/// Ciphertext is the same size as plaintext (there is no overhead space).
		/// </summary>
		/// <remarks>
		/// http://en.wikipedia.org/wiki/Ciphertext_stealing#CBC_implementation_notes
		/// </remarks>
		/// <param name="alg">Symmetric algorithm with which to encrypt.</param>
		/// <param name="plaintext">Content to be encrypted.</param>
		/// <returns>Encrypted content.</returns>
		public static byte[] CTSEncrypt(this SymmetricAlgorithm alg, byte[] plaintext)
		{
			byte[] OldIV = alg.IV;
			CipherMode OldMode = alg.Mode;
			PaddingMode OldPad = alg.Padding;
			try
			{
				alg.IV = Enumerable.Repeat((byte)0, alg.IV.Length).ToArray();
				alg.Mode = CipherMode.CBC;
				alg.Padding = PaddingMode.None;

				int BlockSize = alg.CreateEncryptor().InputBlockSize;
				if ( alg.CreateEncryptor().OutputBlockSize != BlockSize )
					throw new Exception("Chosen algorithm for CTS encryption has non-matching input/output block sizes.");

				if ( plaintext.Length < BlockSize )
					throw new Exception("CTS encryption with given algorithm requires at least " + BlockSize.ToString() + " bytes of plaintext.");
				if ( (plaintext.Length % BlockSize) == 0 )
					return alg.CreateEncryptor().TransformFinalBlock(plaintext, 0, plaintext.Length);

				alg.Padding = PaddingMode.Zeros;
				byte[] CipherText = alg.CreateEncryptor().TransformFinalBlock(plaintext, 0, plaintext.Length);

				int CipherLen = CipherText.Length;
				int LastBlockSize = plaintext.Length + BlockSize - CipherLen;
				return CipherText.Take(CipherLen - BlockSize * 2)
					.Concat(CipherText.Skip(CipherLen - BlockSize).Take(BlockSize))
					.Concat(CipherText.Skip(CipherLen - BlockSize * 2).Take(LastBlockSize))
					.ToArray();
			}
			finally
			{
				alg.IV = OldIV;
				alg.Mode = OldMode;
				alg.Padding = OldPad;
			}
		}
		#endregion

		#region CTSDecrypt
		/// <summary>
		/// Decrypts data encrypted with CTSEncrypt.
		/// </summary>
		/// <remarks>
		/// http://en.wikipedia.org/wiki/Ciphertext_stealing#CBC_implementation_notes
		/// </remarks>
		/// <param name="alg">Symmetric algorithm with which to decrypt.</param>
		/// <param name="ciphertext">Content to be decrypted.</param>
		/// <returns>Decrypted content.</returns>
		public static byte[] CTSDecrypt(this SymmetricAlgorithm alg, byte[] ciphertext)
		{
			byte[] OldIV = alg.IV;
			CipherMode OldMode = alg.Mode;
			PaddingMode OldPad = alg.Padding;
			try
			{
				alg.IV = Enumerable.Repeat((byte)0, alg.IV.Length).ToArray();
				alg.Mode = CipherMode.CBC;
				alg.Padding = PaddingMode.None;

				int BlockSize = alg.CreateDecryptor().InputBlockSize;
				if ( alg.CreateDecryptor().OutputBlockSize != BlockSize )
					throw new Exception("Chosen algorithm for CTS encryption has non-matching input/output block sizes.");

				if ( ciphertext.Length < BlockSize )
					throw new Exception("CTS decryption with given algorithm requires at least " + BlockSize.ToString() + " bytes of ciphertext.");
				if ( (ciphertext.Length % BlockSize) == 0 )
					return alg.CreateDecryptor().TransformFinalBlock(ciphertext, 0, ciphertext.Length);

				int LastBlock = (int)(ciphertext.Length / BlockSize) * BlockSize;
				int SecLastBlock = LastBlock - BlockSize;
				byte[] PadC = ciphertext.Skip(SecLastBlock).Take(BlockSize).ToArray();
				byte[] PadP = alg.CreateDecryptor().TransformFinalBlock(PadC, 0, PadC.Length);

				byte[] Padded = ciphertext.Take(SecLastBlock)
					.Concat(ciphertext.Skip(LastBlock))
					.Concat(PadP.Skip(ciphertext.Length % BlockSize))
					.Concat(ciphertext.Skip(SecLastBlock).Take(BlockSize))
					.ToArray();
				byte[] PlainText = alg.CreateDecryptor().TransformFinalBlock(Padded, 0, Padded.Length);

				return PlainText.Take(ciphertext.Length).ToArray();
			}
			finally
			{
				alg.IV = OldIV;
				alg.Mode = OldMode;
				alg.Padding = OldPad;
			}
		}
		#endregion

		#region DeflateCompress
		/// <summary>
		/// Compresses a series of bytes using the Deflate algorithm.
		/// </summary>
		public static IEnumerable<byte> DeflateCompress(this IEnumerable<byte> bytes, CompressionLevel level = CompressionLevel.Default)
		{
			byte[] ByteArray = bytes.ToArray();
			using ( MemoryStream R = new MemoryStream() )
			{
				R.Write(ByteArray, 0, ByteArray.Length);
				R.Seek(0, SeekOrigin.Begin);
				using ( MemoryStream W = new MemoryStream() )
				{
					using ( DeflateStream D = new DeflateStream(W, CompressionMode.Compress, level, true) )
					{
						R.CopyTo(D);
						D.Flush();
					}
					W.Seek(0, SeekOrigin.Begin);
					ByteArray = new byte[W.Length];
					W.Read(ByteArray, 0, ByteArray.Length);
				}
			}
			return ByteArray;
		}
		#endregion

		#region DeflateDecompress
		/// <summary>
		/// Decompresses a series of bytes using the Deflate algorithm.
		/// </summary>
		public static IEnumerable<byte> DeflateDecompress(this IEnumerable<byte> bytes)
		{
			byte[] ByteArray = bytes.ToArray();
			using ( MemoryStream R = new MemoryStream() )
			{
				R.Write(ByteArray, 0, ByteArray.Length);
				R.Seek(0, SeekOrigin.Begin);
				using ( MemoryStream W = new MemoryStream() )
				{
					using ( DeflateStream D = new DeflateStream(R, CompressionMode.Decompress, true) )
						D.CopyTo(W);
					W.Seek(0, SeekOrigin.Begin);
					ByteArray = new byte[W.Length];
					W.Read(ByteArray, 0, ByteArray.Length);
				}
			}
			return ByteArray;
		}
		#endregion

		#region String IsYes
		/// <summary>
		/// In various places in our code, we are accepting a "liberal" set of
		/// string values to represent a boolean "true."  This extension method
		/// standardizes the set of "yes" values we accept.
		/// </summary>
		/// <remarks>
		/// <para>NULL or empty string returns false.</para>
		/// <para>If it can parse it as an Int32, != 0 returns true.</para>
		/// <para>If it can parse it as a Bool then it is evaluated as normal.</para>
		/// <para>If it starts with Y (case insensitive) then it returns true.</para>
		/// </remarks>
		public static bool IsYes(this string inputString)
		{
			if ( string.IsNullOrWhiteSpace(inputString) )
				return false;

			inputString = inputString.Trim();

			bool BoolValue = false;
			if ( bool.TryParse(inputString, out BoolValue) )
				return BoolValue;

			int IntValue = 0;
			if ( int.TryParse(inputString, out IntValue) )
				return IntValue != 0;

			if ( inputString.StartsWith("Y", StringComparison.CurrentCultureIgnoreCase) )
				return true;

			return false;
		}
		#endregion

		#region String IsDesc
		//public static bool IsDesc(this string inputString)
		//{
		//    if ( string.IsNullOrWhiteSpace(inputString) )
		//        return false;

		//    if ( inputString.Trim().Equals("DESC", StringComparison.CurrentCultureIgnoreCase) )
		//        return true;

		//    return false;
		//}
		#endregion

		#region SysRowStampToString
		///// <summary>
		///// Converts a sysRowStamp value into hexdecimal string (no leading
		///// zeros), suitable for use as a Since= value in the QueryString.
		///// </summary>
		///// <remarks>
		///// We cannot safely use Base64 for this, because (1) +, /, and =
		///// have special meanings in a QueryString, and we don't want to rely
		///// on the client escaping QueryString parameters correctly, and (2)
		///// there is a chance the stamp will match the m/on[a-z]+=/i regex
		///// that .NET uses to detect "dangerous requests".  Non-padded Hex
		///// encoding is designed to take advantage of the fact that these
		///// numbers start at zero and count up, probably not very high for a
		///// long time...
		///// </remarks>
		//public static string SysRowStampToString(this byte[] inputBytes)
		//{
		//    if ( inputBytes == null )
		//        return string.Empty;

		//    // Assume input is in big-endian order; convert to little-endian,
		//    // ensure input is at least long enough to hold a UInt64 (zero-extend),
		//    // then discard any excess MSB's.
		//    byte[] Zero = BitConverter.GetBytes(0L);
		//    inputBytes = inputBytes.Reverse().Concat(Zero).Take(Zero.Length).ToArray();

		//    // Convert byte input into correct endian order for this architecture.
		//    // This conversion is backwards, since the array is now in little endian
		//    // order, instead of the standard big-endian.
		//    if ( !BitConverter.IsLittleEndian )
		//        inputBytes = inputBytes.Reverse().ToArray();

		//    // Convert value into hex and return.
		//    string Hex = BitConverter.ToUInt64(inputBytes, 0).ToString("X");
		//    return Hex;
		//}
		#endregion

		#region StringToSysRowStamp
		///// <summary>
		///// Converts a hex string, encoded using SysRowStampToString, back
		///// into a sysRowStamp value.
		///// </summary>
		//public static byte[] StringToSysRowStamp(this string inputString)
		//{
		//    if ( string.IsNullOrEmpty(inputString) )
		//        return null;

		//    // Convert a hex-encoded string into it UInt64 value, and get the
		//    // array of bytes from it.
		//    byte[] Bytes = BitConverter.GetBytes(Convert.ToUInt64(inputString, 16));

		//    // If we're on a little endian architecture, reverse the array into
		//    // big endian before returning.
		//    if ( BitConverter.IsLittleEndian )
		//        Bytes = Bytes.Reverse().ToArray();
		//    return Bytes;
		//}
		#endregion

		#region ChainAfter
		///// <summary>
		///// Concatenates a new action after an existing one, and returns an
		///// action representing the original action, followed by the new action.
		///// Either action can be null.  If both actions are null, null is returned.
		///// </summary>
		///// <param name="preAction">Action to which the postAction is added.</param>
		///// <param name="postAction">Action which is added after the preAction.</param>
		///// <returns>A new action representing the combined sequence of actions,
		///// or null if both inputs are null.</returns>
		//public static Action ChainAfter(this Action preAction, Action postAction)
		//{
		//    if ( preAction != null )
		//    {
		//        if ( postAction != null )
		//            return new Action(() => { preAction.Invoke(); postAction.Invoke(); });
		//        else
		//            return preAction;
		//    }
		//    else if ( postAction != null )
		//        return postAction;
		//    return null;
		//}
		///// <summary>
		///// Concatenates a new action after an existing one, and returns an
		///// action representing the original action, followed by the new action.
		///// Either action can be null.  If both actions are null, null is returned.
		///// </summary>
		///// <param name="preAction">Action to which the postAction is added.</param>
		///// <param name="postAction">Action which is added after the preAction.</param>
		///// <returns>A new action representing the combined sequence of actions,
		///// or null if both inputs are null.</returns>
		//public static Action<T> ChainAfter<T>(this Action<T> preAction, Action<T> postAction)
		//{
		//    if ( preAction != null )
		//    {
		//        if ( postAction != null )
		//            return new Action<T>(V => { preAction.Invoke(V); postAction.Invoke(V); });
		//        else
		//            return preAction;
		//    }
		//    else if ( postAction != null )
		//        return postAction;
		//    return null;
		//}
		#endregion

		#region ChainBefore
		///// <summary>
		///// Concatenates a new action before an existing one, and returns an
		///// action representing the original action, preceeded by the new action.
		///// Either action can be null.  If both actions are null, null is returned.
		///// </summary>
		///// <param name="postAction">Action which to which the preAction is added.</param>
		///// <param name="preAction">Action which is added before the postAction.</param>
		///// <returns>A new action representing the combined sequence of actions,
		///// or null if both inputs are null.</returns>
		//public static Action ChainBefore(this Action postAction, Action preAction)
		//{
		//    return preAction.ChainAfter(postAction);
		//}
		///// <summary>
		///// Concatenates a new action before an existing one, and returns an
		///// action representing the original action, preceeded by the new action.
		///// Either action can be null.  If both actions are null, null is returned.
		///// </summary>
		///// <param name="postAction">Action which to which the preAction is added.</param>
		///// <param name="preAction">Action which is added before the postAction.</param>
		///// <returns>A new action representing the combined sequence of actions,
		///// or null if both inputs are null.</returns>
		//public static Action<T> ChainBefore<T>(this Action<T> postAction, Action<T> preAction)
		//{
		//    return preAction.ChainAfter(postAction);
		//}
		#endregion

		#region Overlay
		/// <summary>
		/// Overlays one dictionary over another, and returns the resulting dictionary (original
		/// is not modified).  Pairs from the overlay dictionary will override any pairs with
		/// matching keys copied in from the original.
		/// </summary>
		public static Dictionary<X, Y> Overlay<X, Y>(this Dictionary<X, Y> orig, IEnumerable<KeyValuePair<X, Y>> overlay)
		{
			Dictionary<X, Y> NewDict = new Dictionary<X, Y>();
			foreach ( KeyValuePair<X, Y> P in orig )
				NewDict[P.Key] = P.Value;
			foreach ( KeyValuePair<X, Y> P in overlay )
				NewDict[P.Key] = P.Value;
			return NewDict;
		}
		#endregion

		#region UrlCombine
		/// <summary>
		/// Combine two parts of a URL path, with a forward-slash
		/// delimiter.  URL analog to System.IO.Path.Combine
		/// </summary>
		public static string UrlCombine(this string firstPart, string secondPart, char Delim = '/')
		{
			return firstPart.TrimEnd(Delim) + Delim.ToString() + secondPart.TrimStart(Delim);
		}
		#endregion

		#region IsNumeric
		/// <summary>
		/// Is the object numeric.
		/// </summary>
		public static bool IsNumeric(this object Expression)
		{
			double Parsed;
			bool IsNum = Double.TryParse(Convert.ToString(Expression),
				System.Globalization.NumberStyles.Any,
				System.Globalization.NumberFormatInfo.InvariantInfo,
				out Parsed);
			return IsNum;
		}
		#endregion

		#region Solo
		/// <summary>
		/// Wraps a single object into an IEnumerable of that
		/// type, so single results can be returned from "Get
		/// List" functions, concatenated onto other Enumerables,
		/// etc.
		/// </summary>
		public static IEnumerable<T> Solo<T>(this T input)
		{
			if ( input == null )
				return new T[0];
			return new T[] { input };
		}
		#endregion

		#region Concat
		/// <summary>
		/// Override for Concat that allows us to use a single
		/// element instead of having to wrap it in something enumerable.
		/// </summary>
		public static IEnumerable<T> Concat<T>(this IEnumerable<T> enumer, T toAdd)
		{
			return enumer.Concat(toAdd.Solo());
		}
		#endregion

		#region ChunkedSelect
		///// <summary>
		///// Split a stream of objects into fixed-size chunks, and perform chunked operations on them.
		///// Doing too many at a time can trigger scalability issues on the size of queries, while too
		///// few can trigger scalability problems on the number of queries.
		///// </summary>
		//public static void ChunkedExecute<TSource>(this IEnumerable<TSource> source, int chunkSize,
		//    Action<IEnumerable<TSource>> transformChunk)
		//{
		//    List<TSource> Chunk = new List<TSource>();
		//    foreach ( TSource S in source )
		//    {
		//        Chunk.Add(S);
		//        if ( Chunk.Count >= chunkSize )
		//        {
		//            List<TSource> UpChunk = Chunk;
		//            transformChunk.Invoke(UpChunk);
		//            Chunk = new List<TSource>();
		//        }
		//    }
		//    if ( Chunk.Any() )
		//        transformChunk.Invoke(Chunk);
		//}
		///// <summary>
		///// Split a stream of objects into fixed-size chunks, and perform chunked operations on them.
		///// Doing too many at a time can trigger scalability issues on the size of queries, while too
		///// few can trigger scalability problems on the number of queries.
		///// </summary>
		//public static IEnumerable<TResult> ChunkedSelect<TSource, TResult>(this IEnumerable<TSource> source, int chunkSize,
		//    Func<IEnumerable<TSource>, IEnumerable<TResult>> transformChunk)
		//{
		//    List<TResult> Results = new List<TResult>();
		//    source.ChunkedExecute(chunkSize, S => Results.AddRange(transformChunk.Invoke(S)));
		//    return Results;
		//}
		///// <summary>
		///// Split a stream of objects into fixed-size chunks, and perform chunked operations on them.
		///// Doing too many at a time can trigger scalability issues on the size of queries, while too
		///// few can trigger scalability problems on the number of queries.  This variant runs each
		///// chunk in parallel using the thread pool.
		///// </summary>
		//public static void ChunkedExecuteParallel<TSource>(this IEnumerable<TSource> source, int chunkSize,
		//    Action<IEnumerable<TSource>> transformChunk)
		//{
		//    object MonitorObject = new object();
		//    List<Exception> Exceptions = new List<Exception>();
		//    int ThreadCount = 0;
		//    Action<IEnumerable<TSource>> StartThread = L =>
		//    {
		//        Interlocked.Increment(ref ThreadCount);
		//        ThreadManager.ThreadPoolQueue(() =>
		//        {
		//            try { transformChunk.Invoke(L); }
		//            catch ( Exception ex )
		//            {
		//                lock ( Exceptions )
		//                    Exceptions.Add(ex);
		//            }
		//            Interlocked.Decrement(ref ThreadCount);
		//            lock ( MonitorObject )
		//                Monitor.Pulse(MonitorObject);
		//        });
		//    };
		//    source.ChunkedExecute(chunkSize, StartThread);
		//    lock ( MonitorObject )
		//        while ( ThreadCount > 0 )
		//            Monitor.Wait(MonitorObject);
		//    if ( Exceptions.Any() )
		//        throw new Exception("Exception thrown by ChunkedExecuteParallel Worker(s):"
		//            + Environment.NewLine + string.Join(Environment.NewLine,
		//            Exceptions.Select(E => E.ExceptionDump())), Exceptions.First());
		//}
		///// <summary>
		///// Split a stream of objects into fixed-size chunks, and perform chunked operations on them.
		///// Doing too many at a time can trigger scalability issues on the size of queries, while too
		///// few can trigger scalability problems on the number of queries.  This variant runs each
		///// chunk in parallel using the thread pool.
		///// </summary>
		//public static IEnumerable<TResult> ChunkedSelectParallel<TSource, TResult>(this IEnumerable<TSource> source, int chunkSize,
		//    Func<IEnumerable<TSource>, IEnumerable<TResult>> transformChunk)
		//{
		//    List<TResult> Results = new List<TResult>();
		//    source.ChunkedExecuteParallel(chunkSize, S =>
		//    {
		//        IEnumerable<TResult> Temp = transformChunk.Invoke(S);
		//        lock ( Results )
		//            Results.AddRange(Temp);
		//    });
		//    return Results;
		//}

		//public static int ChunkedExecuteProcessorBatchSize<TSource>(this IEnumerable<TSource> source, int? pcount = null)
		//{
		//    if ( source == null )
		//        return 0;

		//    if ( !pcount.HasValue )
		//        pcount = Environment.ProcessorCount;

		//    int s = source.Count() / pcount.Value;
		//    if ( s == 0 )
		//        s++;

		//    return s;
		//}
		#endregion

		#region Get/HasAttribute
		/// <summary>
		/// Syntactically shortened helper to get the first attribute from a member
		/// that matches the specified type.  Does NOT return inherited attribute types.
		/// </summary>
		public static T GetAttribute<T>(this MemberInfo mi, bool inherited = false) where T : Attribute
		{
			return mi.GetCustomAttributes(typeof(T), inherited).OfType<T>().FirstOrDefault();
		}
		/// <summary>
		/// Syntactically shortened helper to find if a member has an attribute
		/// that matches the specified type.  Accepts inherited attribute types.
		/// </summary>
		public static bool HasAttribute<T>(this MemberInfo mi) where T : Attribute
		{
			return mi.GetCustomAttributes(typeof(T), true).OfType<T>().Any();
		}
		#endregion

		#region DisplayName
		/// <summary>
		/// Gets the display name for an object's type, as defined
		/// by a DisplayName attribute, or a reasonable fallback.
		/// </summary>
		public static string DisplayName(this object obj)
		{
			if ( obj == null )
				return "null";
			for ( Type T = obj.GetType(); T != null; T = T.BaseType )
			{
				DisplayNameAttribute DNA = T.GetAttribute<DisplayNameAttribute>(false);
				if ( (DNA != null) && !string.IsNullOrWhiteSpace(DNA.DisplayName) )
					return DNA.DisplayName;
			}
			return obj.GetType().Name;
		}
		#endregion

		#region SetDefaultContextFilterType
		//public static void SetDefaultContextFilterType(this Type type)
		//{
		//    if ( type.HasAttribute<EntityContextFilterLevelAttribute>() )
		//        DataManagementContext.Current.FilterLevel = type
		//            .GetAttribute<EntityContextFilterLevelAttribute>().FilterLevel;
		//}
		#endregion

		#region SetDefaultContextFilterTypeIfUnspecified
		//public static void SetDefaultContextFilterTypeIfUnspecified(this Type type)
		//{
		//    if ( DataManagementContext.Current.FilterLevel == ContextFilterLevel.Unspecified )
		//        SetDefaultContextFilterType(type);
		//}
		#endregion

		#region IsMissingColumns
		//public static bool IsMissingColumns(this Type type, SchemaTemplateColumn columns)
		//{
		//    return type.GetCustomAttributes(typeof(SchemaTemplateMissingColumnsAttribute), true)
		//        .OfType<SchemaTemplateMissingColumnsAttribute>()
		//        .Select(A => A.MissingColumns)
		//        .Any(M => (M & columns) != 0);
		//}
		//public static bool IsMissingColumns(this DMEntityBase entity, SchemaTemplateColumn columns)
		//{
		//    return entity.GetType().IsMissingColumns(columns);
		//}
		#endregion

		#region DefaultTo
		/// <summary>
		/// Override the value of a default value.  For reference types, this
		/// is equivalent to the ?? operator.  For value types, it replaces
		/// default values, such as 0 for integers or DateTime.MinValue for DateTime.
		/// </summary>
		public static TVal DefaultTo<TVal>(this TVal originalValue, TVal fallbackValue)
		{
			return (object.Equals(originalValue, null) || originalValue.Equals(default(TVal))) ? fallbackValue : originalValue;
		}
		#endregion

		#region ChopOff
		/// <summary>
		/// Chops off a string that's too long, and returns trimmed/chopped string.
		/// </summary>
		/// <param name="toChop">String to chop.</param>
		/// <param name="maxLength">Maximum total number of characters to return.</param>
		/// <param name="elipsis">If string is chopped, replace the end part with this string.</param>
		/// <returns>The beginning part of the string, limited to maxLength characters.</returns>
		public static string ChopOff(this string toChop, int maxLength, string elipsis = "...")
		{
			if ( elipsis.Length > maxLength )
				throw new Exception(string.Format("Elipsis value (\"{0}\", {1} characters long) is "
					+ "longer than the maximum string length specified ({2}.", elipsis, elipsis.Length, maxLength));
			if ( string.IsNullOrEmpty(toChop) )
				return toChop;
			toChop = toChop.Trim();
			if ( toChop.Length > maxLength )
				return toChop.Substring(0, maxLength - elipsis.Length) + elipsis;
			return toChop;
		}
		#endregion

		#region InvokeUnlessDefault
		/// <summary>
		/// Invoke an action if the input is not default.  For reference
		/// types, default means null.
		/// </summary>
		public static void InvokeUnlessDefault<TObj>(this TObj obj, Action<TObj> toRun)
		{
			if ( !object.Equals((object)obj, (object)default(TObj)) )
				toRun.Invoke(obj);
		}
		/// <summary>
		/// Invoke a function if the input is not its type's default value.
		/// For reference types, default means null.  Returns the result of
		/// the function if it was invoked, or the specified default value
		/// (or default for the result type) if not invoked.
		/// </summary>
		public static TResult InvokeUnlessDefault<TObj, TResult>(this TObj obj, Func<TObj, TResult> toRun, TResult defaultResult = default(TResult))
		{
			TResult Result = defaultResult;
			obj.InvokeUnlessDefault(O => { Result = toRun.Invoke(O); });
			return Result;
		}
		#endregion

		#region EndianFlip
		/// <summary>
		/// Used to coerce byte arrays to/from a specific endianness.  Call this
		/// once on input, and once on output, and it will flip the bytes if the
		/// endianness of the platform is not the correct one.  Defaults to flipping
		/// on little-endian architectures (force data over the wire into big endian).
		/// </summary>
		public static IEnumerable<byte> EndianFlip(this IEnumerable<byte> toFlip, bool flipBigEndian = false)
		{
			return (BitConverter.IsLittleEndian ^ flipBigEndian) ? toFlip.Reverse() : toFlip;
		}
		#endregion

		#region EnumTryParse
		/// <summary>
		/// Tries to parse a string as an enum value.  Returns parsed value, or
		/// default value if parsing fails.
		/// </summary>
		/// <typeparam name="TEnum">Enum value type into which to parse.</typeparam>
		/// <param name="toParse">String to parse as enum.</param>
		/// <param name="defValue">An optional alternate default value, instead of default(TEnum).</param>
		public static TEnum EnumTryParse<TEnum>(this string toParse, TEnum defValue = default(TEnum))
			 where TEnum : struct
		{
			TEnum Val = defValue;
			if ( toParse == null )
				return Val;
			Enum.TryParse<TEnum>(toParse, true, out Val);
			return Val;
		}
		#endregion

		#region SetDefaultValues
		/// <summary>
		/// Sets the value of any property that has a "DefaultValue" attribute
		/// to its defined default value.
		/// </summary>
		public static void SetDefaultValues(this object O)
		{
			foreach ( KeyValuePair<PropertyInfo, DefaultValueAttribute> Pair in O.GetType()
				.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
				.Where(P => P.CanWrite && !P.GetIndexParameters().Any())
				.Select(P => new KeyValuePair<PropertyInfo, DefaultValueAttribute>(P, P.GetCustomAttributes(typeof(DefaultValueAttribute),
					true).OfType<DefaultValueAttribute>().FirstOrDefault()))
				.Where(P => P.Value != null) )
				Pair.Key.SetValue(O, Convert.ChangeType(Pair.Value.Value, Pair.Key.PropertyType), null);
		}
		#endregion

		#region Locked
		/// <summary>
		/// Syntactic sugar to run a quick function while an object is locked.
		/// </summary>
		public static T Locked<T>(this object obj, Func<T> func)
		{
			lock ( obj )
				return func.Invoke();
		}

		public static void Locked(this object obj, Action toExec)
		{
			lock ( obj )
				toExec.Invoke();
		}
		#endregion

		#region DataContractXMLSerialize
		/// <summary>
		/// Serializes a DataContract-serializable object into an XML fragment.
		/// </summary>
		public static string DataContractXMLSerialize<T>(this T obj)
		{
			StringBuilder XSB = new StringBuilder();
			using ( XmlWriter XW = XmlWriter.Create(XSB, new XmlWriterSettings
			{
				CloseOutput = false,
				ConformanceLevel = ConformanceLevel.Fragment,
				Indent = true,
				IndentChars = "\t",
				NamespaceHandling = NamespaceHandling.OmitDuplicates,
				NewLineChars = Environment.NewLine,
				NewLineHandling = NewLineHandling.Replace,
				NewLineOnAttributes = true
			}) )
			{
				new DataContractSerializer(typeof(T)).WriteObject(XW, obj);
				XW.Flush();
			}
			return XSB.ToString();
		}
		#endregion

		#region DataContractXMLDeserialize
		/// <summary>
		/// Deserializes a DataContract-serializable object from an XML fragment.
		/// </summary>
		public static T DataContractXMLDeserialize<T>(this string str)
		{
			using ( XmlReader XR = XmlReader.Create(new StringReader(str)) )
				return (T)new DataContractSerializer(typeof(T)).ReadObject(XR);
		}
		#endregion

		#region DataContractJSONSerialize
		/// <summary>
		/// Serializes a DataContract-serializable object into a JSON fragment.
		/// </summary>
		public static string DataContractJSONSerialize<T>(this T obj)
		{
			using ( MemoryStream MS = new MemoryStream() )
			{
				new DataContractJsonSerializer(typeof(T)).WriteObject(MS, obj);
				MS.Position = 0;
				StreamReader SR = new StreamReader(MS, true);
				return SR.ReadToEnd();
			}
		}
		#endregion

		#region DataContractJSONDeserialize
		/// <summary>
		/// Deserializes a DataContract-serializable object from a JSON fragment.
		/// </summary>
		public static T DataContractJSONDeserialize<T>(this string str)
		{
			using ( MemoryStream MS = new MemoryStream() )
			{
				using ( StreamWriter SW = new StreamWriter(MS, Encoding.UTF8) )
				{
					SW.Write(str);
					SW.Flush();
				}
				object O = new DataContractJsonSerializer(typeof(T)).ReadObject(MS);
				return (O is T) ? (T)O : default(T);
			}
		}
		#endregion

		#region TryGetKeep
		/// <summary>
		///  Try to get a value from a dictionary by key.  If the key is missing,
		///  generate a new value and insert it in the dictionary at that key.
		/// </summary>
		public static TValue TryGetKeep<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, Func<TValue> getDefault)
		{
			TValue Val = default(TValue);
			if ( !dict.TryGetValue(key, out Val) )
			{
				Val = getDefault.Invoke();
				dict[key] = Val;
			}
			return Val;
		}
		#endregion

		#region EmptyIfNull
		/// <summary>
		/// If the enumeration is null, return an empty enumeration.
		/// </summary>
		public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T> enumer)
		{
			return enumer.DefaultTo(new T[0]);
		}
		#endregion

		#region With
		/// <summary>
		/// Modify an object mid-expression.
		/// </summary>
		public static T With<T>(this T what, Action<T> toDo)
		{
			toDo.Invoke(what);
			return what;
		}
		#endregion

		#region ToDictionaryIgnoreDupes
		/// <summary>
		/// Similar to ToDictionary(), but clobber duplicate records instead
		/// of throwing an exception.
		/// </summary>
		public static Dictionary<TKey, TValue> ToDictionaryIgnoreDupes<TRaw, TKey, TValue>(this IEnumerable<TRaw> input,
			Func<TRaw, TKey> getKey, Func<TRaw, TValue> getValue, IEqualityComparer<TKey> comparer)
		{
			Dictionary<TKey, TValue> dict = new Dictionary<TKey, TValue>(comparer);
			foreach ( TRaw raw in input )
				dict[getKey.Invoke(raw)] = getValue.Invoke(raw);
			return dict;
		}
		/// <summary>
		/// Similar to ToDictionary(), but clobber duplicate records instead
		/// of throwing an exception.
		/// </summary>
		public static Dictionary<TKey, TValue> ToDictionaryIgnoreDupes<TKey, TValue>(this IEnumerable<TValue> input,
			Func<TValue, TKey> getKey, IEqualityComparer<TKey> comparer)
		{
			return input.ToDictionaryIgnoreDupes(getKey, V => V, comparer);
		}
		/// <summary>
		/// Similar to ToDictionary(), but clobber duplicate records instead
		/// of throwing an exception.
		/// </summary>
		public static Dictionary<TKey, TValue> ToDictionaryIgnoreDupes<TRaw, TKey, TValue>(this IEnumerable<TRaw> input,
			Func<TRaw, TKey> getKey, Func<TRaw, TValue> getValue)
		{
			Dictionary<TKey, TValue> dict = new Dictionary<TKey, TValue>();
			foreach ( TRaw raw in input )
				dict[getKey.Invoke(raw)] = getValue.Invoke(raw);
			return dict;
		}
		/// <summary>
		/// Similar to ToDictionary(), but clobber duplicate records instead
		/// of throwing an exception.
		/// </summary>
		public static Dictionary<TKey, TValue> ToDictionaryIgnoreDupes<TKey, TValue>(this IEnumerable<TValue> input,
			Func<TValue, TKey> getKey)
		{
			return input.ToDictionaryIgnoreDupes(getKey, V => V);
		}
		#endregion

		#region DistinctPreserveOrder
		/// <summary>
		/// Similar to Distinct(), but preserves the order of objects in
		/// the collection.  Only the first objuect matching the given key
		/// in the collection will be kept, subsequent items will be removed.
		/// </summary>
		public static IEnumerable<TValue> DistinctPreserveOrder<TKey, TValue>(this IEnumerable<TValue> inlist, Func<TValue, TKey> getDistinctKey)
		{
			HashSet<TKey> Hash = new HashSet<TKey>();
			List<TValue> Output = new List<TValue>();
			foreach ( TValue Item in inlist )
			{
				TKey Key = getDistinctKey(Item);
				if ( Hash.Contains(Key) )
					continue;
				Hash.Add(Key);
				Output.Add(Item);
			}
			return Output;
		}
		/// <summary>
		/// Similar to Distinct(), but preserves the order of objects in
		/// the collection.  Only the first objuect matching the given key
		/// in the collection will be kept, subsequent items will be removed.
		/// </summary>
		public static IEnumerable<TValue> DistinctPreserveOrder<TValue>(this IEnumerable<TValue> inlist)
		{
			return inlist.DistinctPreserveOrder(I => I);
		}
		#endregion

		#region GetConnectionStringProperty
		///// <summary>
		///// If this is an Entity Framework connection string, remove the Entity Framework
		///// components and return the inner "provider connection string" component.  Does
		///// not return the provider type.
		///// </summary>
		//public static string ConnectionStringRemoveEF(this string connectionString)
		//{
		//    DbConnectionStringBuilder Builder = new DbConnectionStringBuilder();
		//    Builder.ConnectionString = connectionString;
		//    const string PROV = "provider connection string";
		//    if ( Builder.ContainsKey(PROV) && (Builder[PROV] != null) )
		//        return Builder[PROV].ToString().ConnectionStringRemoveEF();
		//    return connectionString;
		//}
		///// <summary>
		///// Gets a selected property from a connection string.  If an Entity connection string
		///// is provided, it searches the inner store connection string.
		///// </summary>
		//public static string GetConnectionStringProperty<TBuilder>(this string connectionString, Func<TBuilder, object> getProperty)
		//    where TBuilder : DbConnectionStringBuilder, new()
		//{
		//    TBuilder Builder2 = new TBuilder();
		//    Builder2.ConnectionString = connectionString.ConnectionStringRemoveEF();
		//    object O = getProperty.Invoke(Builder2);
		//    if ( O != null )
		//        return O.ToString() ?? string.Empty;
		//    return string.Empty;
		//}
		///// <summary>
		///// Gets the initial catalog name from an MSSQL connection string.
		///// </summary>
		//public static string GetConnectionStringSqlDatabase(this string connectionString)
		//{
		//    return GetConnectionStringProperty<SqlConnectionStringBuilder>(connectionString, B => B.InitialCatalog);
		//}
		///// <summary>
		///// Gets the server name name from an MSSQL connection string.
		///// </summary>
		//public static string GetConnectionStringSqlServer(this string connectionString)
		//{
		//    return GetConnectionStringProperty<SqlConnectionStringBuilder>(connectionString, B => B.DataSource);
		//}
		#endregion

		#region SanitizeConnectionString
		///// <summary>
		///// Try to parse the string as a connection string.  If the string is a valid
		///// connection string, find any fields that look like a password field (contain
		///// the text "password") and blank them out.  Works recursively on entity
		///// connection strings.  If the string is not a valid connection string, it
		///// is returned unmodified.
		///// </summary>
		//public static string SanitizeConnectionString(this string connStr)
		//{
		//    try
		//    {
		//        DbConnectionStringBuilder Builder = new DbConnectionStringBuilder();
		//        Builder.ConnectionString = connStr;
		//        foreach ( string Key in Builder.Keys.OfType<string>().ToArray() )
		//            if ( Key.ToUpper().Contains("PASSWORD") )
		//            {
		//                Builder[Key] = "********";
		//            }
		//            else
		//            {
		//                Builder[Key] = Builder[Key].ToString().SanitizeConnectionString();
		//            }
		//        return Builder.ToString();
		//    }
		//    catch
		//    {
		//        return connStr;
		//    }
		//}
		#endregion

		#region Action Wrap
		/// <summary>
		/// Wrap an outer action around this one, and return the resulting
		/// composite action.
		/// </summary>
		/// <param name="act">Inner action to be wrapped.</param>
		/// <param name="wrapper">Wrapper, that invokes the inner action.</param>
		/// <returns>Composite action that represents the inner action
		/// wrapped by the wrapper.</returns>
		public static Action Wrap(this Action act, Action<Action> wrapper)
		{
			return () => wrapper.Invoke(act);
		}
		#endregion

		#region Function Wrap
		/// <summary>
		/// Wrap a function or transformation around another function, and
		/// return the resulting composite function.
		/// </summary>
		/// <typeparam name="TIn">Return type of the input function.</typeparam>
		/// <typeparam name="TOut">Return type of the wrapper function.</typeparam>
		/// <param name="func">Inner function to be wrapped and invoked by the wrapper.</param>
		/// <param name="wrapper">Wrapper function that transforms the inner function.</param>
		/// <returns>Composite function representing the inner function wrapped by the wrapper.</returns>
		public static Func<TOut> Wrap<TIn, TOut>(this Func<TIn> func, Func<Func<TIn>, TOut> wrapper)
		{
			return () => wrapper.Invoke(func);
		}
		#endregion

		#region Function WrapAction
		/// <summary>
		/// Wrap a function with an action.  Used to transform an action wrapper that
		/// accepts an action into a function wrapper.
		/// </summary>
		/// <typeparam name="T">Return type of the inner function.</typeparam>
		/// <param name="func">Inner function to invoke.</param>
		/// <param name="wrapper">Action wrapper to be applied around the function.</param>
		/// <returns>A composite function representing the action wrapper wrapping around
		/// the action of invoking the function, then returning the invocation result.</returns>
		public static Func<T> WrapAction<T>(this Func<T> func, Action<Action> wrapper)
		{
			return () =>
			{
				T Result = default(T);
				wrapper.Invoke(() => Result = func.Invoke());
				return Result;
			};
		}
		#endregion

		#region ExceptionDump
		/// <summary>
		/// Generate a dump on an exception, including inner exception
		/// linked list, more thoroughly than Exception.ToString().
		/// </summary>
		public static string ExceptionDump(this Exception ex)
		{
			StringBuilder SB = new StringBuilder();
			for ( Exception e = ex; e != null; e = e.InnerException )
			{
				if ( SB.Length > 0 )
					SB.AppendLine();
				SB.AppendLine("Type: " + e.GetType().FullName);
				SB.AppendLine("Message: " + e.Message);
				if ( !string.IsNullOrEmpty(e.HelpLink) )
					SB.AppendLine("HelpLink: " + e.HelpLink);
				if ( !string.IsNullOrEmpty(e.Source) )
					SB.AppendLine("Source: " + e.Source);
				if ( e.TargetSite != null )
					SB.AppendLine("TargetSite: " + e.TargetSite.ToString());
				if ( !string.IsNullOrEmpty(e.StackTrace) )
					SB.AppendLine("StackTrace: " + e.StackTrace);
			}
			return SB.ToString();
		}
		#endregion

		#region RandomSample
		/// <summary>
		/// Collect a uniform simple random sample from an enumerable.
		/// </summary>
		/// <param name="list">The population over which to sample.</param>
		/// <param name="maxSize">The maximum size of the sample.</param>
		/// <returns>The sample of up to maxSize elements.  Elements within
		/// the list are chosen randomly, but are in their original order.  If
		/// the list size is less than or equal to maxSize, this will be the
		/// entire list.</returns>
		public static IEnumerable<T> RandomSample<T>(this IEnumerable<T> list, int maxSize)
		{
			Random R = new Random();
			int N = 0;
			return list.Aggregate(new List<T>(), (L, I) =>
			{
				N++;
				if ( L.Count < maxSize )
					L.Add(I);
				else if ( R.Next(N) == 0 )
				{
					L.RemoveAt(R.Next(L.Count));
					L.Add(I);
				}
				return L;
			});
		}
		#endregion

		#region Shuffle
		/// <summary>
		/// Performs an efficient (O(n)) Fischer-Yates shuffle on a
		/// finite set of inputs, and returns a copy of the set in
		/// random order.
		/// </summary>
		public static IEnumerable<T> Shuffle<T>(IEnumerable<T> input)
		{
			T[] ShuffleArray = input.ToArray();
			if ( ShuffleArray.Length < 2 )
				return ShuffleArray;

			byte[] RandBytes = BitConverter.GetBytes((int)0);
			new RNGCryptoServiceProvider().GetBytes(RandBytes);
			Random Rand = new Random(BitConverter.ToInt32(RandBytes, 0));

			for ( int X = ShuffleArray.Length - 1; X > 0; X-- )
			{
				int Y = Rand.Next(0, X);
				if ( Y != X )
				{
					T Temp = ShuffleArray[X];
					ShuffleArray[X] = ShuffleArray[Y];
					ShuffleArray[Y] = Temp;
				}
			}

			return ShuffleArray;
		}
		#endregion

		#region Recurse
		/// <summary>
		/// Searches a tree of objects and returns all nodes found in a single
		/// flattened IEnumerable.  Nodes are visited in arbitrary order, and
		/// returned in visited order.
		/// </summary>
		public static IEnumerable<T> Recurse<T>(this T root, Func<T, IEnumerable<T>> funcChildren)
		{
			List<T> Found = new List<T>(root.Solo());
			Queue<T> Q = new Queue<T>();
			Q.Enqueue(root);
			while ( Q.Count > 0 )
			{
				T Cur = Q.Dequeue();
				Found.Add(Cur);
				List<T> Children = funcChildren.Invoke(Cur).ToList();
				foreach ( T Child in Children )
					Q.Enqueue(Child);
			}
			return Found;
		}
		#endregion

		#region DataTable ToList
		/// <summary>
		/// Return a list of T types from the DataTable.
		/// MOVED TO ExpoExchange.EXS.Common CRMExtensions()
		/// </summary>
		/// <remarks>
		/// Any Property in the type with a matching column name in the
		/// DataTable is populated in the object list returned.
		/// </remarks>
		/// <returns>List of T objects.</returns>
		//public static List<T> ToList<T>(this DataTable dt) where T : new()
		//{
		//	List<T> TR = new List<T>();
		//	// Get mapping of properties to columns that we want to convert.
		//	var PI = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
		//		.Where(P => P.CanWrite && !P.GetIndexParameters().Any() && dt.Columns.Contains(P.Name))
		//		.Select(P => new Tuple<PropertyInfo, int>(P, dt.Columns.IndexOf(P.Name)))
		//		.ToList();
		//	foreach ( DataRow DR in dt.Rows )
		//	{
		//		T NewObj = new T();
		//		//NewObj.SetDefaultValues();
		//		PI.ForEach((P) =>
		//		{
		//			// Got to do some strange stuff for DBNull values in the DataTable.
		//			if ( P.Item1.PropertyType.IsGenericType && P.Item1.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>) )
		//			{
		//				dynamic objValue = System.Activator.CreateInstance(P.Item1.PropertyType);
		//				objValue = Convert.IsDBNull(DR[P.Item2]) ? null : DR[P.Item2];
		//				P.Item1.SetValue(NewObj, (object)objValue, null);
		//			}
		//			else
		//				P.Item1.SetValue(NewObj, DR[P.Item2], null);

		//			//P.Item1.SetValue(NewObj, Convert.ChangeType(DR[P.Item2], P.Item1.PropertyType), null);
		//		});
		//		TR.Add(NewObj);
		//	}
		//	return TR;
		//}
		#endregion

		#region | ToDataTable |
		public static DataTable ToDataTable<T>(this List<T> items)
		{
			var tb = new DataTable(typeof(T).Name);
			var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
			foreach ( PropertyInfo prop in props )
			{
				Type t = GetCoreType(prop.PropertyType);
				tb.Columns.Add(prop.Name, t);
			}
			foreach ( T item in items )
			{
				var values = new object[props.Length];
				for ( int i = 0; i < props.Length; i++ )
				{
					values[i] = props[i].GetValue(item, null);
				}
				tb.Rows.Add(values);
			}
			return tb;
		}
		/// <summary>
		/// Determine of specified type is nullable
		/// </summary>
		public static bool IsNullable(Type t)
		{
			return !t.IsValueType || (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>));
		}
		/// <summary>
		/// Return underlying type if type is Nullable otherwise return the type
		/// </summary>
		public static Type GetCoreType(Type t)
		{
			if ( t != null && IsNullable(t) )
			{
				if ( !t.IsValueType )
				{
					return t;
				}
				else
				{
					return Nullable.GetUnderlyingType(t);
				}
			}
			else
			{
				return t;
			}
		}
		#endregion

		#region | Splice |
		public static IEnumerable<string> Splice(this string s, int spliceLength)
		{
			if ( s == null )
				throw new ArgumentNullException("s");
			if ( spliceLength < 1 )
				throw new ArgumentOutOfRangeException("spliceLength");

			if ( s.Length == 0 )
				yield break;
			var start = 0;
			for ( var end = spliceLength; end < s.Length; end += spliceLength )
			{
				yield return s.Substring(start, spliceLength);
				start = end;
			}
			yield return s.Substring(start);
		}
		#endregion

		#region | Split |
		public static string[] Split(this string s, int length)
		{
			if ( s == null )
				throw new ArgumentNullException("s");
			if ( length < 1 )
				throw new ArgumentOutOfRangeException("length");

			System.Globalization.StringInfo str = new System.Globalization.StringInfo(s);

			int lengthAbs = Math.Abs(length);

			if ( str == null || str.LengthInTextElements == 0 || lengthAbs == 0 || str.LengthInTextElements <= lengthAbs )
				return new string[] { str.String };

			string[] array = new string[(str.LengthInTextElements % lengthAbs == 0 ? str.LengthInTextElements / lengthAbs : (str.LengthInTextElements / lengthAbs) + 1)];

			if ( length > 0 )
				for ( int iStr = 0, iArray = 0; iStr < str.LengthInTextElements && iArray < array.Length; iStr += lengthAbs, iArray++ )
					array[iArray] = str.SubstringByTextElements(iStr, (str.LengthInTextElements - iStr < lengthAbs ? str.LengthInTextElements - iStr : lengthAbs));
			else
				for ( int iStr = str.LengthInTextElements - 1, iArray = array.Length - 1; iStr >= 0 && iArray >= 0; iStr -= lengthAbs, iArray-- )
					array[iArray] = str.SubstringByTextElements((iStr - lengthAbs < 0 ? 0 : iStr - lengthAbs + 1), (iStr - lengthAbs < 0 ? iStr + 1 : lengthAbs));

			return array;
		}
		#endregion
	}
}
