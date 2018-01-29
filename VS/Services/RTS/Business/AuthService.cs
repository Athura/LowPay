using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Lowpay.Framework.Business;
using Lowpay.Framework.Common;
using RTS.Service.Common;

namespace RTS.Service.Business
{
	public class AuthService
	{
		private const string L2_USERNAME = "Lead2";

		#region Password Encryption

		#region CreateCipher
		/// <summary>
		/// Creates the symmetric cipher used for built-in encryption.  Currently, this is
		/// an AES-256 cipher, keyed with the SHA-256 digest of the provided key.
		/// </summary>
		private static SymmetricAlgorithm CreateCipher(string key)
		{
			SymmetricAlgorithm Cipher = null;

			try { Cipher = new AesCryptoServiceProvider(); }
			catch { Cipher = new AesManaged(); }

			try { Cipher.Key = new SHA256CryptoServiceProvider().ComputeHash(Encoding.UTF8.GetBytes(key)); }
			catch { Cipher.Key = new SHA256Managed().ComputeHash(Encoding.UTF8.GetBytes(key)); }

			return Cipher;
		}
		#endregion

		public static string EncryptPassword(string pwd)
		{
			SymmetricAlgorithm alg = CreateCipher(Constants.SECRET_PREVIEW_HMAC_KEY);
			var TR = alg.CBCEncrypt(pwd.GetUTF8Bytes()).Base64Encode();
			return TR;
		}
		private static string DecryptPassword(string encryptedPassword)
		{
			SymmetricAlgorithm alg = CreateCipher(Constants.SECRET_PREVIEW_HMAC_KEY);
			var TR = alg.CBCDecrypt(encryptedPassword.Base64Decode()).FromUTF8Bytes();
			return TR;
		}
		#endregion

		#region LoginByUserNamePassword
		public static UserAuthToken LoginByUserNamePassword(string userName, string password, TimeSpan ttl)
		{
			const string LOG_TITLE = "LoginByUserNamePassword";

			if ( !string.IsNullOrWhiteSpace(userName) && !string.IsNullOrWhiteSpace(password) )
			{
				// Special case for the LEAD2 program.
				if ( userName.Equals(L2_USERNAME, StringComparison.CurrentCultureIgnoreCase) )
				{
					var AI = EXSLogger.CreateAdditionalItemList("UserName", userName);
					//AI["UserName"] = userName;
					AI["Password"] = string.IsNullOrEmpty(password) ? "" :
										password.Length < 2 ? "*" :
										password.Substring(0, 1) + "**********" + password.Substring(password.Length - 1, 1);
					AI["TimeToLive"] = ttl;

					if ( ValidLead2TokenKeys.Contains(EncryptPassword(password)) )
					{
						var Token = new UserAuthToken()
						{
							UserName = L2_USERNAME,
							Password = password,
							ExpiresUTC = (ttl == TimeSpan.Zero) ? DateTime.MaxValue : DateTime.UtcNow.Add(ttl)
						};
						Token.GenerateAuthTokenKey();

						AI["TokenExpDateUTC"] = Token.ExpiresUTC.ToString();
						EXSLogger.Log("Password is valid", LOG_TITLE, LogSeverity.Information, AI);

						return Token;
					}
					EXSLogger.Log("Password is NOT valid", LOG_TITLE, LogSeverity.Information, AI);
				}
				else
				{
					// Else we're going to use Windows Domain Auth.
					return LoginUsingAD(userName, password, ttl);
				}
			}
			else
				EXSLogger.Log("User Name and/or Password is Blank", LOG_TITLE, LogSeverity.Warning);
			return null;
		}
		#endregion

		#region LoginByAuthToken
		public static UserAuthToken LoginByAuthToken(string auth, TimeSpan ttl)
		{
			// If this has expired, this operation will return NULL.
			UserAuthToken CurrentToken = null;
			try { CurrentToken = UserAuthToken.FromAuthTokenKey(auth); }
			catch ( Exception ex ) { ex.Log(); }
			if ( CurrentToken != null )
			{
				// Special case for the LEAD2 program.
				if ( CurrentToken.UserName.Equals(L2_USERNAME, StringComparison.CurrentCultureIgnoreCase) )
				{
					if ( ValidLead2TokenKeys.Contains(EncryptPassword(CurrentToken.Password)) )
					{
						var Token = new UserAuthToken()
						{
							UserName = CurrentToken.UserName,
							Password = CurrentToken.Password,
							ExpiresUTC = (ttl == TimeSpan.Zero) ? DateTime.MaxValue : DateTime.UtcNow.Add(ttl)
						};
						Token.GenerateAuthTokenKey();
						return Token;
					}
				}
				else
				{
					return LoginUsingAD(CurrentToken.UserName, CurrentToken.Password, ttl);
				}
			}
			return null;
		}
		#endregion

		#region GetUserNameFromAuthToken
		/// <summary>
		/// Retrieve the user name from the given auth token.
		/// </summary>
		/// <remarks>
		/// Extract the User's Name from a valid auth token.  If the auth token has expired or
		/// can't be decrypted for some reason, then NULL is returned.
		/// </remarks>
		/// <param name="auth">The authentication token to exract the user name from.</param>
		public static string GetUserNameFromAuthToken(string auth)
		{
			// If this has expired, this operation will return NULL.
			UserAuthToken CurrentToken = null;
			try { CurrentToken = UserAuthToken.FromAuthTokenKey(auth); }
			catch ( Exception ex ) { ex.Log(); }
			return (CurrentToken != null) ? CurrentToken.UserName : null;
		}
		#endregion

		#region CheckAuthToken
		public static UserAuthToken CheckAuthToken(string auth)
		{
			const string LOG_TITLE = "CheckAuthToken";

			UserAuthToken TR = null;
			try { TR = UserAuthToken.FromAuthTokenKey(auth); }
			catch ( Exception ex ) { ex.Log(LogSeverity.Warning); }
			if ( TR != null )
			{
				// Special case for the LEAD2 program.
				if ( TR.UserName.Equals(L2_USERNAME, StringComparison.CurrentCultureIgnoreCase) )
				{
					if ( ValidLead2TokenKeys.Contains(EncryptPassword(TR.Password)) )
					{
						var AI = EXSLogger.CreateAdditionalItemList("UserName", TR.UserName);
						AI["ExpDateUTC"] = TR.ExpiresUTC.ToString();
						EXSLogger.Log("Auth Token Valid", LOG_TITLE, LogSeverity.Verbose, AI);
						return TR;
					}
				}
				else
				{
					// Else we're going to use Windows Domain Auth.
					if ( LoginUsingAD(TR.UserName, TR.Password, TimeSpan.Zero) != null )
					{
						var AI = EXSLogger.CreateAdditionalItemList("UserName", TR.UserName);
						AI["ExpDateUTC"] = TR.ExpiresUTC.ToString();
						EXSLogger.Log("Auth Token Valid", LOG_TITLE, LogSeverity.Verbose, AI);
						return TR;
					}
				}
			}
			EXSLogger.Log("Auth Token is invalid", LOG_TITLE, LogSeverity.Warning);
			return null;
		}
		#endregion

		#region ValidLead2TokenKeys
		/// <summary>
		/// Valid security keys from the web.config (or wherever the data store)
		/// to be used for LEAD2 authentication.
		/// </summary>
		private static IEnumerable<string> ValidLead2TokenKeys
		{
			get
			{
				var TR = ApplicationCache.CacheManagerGet<SynchronizedCollection<string>>("_ConfigurationValidLead2TokenKeys_",
							() => new SynchronizedCollection<string>(), TimeSpan.Zero);
				if ( !TR.Any() )
				{
					// Pull from web.config.
					string Keys = ConfigurationManager.AppSettings["ValidLead2UserTokenKeys"];
					if ( !string.IsNullOrEmpty(Keys) )
					{
						lock ( TR.SyncRoot )
						{
							foreach ( string ThisKey in Keys.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries) )
								TR.Add(ThisKey);
						}
					}
				}
				return TR;
			}
		}
		#endregion

		#region LoginUsingAD
		private static UserAuthToken LoginUsingAD(string userName, string password, TimeSpan ttl)
		{
			const string LOG_TITLE = "LoginUsingAD";

			string CacheKey = "RTS.Service.Business.AuthController.LoginUsingAD:UserAuthToken:" + userName + ":" + password;

			var AI = EXSLogger.CreateAdditionalItemList("UserName", userName);
			//AI["UserName"] = userName;
			AI["Password"] = string.IsNullOrEmpty(password) ? "" :
								password.Length < 2 ? "*" :
								password.Substring(0, 1) + "**********" + password.Substring(password.Length - 1, 1);
			AI["TimeToLive"] = ttl;

			// Look in the cache first.  The process of going into AD takes forever (sometimes)!
			var URFromCache = ApplicationCache.CacheManagerGet<UserAuthToken>(CacheKey);
			if ( URFromCache != null )
			{
				// If it's in the cache but is expired, then we go through the entire
				// AD auth process again.  Otherwise lets work with this token.
				if ( URFromCache.ExpiresUTC > DateTime.UtcNow )
				{
					EXSLogger.Log("RTS.Service.Business.AuthService: Credentials found in my cache and were not expired.", LOG_TITLE, LogSeverity.Verbose, AI);
					// If they passed us a TimeSpan of Zero, then they're just checking the Auth.
					// Otherwise they're logging someone in so we need to apply their TimeSpan.
					if ( ttl != TimeSpan.Zero )
					{
						URFromCache.ExpiresUTC = DateTime.UtcNow.Add(ttl);
						URFromCache.GenerateAuthTokenKey();
					}
					return URFromCache;
				}
				// Clear item out of the cache - we're doing to go through the whole
				// AD process again.
				//ApplicationCache.CacheManagerSet(CacheKey, null, TimeSpan.Zero);
				ApplicationCache.CacheManagerRemove(CacheKey);
				EXSLogger.Log("RTS.Service.Business.AuthService: Credentials found in my cache, but were expired so we will check Active Directory again.", LOG_TITLE, LogSeverity.Verbose, AI);
			}

			// Else we have to go through this whole process.
			var ConString = ConnectionManager.GetRegEntConnectionString();

			bool adPassed = false;
			string joinedName = null;

			var adSetting = new ADSetting();
			string[] username = userName.Split('\\');
			if ( username.Length > 1 )
			{
				switch ( username[0].ToLower() )
				{
					case "expoexchange":
						adSetting.ADServer = "expoexchange.com";
						break;
					case "exporeg":
						adSetting.ADServer = "reg.expoexchange.com";
						break;
					case "conferon-inc":
						adSetting.ADServer = "conferon.local";
						break;
					default:
						adSetting.ADServer = "reg.expoexchange.com";
						break;
				}
				adSetting.ADUser = username[1];

				using ( PrincipalContext ctx = new PrincipalContext(ContextType.Domain, adSetting.ADServer) )
				{
					adPassed = ctx.ValidateCredentials(adSetting.ADUser, password);
					if ( adPassed )
						joinedName = userName;
				}
			}
			else
			{
				// Try all 3?
				adSetting.ADServer = "conferon.local|reg.expoexchange.com|expoexchange.com";
				adSetting.ADUser = userName;

				using ( PrincipalContext ctx = new PrincipalContext(ContextType.Domain, "conferon.local") )
					adPassed = ctx.ValidateCredentials(adSetting.ADUser, password);
				if ( adPassed )
					joinedName = string.Format("conferon-inc\\{0}", adSetting.ADUser);
				else
				{
					using ( PrincipalContext ctx = new PrincipalContext(ContextType.Domain, "reg.expoexchange.com") )
						adPassed = ctx.ValidateCredentials(adSetting.ADUser, password);

					if ( adPassed )
						joinedName = string.Format("exporeg\\{0}", adSetting.ADUser);
					else
					{
						using ( PrincipalContext ctx = new PrincipalContext(ContextType.Domain, "expoexchange.com") )
							adPassed = ctx.ValidateCredentials(adSetting.ADUser, password);

						if ( adPassed )
							joinedName = string.Format("expoexchange\\{0}", adSetting.ADUser);
					}
				}
			}

			adSetting.ADPassword = password;
			adSetting.ADPath = "LDAP://";

			if ( !adPassed )
			{
				EXSLogger.Log("RTS.Service.Business.AuthService: Active Directory login failed.", LOG_TITLE, LogSeverity.Verbose, AI);
				return null;
			}

			var Controller = new BusinessPrincipalController(ConString);

			// Perform the AD authentication for this username and password.
			//var UI = Controller.Login(adSetting);
			var UI = Controller.GetUserIdentity(joinedName);
			if ( UI != null )
			{
				// AD authentication works.  Now let's check to make sure this user has EXS_CRM
				// application rights according to the EXS Framework/system.
				DataTable ToFill = null;
				new SecurityController().LoadApplicationUsers(out ToFill, CRMConfig.ExternalApplicationCode);
				if ( ToFill.AsEnumerable().OfType<DataRow>().Any(R => R["UserID"] != DBNull.Value && Convert.ToInt32(R["UserID"]) == UI.UserID) )
				{
					var TR = new UserAuthToken()
					{
						//UserName = adSetting.ADUser,
						UserName = UI.UserName,
						Password = adSetting.ADPassword,
						ExpiresUTC = (ttl == TimeSpan.Zero) ? DateTime.MaxValue : DateTime.UtcNow.Add(ttl)
					};

					CacheKey = "RTS.Service.Business.AuthController.LoginUsingAD:UserAuthToken:" + UI.UserName + ":" + password;

					TR.GenerateAuthTokenKey();
					// Throw it in the cache.
					ApplicationCache.CacheManagerSet(CacheKey, TR, TimeSpan.FromMinutes(60));
					EXSLogger.Log("RTS.Service.Business.AuthService: Active Directory login succeeded.", LOG_TITLE, LogSeverity.Verbose, AI);
					return TR;
				}
				EXSLogger.Log("RTS.Service.Business.AuthService: Active Directory login succeeded, but user is not in the EXS CRM group.", LOG_TITLE, LogSeverity.Verbose, AI);
			}
			EXSLogger.Log("RTS.Service.Business.AuthService: Active Directory login failed.", LOG_TITLE, LogSeverity.Verbose, AI);
			return null;
		}
		#endregion

		#region GetUserName
		public static string GetUserName(string auth)
		{
			// If this has expired, this operation will return "".
			UserAuthToken CurrentToken = null;
			try { CurrentToken = UserAuthToken.FromAuthTokenKey(auth); }
			catch ( Exception ex ) { ex.Log(); }
			if ( CurrentToken != null )
			{
				// Special case for the LEAD2 program.
				if ( CurrentToken.UserName.Equals(L2_USERNAME, StringComparison.CurrentCultureIgnoreCase) )
				{
					if ( ValidLead2TokenKeys.Contains(EncryptPassword(CurrentToken.Password)) )
					{
						return CurrentToken.UserName;
					}
				}
				else
				{
					// Else we're going to use Windows Domain Auth.
					return CurrentToken.UserName;
				}
			}
			return "";
		}
		#endregion
	}
}
