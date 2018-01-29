using System;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace App.Framework.Business
{
	public class CMSecurity
	{
		#region properties

		private static Hashtable _keyHash = new Hashtable();
		private static Hashtable _ivHash = new Hashtable();

		#endregion

		#region methods

		#region logging

		private static void WriteEventLogEntry(EventLogEntryType etype, string message)
		{
			try
			{
				string SM_SOURCE = "EventXLDecryptLogging";

				if ( !EventLog.SourceExists(SM_SOURCE) )
					EventLog.CreateEventSource(SM_SOURCE, "Application");

				EventLog.WriteEntry(SM_SOURCE, message, etype);
			}
			catch { }
		}

		#endregion

		#region derectifier

		public static string DeRectifier(string toUse, string enterpriseConnectionString)
		{
			return DeRectifier(toUse, enterpriseConnectionString, false);
		}

		public static string DeRectifier(string toUse, string enterpriseConnectionString, bool refresh)
		{
			string uEnterpriseConnectionString = enterpriseConnectionString.ToUpper().Trim();

			if ( refresh )
			{
				if ( _keyHash.ContainsKey(uEnterpriseConnectionString) )
					_keyHash.Remove(uEnterpriseConnectionString);
				if ( _ivHash.ContainsKey(uEnterpriseConnectionString) )
					_ivHash.Remove(uEnterpriseConnectionString);
			}

			// check cache for enterprise connection string and use if possible
			string rkey = "";
			string riv = "";

			lock ( _keyHash )
			{
				lock ( _ivHash )
				{
					if ( _keyHash.ContainsKey(uEnterpriseConnectionString)
						&& _ivHash.ContainsKey(uEnterpriseConnectionString) )
					{
						rkey = _keyHash[uEnterpriseConnectionString].ToString();
						riv = _ivHash[uEnterpriseConnectionString].ToString();
					}
					else
					{
						// retrieve based on enterpriseconnectionstring
						try
						{
							DataTable kres = new DataTable();

							SqlConnection connection = new SqlConnection(enterpriseConnectionString);
							SqlCommand command = new SqlCommand("dbo.spGetKeys", connection);
							command.CommandType = CommandType.StoredProcedure;
							SqlDataAdapter dadapt = new SqlDataAdapter(command);

							connection.Open();
							dadapt.Fill(kres);

							if ( kres.Rows.Count > 0 )
							{
								rkey = kres.Rows[0]["key"].ToString();
								riv = kres.Rows[0]["IV"].ToString();
							}

							if ( connection.State == ConnectionState.Open )
								connection.Close();

							if ( rkey.Length > 0
								&& riv.Length > 0 )
							{
								// add to cache
								_keyHash.Add(uEnterpriseConnectionString, rkey);
								_ivHash.Add(uEnterpriseConnectionString, riv);
							}

							StringBuilder sb = new StringBuilder();
							sb.AppendFormat("Encryption values set. rkey: {0}, riv: {1}, rent: {2}, stack trace: {3}", rkey, riv, enterpriseConnectionString, Environment.StackTrace);
							WriteEventLogEntry(EventLogEntryType.Information, sb.ToString());
						}
						catch ( System.Exception err )
						{
							StringBuilder sb = new StringBuilder();
							sb.AppendFormat("Exception while retrieving encryption values. rent: {0}, error: {1}, stack trace: {2}", enterpriseConnectionString, err.Message, err.StackTrace);
							WriteEventLogEntry(EventLogEntryType.Error, sb.ToString());
						}
					}

				}
			}

			SymmetricAlgorithm CryptoService = new RijndaelManaged();

			byte[] Key = ASCIIEncoding.ASCII.GetBytes(rkey);
			byte[] IV = ASCIIEncoding.ASCII.GetBytes(riv);

			CryptoService.Key = Key;
			CryptoService.IV = IV;

			byte[] In = System.Convert.FromBase64String(toUse);

			ICryptoTransform Decryptor = CryptoService.CreateDecryptor();
			System.IO.MemoryStream MemoryCache = new System.IO.MemoryStream(In);
			CryptoStream DecryptedStream = new CryptoStream(MemoryCache, Decryptor, CryptoStreamMode.Read);


			// write out encrypted content into MemoryStream
			byte[] res = new byte[In.Length];
			DecryptedStream.Read(res, 0, res.Length);

			toUse = ASCIIEncoding.ASCII.GetString(res);

			// remove excess padding
			toUse = toUse.Replace("\0", "");

			return toUse;
		}

		public static string DeRectifier(string toUse, string enterpriseConnectionString, string rkey, string riv)
		{
			SymmetricAlgorithm CryptoService = new RijndaelManaged();

			byte[] Key = ASCIIEncoding.ASCII.GetBytes(rkey);
			byte[] IV = ASCIIEncoding.ASCII.GetBytes(riv);

			CryptoService.Key = Key;
			CryptoService.IV = IV;

			byte[] In = System.Convert.FromBase64String(toUse);

			ICryptoTransform Decryptor = CryptoService.CreateDecryptor();
			System.IO.MemoryStream MemoryCache = new System.IO.MemoryStream(In);
			CryptoStream DecryptedStream = new CryptoStream(MemoryCache, Decryptor, CryptoStreamMode.Read);

			// write out encrypted content into MemoryStream
			byte[] res = new byte[In.Length];
			DecryptedStream.Read(res, 0, res.Length);

			toUse = ASCIIEncoding.ASCII.GetString(res);

			// remove excess padding
			toUse = toUse.Replace("\0", "");

			return toUse;
		}

		#endregion

		#region rectifier

		public static string Rectifier(string toUse, string enterpriseConnectionString)
		{
			return Rectifier(toUse, enterpriseConnectionString, false);
		}

		public static string Rectifier(string toUse, string enterpriseConnectionString, bool refresh)
		{
			string uEnterpriseConnectionString = enterpriseConnectionString.ToUpper().Trim();

			if ( refresh )
			{
				if ( _keyHash.ContainsKey(uEnterpriseConnectionString) )
					_keyHash.Remove(uEnterpriseConnectionString);
				if ( _ivHash.ContainsKey(uEnterpriseConnectionString) )
					_ivHash.Remove(uEnterpriseConnectionString);
			}

			// check cache for enterprise connection string and use if possible
			string rkey = "";
			string riv = "";

			lock ( _keyHash )
			{
				lock ( _ivHash )
				{
					if ( _keyHash.ContainsKey(uEnterpriseConnectionString)
						&& _ivHash.ContainsKey(uEnterpriseConnectionString) )
					{
						rkey = _keyHash[uEnterpriseConnectionString].ToString();
						riv = _ivHash[uEnterpriseConnectionString].ToString();
					}
					else
					{
						// retrieve based on enterpriseconnectionstring
						try
						{
							DataTable kres = new DataTable();

							SqlConnection connection = new SqlConnection(enterpriseConnectionString);
							SqlCommand command = new SqlCommand("dbo.spGetKeys", connection);
							command.CommandType = CommandType.StoredProcedure;
							SqlDataAdapter dadapt = new SqlDataAdapter(command);

							connection.Open();
							dadapt.Fill(kres);

							if ( kres.Rows.Count > 0 )
							{
								rkey = kres.Rows[0]["key"].ToString();
								riv = kres.Rows[0]["IV"].ToString();
							}

							if ( connection.State == ConnectionState.Open )
								connection.Close();

							if ( rkey.Length > 0
								&& riv.Length > 0 )
							{
								// add to cache
								_keyHash.Add(uEnterpriseConnectionString, rkey);
								_ivHash.Add(uEnterpriseConnectionString, riv);
							}

							StringBuilder sb = new StringBuilder();
							sb.AppendFormat("Encryption values set. rkey: {0}, riv: {1}, rent: {2}, stack trace: {3}", rkey, riv, enterpriseConnectionString, Environment.StackTrace);
							WriteEventLogEntry(EventLogEntryType.Information, sb.ToString());
						}
						catch ( System.Exception err )
						{
							StringBuilder sb = new StringBuilder();
							sb.AppendFormat("Exception while retrieving encryption values. rent: {0}, error: {1}, stack trace: {2}", enterpriseConnectionString, err.Message, err.StackTrace);
							WriteEventLogEntry(EventLogEntryType.Error, sb.ToString());
						}
					}
				}
			}

			SymmetricAlgorithm CryptoService = new RijndaelManaged();

			byte[] Key = ASCIIEncoding.ASCII.GetBytes(rkey);
			byte[] IV = ASCIIEncoding.ASCII.GetBytes(riv);

			CryptoService.Key = Key;
			CryptoService.IV = IV;

			byte[] In = ASCIIEncoding.ASCII.GetBytes(toUse);

			ICryptoTransform Encryptor = CryptoService.CreateEncryptor();
			System.IO.MemoryStream MemoryCache = new System.IO.MemoryStream();
			CryptoStream EncryptedStream = new CryptoStream(MemoryCache, Encryptor, CryptoStreamMode.Write);

			EncryptedStream.Write(In, 0, In.Length);
			EncryptedStream.FlushFinalBlock();

			byte[] Out = MemoryCache.ToArray();

			toUse = System.Convert.ToBase64String(Out);

			return toUse;
		}

		public static string Rectifier(string toUse, string enterpriseConnectionString, string rkey, string riv)
		{
			SymmetricAlgorithm CryptoService = new RijndaelManaged();

			byte[] Key = ASCIIEncoding.ASCII.GetBytes(rkey);
			byte[] IV = ASCIIEncoding.ASCII.GetBytes(riv);

			CryptoService.Key = Key;
			CryptoService.IV = IV;

			byte[] In = ASCIIEncoding.ASCII.GetBytes(toUse);

			ICryptoTransform Encryptor = CryptoService.CreateEncryptor();
			System.IO.MemoryStream MemoryCache = new System.IO.MemoryStream();
			CryptoStream EncryptedStream = new CryptoStream(MemoryCache, Encryptor, CryptoStreamMode.Write);

			EncryptedStream.Write(In, 0, In.Length);
			EncryptedStream.FlushFinalBlock();

			byte[] Out = MemoryCache.ToArray();

			toUse = System.Convert.ToBase64String(Out);

			return toUse;
		}
		#endregion

		#region flush

		public static void Flush()
		{
			_keyHash = new Hashtable();
			_ivHash = new Hashtable();
		}
		#endregion

		#endregion
	}
}
