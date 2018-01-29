using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;

namespace App.Framework.Business
{
	/// <summary>
	/// Summary description for ConnectionManagerBase.
	/// </summary>
	public class ConnectionManagerBase
	{
		public enum ServerTypeCode
		{
			//[Obsolete] RD = 0,
			DEV,
			QA,
			PROD,
			ONS
		}

		public enum AppTypeCode
		{
			Default = 0,
			ProductionManager
		}

		#region | Properties / Settings |
		/// <summary>
		/// Passed enterprise connection string. This will be used if it's not empty.
		/// </summary>
		private string _passedEntConnectionString = "";
		protected string passedEntConnectionString
		{
			get
			{
				return this._passedEntConnectionString;
			}
			set
			{
				this._passedEntConnectionString = value;
				GetServerTable(_passedEntConnectionString);
			}
		}

		/// <summary>
		/// Cached enterprise connection string. This can be passing in or create by this class.
		/// </summary>
		private static string _cachedEntConnectionString = "";
		protected static string cachedEntConnectionString
		{
			get
			{
				return _cachedEntConnectionString;
			}
			set
			{
				_cachedEntConnectionString = value;
				GetServerTable(_cachedEntConnectionString);
			}
		}

		/// <summary>
		/// List of servers from the server table on the current enterprise database.
		/// </summary>
		private static DataTable _serverTable = null;
		public static DataTable ServerTable
		{
			get
			{
				if ( _serverTable == null )
				{
					if ( _cachedEntConnectionString.Length > 0 )
						GetServerTable(_cachedEntConnectionString);
					else
						GetServerTable(new ConnectionManagerBase().GetEntConnectionString());
				}
				return _serverTable;
			}
		}

		/// <summary>
		/// Cached show connection strings.
		/// </summary>
		protected static Hashtable cachedConnectionStrings = new Hashtable();

		/// <summary>
		/// The AppTypeCode that will be used is passed AppTypeCode couldn't be found.
		/// </summary>
		public string DefaultAppTypeCode = "Default";

		/// <summary>
		/// Default enterprise SQL Server AppSettings key name in config file.
		/// </summary>
		public const string DefaultEntSqlServerAppSettingsKeyConst = "SqlServer";
		public string DefaultEntSqlServerAppSettingsKey
		{
			get
			{
				return DefaultEntSqlServerAppSettingsKeyConst;
			}
		}

		/// <summary>
		/// Default enterprise database name AppSettings key name in config file.
		/// </summary>
		public const string DefaultEntDatabaseNameAppSettingsKey = "EnterpriseDatabase";

		public string EntServerName
		{
			get
			{
				return this.GetEntServerName();
			}
		}

		public string EntDatabaseName
		{
			get
			{
				return this.GetEntDatabaseName();
			}
		}
		#endregion

		#region | Constructor |
		/// <summary>
		/// Pass in temporary enterprise connection string instead of retrieving from config file.
		/// Might be used to retrieve show connection string from other server.
		/// </summary>
		/// <param name="entConnectionString"></param>
		public ConnectionManagerBase(string entConnectionString)
		{
			CMSecurity.Flush();

			// need to validate if this is an enterprise connection string
			string enterpriseDatabaseName = "Enterprise";

			// VS2005
			if ( System.Configuration.ConfigurationManager.AppSettings[DefaultEntDatabaseNameAppSettingsKey] != null )
			{
				// VS2005
				enterpriseDatabaseName = System.Configuration.ConfigurationManager.AppSettings[DefaultEntDatabaseNameAppSettingsKey];
			}

			System.Text.RegularExpressions.Regex reg = new System.Text.RegularExpressions.Regex(@"database\s*=\s*" + enterpriseDatabaseName, System.Text.RegularExpressions.RegexOptions.IgnoreCase);

			if ( reg.IsMatch(entConnectionString) )
			{
				this.passedEntConnectionString = entConnectionString;
			}
			else
			{
				string newEntConnectionString = "";
				string[] part = entConnectionString.Split(';');
				for ( int i = 0; i < part.Length; i++ )
				{
					if ( part[i].Length > 0 )
					{
						if ( part[i].ToLower().Trim().IndexOf("database") == 0 )
						{
							part[i] = "database=" + enterpriseDatabaseName;
						}
						newEntConnectionString += part[i];
						if ( i != part.Length - 1 )
						{
							newEntConnectionString += ";";
						}
					}
				}
				this.passedEntConnectionString = newEntConnectionString;
			}
		}

		/// <summary>
		/// ConnectionManagerBase will create enterprise connection string based on the config file
		/// or overrided GetEntServerName and GetEntDatabaseName mthods.
		/// </summary>
		public ConnectionManagerBase()
		{
		}

		/// <summary>
		/// ConnectionManagerBase will create enterprise connection based on the parameters in this
		/// constructor.  
		/// </summary>
		/// <param name="ServerName"></param>
		/// <param name="UserName"></param>
		/// <param name="Password"></param>
		/// <param name="EnterpriseDBName"></param>
		public ConnectionManagerBase(string ServerName, string UserName, string Password, string EnterpriseDBName)
		{
			this.passedEntConnectionString = (string.IsNullOrEmpty(UserName) || string.IsNullOrEmpty(Password))
				? GetGenericConnectionString(ServerName, EnterpriseDBName)
				: GetGenericConnectionString(ServerName, EnterpriseDBName, UserName, Password);
		}
		#endregion

		#region | GetConnectionString |
		/// <summary>
		/// Get show connection string from cached if available.
		/// </summary>
		/// <param name="showCode"></param>
		/// <param name="appTypeCode"></param>
		/// <returns>Show connection string</returns>
		public virtual string GetConnectionString(string showCode, string appTypeCode)
		{
			return this.GetConnectionString(showCode, appTypeCode, true);
		}

		/// <summary>
		/// Get default show connection string from cache if available.
		/// </summary>
		/// <param name="showCode"></param>
		/// <returns>Show connection string</returns>
		public virtual string GetConnectionString(string showCode)
		{
			return this.GetConnectionString(showCode, this.DefaultAppTypeCode, true);
		}

		/// <summary>
		/// Get show connection string directly from database instead of cache.
		/// </summary>
		/// <param name="showCode"></param>
		/// <param name="appTypeCode"></param>
		/// <returns>Show connection string</returns>
		public virtual string GetConnectionStringFromDatabase(string showCode, string appTypeCode)
		{
			return this.GetConnectionString(showCode, appTypeCode, false);
		}

		/// <summary>
		/// Get default show connection string directly from database instead of cache.
		/// </summary>
		/// <param name="showCode"></param>
		/// <returns>Show connection string</returns>
		public virtual string GetConnectionStringFromDatabase(string showCode)
		{
			return this.GetConnectionString(showCode, this.DefaultAppTypeCode, false);
		}

		#region | private GetConnectionString(string showCode, string appTypeCode, bool useCached) |
		private string GetConnectionString(string showCode, string appTypeCode, bool useCached)
		{
			string cachedConnectionStringKey = string.Concat(this.GetEntServerName(), showCode, appTypeCode).ToLower();

			// return cached connection string if available
			if ( useCached && cachedConnectionStrings.ContainsKey(cachedConnectionStringKey) )
			{
				return cachedConnectionStrings[cachedConnectionStringKey].ToString();
			}

			string enterpriseConnectionString;

			// use passed enterprise connection string if available
			if ( this.passedEntConnectionString.Length > 0 )
			{
				enterpriseConnectionString = this.passedEntConnectionString;
			}
			else if ( cachedEntConnectionString.Length > 0 )
			{
				enterpriseConnectionString = cachedEntConnectionString;
			}
			else
			{
				cachedEntConnectionString = this.GetEntConnectionString();
				enterpriseConnectionString = cachedEntConnectionString;
			}

			return this.GetConnectionString(cachedConnectionStringKey, enterpriseConnectionString, showCode, appTypeCode);
		}
		#endregion

		#region | private GetConnectionString(string cachedConnectionStringKey, string enterpriseConnectionString, string showCode, string appTypeCode) |
		private string GetConnectionString(string cachedConnectionStringKey, string enterpriseConnectionString, string showCode, string appTypeCode)
		{
			// trip to enterprise database
			SqlConnection connection = new SqlConnection(enterpriseConnectionString);
			SqlCommand command = new SqlCommand("dbo.spShowConnectionSelectByShowCode", connection);
			command.CommandType = CommandType.StoredProcedure;

			// VS2005
			command.Parameters.AddWithValue("@ShowCode", showCode);

			SqlDataReader reader = null;
			try
			{
				connection.Open();
				reader = command.ExecuteReader(CommandBehavior.CloseConnection);
			}
			catch ( Exception ex )
			{
				if ( connection.State == ConnectionState.Open )
				{
					connection.Close();
				}
				throw new Exception("Failed open connection. " + ex.Message);
			}

			string defaultConnectionString = "";
			string connectionString = "";

			try
			{
				while ( reader.Read() )
				{
					// JDM: check if encrypted password column exists in results
					bool hasEPass = false;
					for ( int i = 0; i < reader.FieldCount; i++ )
					{
						if ( reader.GetName(i).ToLower() == "epassword" )
						{
							hasEPass = true;
							break;
						}
					}

					if ( reader.GetString(reader.GetOrdinal("AppTypeCode")).ToLower() == appTypeCode.ToLower() )
					{
						if ( reader.GetString(reader.GetOrdinal("ConnectionType")).ToLower() == "sqlauth" )
						{
							if ( hasEPass && !reader.IsDBNull(reader.GetOrdinal("EPassword")) )
							{
								string toDecrypt = reader.GetString(reader.GetOrdinal("EPassword"));
								toDecrypt = CMSecurity.DeRectifier(toDecrypt, enterpriseConnectionString);

								connectionString = GetGenericConnectionString(
									reader.GetString(reader.GetOrdinal("ServerName")),
									reader.GetString(reader.GetOrdinal("DatabaseName")),
									reader.GetString(reader.GetOrdinal("UserName")),
									toDecrypt);
							}
							else
								connectionString = GetGenericConnectionString(
									reader.GetString(reader.GetOrdinal("ServerName")),
									reader.GetString(reader.GetOrdinal("DatabaseName")),
									reader.GetString(reader.GetOrdinal("UserName")),
									reader.GetString(reader.GetOrdinal("UserPassword")));
							break;
						}
						else
						{
							connectionString = GetWinAuthConnectionString(
								reader.GetString(reader.GetOrdinal("ServerName")),
								reader.GetString(reader.GetOrdinal("DatabaseName")));
							break;
						}
					}
					else if ( reader.GetString(reader.GetOrdinal("AppTypeCode")).ToLower() == this.DefaultAppTypeCode.ToLower() )
					{
						if ( reader.GetString(reader.GetOrdinal("ConnectionType")).ToLower() == "sqlauth" )
						{
							if ( hasEPass && !reader.IsDBNull(reader.GetOrdinal("EPassword")) )
							{
								string toDecrypt = reader.GetString(reader.GetOrdinal("EPassword"));
								toDecrypt = CMSecurity.DeRectifier(toDecrypt, enterpriseConnectionString);

								defaultConnectionString = GetGenericConnectionString(
									reader.GetString(reader.GetOrdinal("ServerName")),
									reader.GetString(reader.GetOrdinal("DatabaseName")),
									reader.GetString(reader.GetOrdinal("UserName")),
									toDecrypt);
							}
							else
								defaultConnectionString = GetGenericConnectionString(
									reader.GetString(reader.GetOrdinal("ServerName")),
									reader.GetString(reader.GetOrdinal("DatabaseName")),
									reader.GetString(reader.GetOrdinal("UserName")),
									reader.GetString(reader.GetOrdinal("UserPassword")));
						}
						else
						{
							defaultConnectionString = GetWinAuthConnectionString(
								reader.GetString(reader.GetOrdinal("ServerName")),
								reader.GetString(reader.GetOrdinal("DatabaseName")));
						}
					}
				}
			}
			catch ( Exception ex )
			{
				throw new Exception("Failed read information from database. " + ex.Message);
			}
			finally
			{
				reader.Close();
				connection.Close();
			}

			if ( connectionString.Length == 0 && defaultConnectionString.Length == 0 )
			{
				throw new Exception(string.Format("Unable to get default or '{0}' connection information for {1}. There is no entry in ShowConnection table.", appTypeCode, showCode));
			}

			// return AppTypeCode connection string if available
			if ( connectionString.Length > 0 )
			{
				// add/update cached connection strings collection
				lock ( cachedConnectionStrings.SyncRoot )
				{
					if ( cachedConnectionStrings.ContainsKey(cachedConnectionStringKey) )
					{
						cachedConnectionStrings[cachedConnectionStringKey] = connectionString;
					}
					else
					{
						cachedConnectionStrings.Add(cachedConnectionStringKey, connectionString);
					}
				}
				return connectionString;
			}
			else
			{
				// add/update cached connection strings collection
				lock ( cachedConnectionStrings.SyncRoot )
				{
					if ( cachedConnectionStrings.ContainsKey(cachedConnectionStringKey) )
					{
						cachedConnectionStrings[cachedConnectionStringKey] = defaultConnectionString;
					}
					else
					{
						cachedConnectionStrings.Add(cachedConnectionStringKey, defaultConnectionString);
					}
				}
				return defaultConnectionString;
			}
		}
		#endregion

		/// <summary>
		/// Get show connection string in specific environment.
		/// </summary>
		/// <param name="showCode"></param>
		/// <param name="environment"></param>
		/// <param name="appTypeCode"></param>
		/// <returns></returns>
		public virtual string GetConnectionString(string showCode, ServerTypeCode environment, AppTypeCode appTypeCode)
		{
			// flush environment keys
			CMSecurity.Flush();

			// find environment master
			DataRow[] server = ConnectionManagerBase.ServerTable.Select("servertypecode = '" + environment.ToString() + "' and IsEnvironmentMaster = 1");
			if ( server.Length == 0 )
			{
				throw new Exception("Failed to retrieve environment server information.");
			}
			string entServerName = server[0]["ServerName"].ToString();

			// for caching purpose
			string cachedConnectionStringKey = string.Concat(entServerName, showCode, appTypeCode.ToString()).ToLower();

			if ( cachedConnectionStrings.ContainsKey(cachedConnectionStringKey) )
			{
				return cachedConnectionStrings[cachedConnectionStringKey].ToString();
			}

			return this.GetConnectionString(
				cachedConnectionStringKey,
				ConnectionManagerBase.GetGenericConnectionString(entServerName, this.EntDatabaseName),
				showCode,
				appTypeCode.ToString());
		}

		/// <summary>
		/// Get show connection string using server name and show code.
		/// </summary>
		/// <param name="showCode"></param>
		/// <param name="serverName"></param>
		/// <param name="appTypeCode"></param>
		/// <returns></returns>
		public virtual string GetConnectionString(string showCode, string serverName, AppTypeCode appTypeCode)
		{
			// for caching purpose
			string cachedConnectionStringKey = string.Concat(serverName, showCode, appTypeCode.ToString()).ToLower();

			if ( cachedConnectionStrings.ContainsKey(cachedConnectionStringKey) )
			{
				return cachedConnectionStrings[cachedConnectionStringKey].ToString();
			}

			return this.GetConnectionString(
				cachedConnectionStringKey,
				ConnectionManagerBase.GetGenericConnectionString(serverName, this.EntDatabaseName),
				showCode,
				appTypeCode.ToString());
		}
		#endregion

		#region | GetEntConnectionString |
		public virtual string GetEntConnectionString()
		{
			if ( cachedEntConnectionString.Length > 0 )
			{
				if ( _serverTable == null )    // only do this if _serverTable is null
					cachedEntConnectionString = cachedEntConnectionString; // reset server table
				return cachedEntConnectionString;
			}

			string serverName;
			string databaseName;

			try
			{
				serverName = this.GetEntServerName();
				databaseName = this.GetEntDatabaseName();
			}
			catch
			{
				throw new Exception("There is no default \"SqlServer\" and/or \"Database\" settings in application configuration file. Please either add the settings to application configuration file or pass enterprise connection string into this object.");
			}

			// JDM (2009.4.14): changed to use windows auth string
			cachedEntConnectionString = GetWinAuthConnectionString(serverName, databaseName);
			return cachedEntConnectionString;
		}

		public virtual string GetEntConnectionString(ServerTypeCode environment)
		{
			var filter = string.Format("[ServerTypeCode] = '{0}' AND [IsEnvironmentMaster] = 1", Enum.GetName(typeof(ServerTypeCode), environment));
			var server = ConnectionManagerBase.ServerTable.Select(filter).FirstOrDefault();
			if ( server == null
				|| !server.Table.Columns.Contains("ServerName")
				|| server.Table.Columns["ServerName"].DataType != typeof(string) )
			{
				throw new Exception("Failed to retrieve environment server information.");
			}
			var entServerName = (string)server["ServerName"];
			return ConnectionManagerBase.GetWinAuthConnectionString(entServerName, this.EntDatabaseName);
		}
		#endregion

		#region | FlushCache methods |

		public void FlushCache(string showCode)
		{
			FlushCacheStatic(showCode);
		}

		public static void FlushCacheStatic(string showCode)
		{
			lock ( cachedConnectionStrings.SyncRoot )
			{
				System.Collections.ArrayList toRemove = new ArrayList();
				foreach ( string key in cachedConnectionStrings.Keys )
				{
					if ( key.ToLower().IndexOf(showCode.ToLower()) != -1 )
					{
						toRemove.Add(key);
					}
				}

				foreach ( string rkey in toRemove )
				{
					cachedConnectionStrings.Remove(rkey);
				}
			}
		}

		public void FlushCache()
		{
			cachedEntConnectionString = "";
			lock ( cachedConnectionStrings.SyncRoot )
			{
				cachedConnectionStrings.Clear();
			}
		}

		#endregion

		#region | GetEntServerName |
		/// <summary>
		/// Get Enterpriser SQL Server name from config file using DefaultEntSqlServerAppSettingsKey
		/// property that can be changed programmically. Tip: this method can be overrided by a derived 
		/// class to return SQL Server name other than from config file.
		/// </summary>
		/// <returns>Enterprise SQL Server name</returns>
		protected virtual string GetEntServerName()
		{
			// 9/27/06 MLA - return ent server name from "passed" enterprise connection string if available.
			if ( this.passedEntConnectionString.Length > 0 )
			{
				try
				{
					int start = this.passedEntConnectionString.ToLower().IndexOf("server=") + 7;
					int end = this.passedEntConnectionString.ToLower().IndexOf(";", start);
					string server = this.passedEntConnectionString.Substring(start, end - start).Trim();
					if ( server == null || server.Length == 0 )
					{
						throw new Exception("Failed getting Enterprise server name from passed connection string");
					}
					else
					{
						return server;
					}
				}
				catch
				{
					throw new Exception("Failed getting Enterprise server name from passed connection string");
				}
			}
			else
			{
				// VS2005
				return System.Configuration.ConfigurationManager.AppSettings[DefaultEntSqlServerAppSettingsKey];
			}
		}
		#endregion

		#region | GetEntDatabaseName |
		/// <summary>
		/// Get Enterpriser database name from config file using DefaultEntDatabaseNameAppSettingsKey
		/// property that can be changed programmically. Tip: this method can be overrided by a derived 
		/// class to return database name other than from config file.
		/// </summary>
		/// <returns>Enterprise database name</returns>
		protected virtual string GetEntDatabaseName()
		{
			if ( this.passedEntConnectionString.Length > 0 )
			{
				try
				{
					int start = this.passedEntConnectionString.ToLower().IndexOf("database=") + 9;
					int end = this.passedEntConnectionString.ToLower().IndexOf(";", start);
					string database = this.passedEntConnectionString.Substring(start, end - start).Trim();
					if ( database == null || database.Length == 0 )
					{
						throw new Exception("Failed getting Enterprise database name from passed connection string");
					}
					else
					{
						return database;
					}
				}
				catch
				{
					throw new Exception("Failed getting Enterprise database name from passed connection string");
				}
			}
			else
			{
				// VS2005
				if ( System.Configuration.ConfigurationManager.AppSettings[DefaultEntDatabaseNameAppSettingsKey] == null )
				{
					// couldn't find enterprise database from configuration file, return default
					return "Enterprise";
				}
				else
				{
					// VS2005
					return System.Configuration.ConfigurationManager.AppSettings[DefaultEntDatabaseNameAppSettingsKey];
				}
			}
		}
		#endregion

		#region | GetGenericConnectionString |
		/// <summary>
		/// Return SQL connection string by using ServerName and DatabaseName and default ID and password.
		/// </summary>
		/// <param name="serverName"></param>
		/// <param name="databaseName"></param>
		/// <returns>SQL authentication connection string</returns>
		public static string GetGenericConnectionString(string serverName, string databaseName)
		{
			if ( serverName == null || serverName.Length == 0 || databaseName == null || databaseName.Length == 0 )
			{
				throw new Exception("\"serverName\" and \"databaseName\" can not be blank.");
			}

			// JDM (2009.4.14): changed to use windows auth; no more default sql accounts
			return GetWinAuthConnectionString(serverName, databaseName);
		}

		/// <summary>
		/// Return SQL connection string by using ServerName, DatabaseName, UserID, and Password.
		/// </summary>
		/// <param name="serverName"></param>
		/// <param name="databaseName"></param>
		/// <param name="userID"></param>
		/// <param name="userPassword"></param>
		/// <returns>SQL authentication connection string</returns>
		public static string GetGenericConnectionString(string serverName, string databaseName, string userID, string userPassword)
		{
			if ( serverName == null || serverName.Length == 0 || databaseName == null || databaseName.Length == 0 )
			{
				throw new Exception("\"serverName\" and \"databaseName\" can not be blank.");
			}

			return string.Format("SERVER={0};USER ID={1};PASSWORD={2};DATABASE={3};CONNECTION RESET=FALSE;Application Name={4};", serverName, userID, userPassword, databaseName, AppNameString);
		}

		#endregion

		#region | GetWinAuthConnectionString |
		/// <summary>
		/// Return Windows authenticaed SQL connection string by using ServerName and DatabaseName.
		/// </summary>
		/// <param name="serverName"></param>
		/// <param name="databaseName"></param>
		/// <returns>Windows authentication connection string</returns>
		public static string GetWinAuthConnectionString(string serverName, string databaseName)
		{
			if ( serverName == null || serverName.Length == 0 || databaseName == null || databaseName.Length == 0 )
			{
				throw new Exception("\"serverName\" and \"databaseName\" can not be blank.");
			}

			return string.Format("SERVER={0};DATABASE={1};Trusted_Connection=True;Application Name={2};", serverName, databaseName, AppNameString);
		}
		#endregion

		#region | GetServerTable | 
		private static object stLock = new object();
		private static void GetServerTable(string enterpriseConnectionString)
		{
			lock ( stLock )
			{
				DataTable prevTable = null;
				if ( _serverTable != null
					&& _serverTable.Rows.Count > 0 )
					prevTable = _serverTable.Copy();

				_serverTable = new DataTable();

				if ( enterpriseConnectionString.Length == 0 )
					return;

				// trip to enterprise database
				SqlConnection connection = new SqlConnection(enterpriseConnectionString);
				SqlCommand command = new SqlCommand("dbo.spServerSelectList", connection);
				command.CommandType = CommandType.StoredProcedure;
				SqlDataAdapter dAdapt = new SqlDataAdapter(command);

				try
				{
					connection.Open();
					dAdapt.Fill(_serverTable);
				}
				catch ( System.Exception err )
				{
					StringBuilder msg = new StringBuilder();
					msg.AppendFormat("Exception encountered in ConnectionManagerBase.GetServerTable: {0}     Stack Trace: {1}     enterpriseConnectionString: {2}", err.Message, err.StackTrace, enterpriseConnectionString);
					_lastServerTableException = msg.ToString();

					if ( prevTable != null )
						_serverTable = prevTable.Copy();
				}

				if ( connection.State == ConnectionState.Open )
					connection.Close();
			}
		}

		private static string _lastServerTableException = null;
		public static string LastServerTableException
		{
			get
			{
				return _lastServerTableException;
			}
		}
		#endregion

		#region | FlushServerTable |
		private static object flushServerTableLock = new object();
		/// <summary>
		/// For Disaster Recovery work, need a way to reset the Server table.
		/// This is because we directly change it and then operate from it.
		/// </summary>
		public static void FlushServerTable()
		{
			lock ( flushServerTableLock )
			{
				_serverTable = null;
			}
		}
		#endregion

		#region AppNameString

		private static string _appNameString = null;
		public static string AppNameString
		{
			get
			{
				if ( _appNameString == null || _appNameString == "detection_error" )
				{

					try
					{
						// windows  <-- this path works in web too;  test in deployment...
						if ( System.Windows.Forms.Application.ExecutablePath != null )
						{
							string tmp = Path.GetFileNameWithoutExtension(System.Windows.Forms.Application.ExecutablePath);

							switch ( tmp )
							{
								case "aspnet_wp":   // local web debug; webreg, weballocation, showmanager, etc...
								case "w3wp":        // deployed web app

									if ( System.Web.HttpContext.Current != null
										&& System.Web.HttpContext.Current.Request != null
										&& System.Web.HttpContext.Current.Request.ApplicationPath != null )
									{
										tmp = System.Web.HttpContext.Current.Request.ApplicationPath.Replace("/", ".");
										if ( tmp.StartsWith(".") )
										{
											tmp = tmp.Remove(0, 1);
										}
										_appNameString = tmp;

									}
									break;
								default:
									_appNameString = tmp;
									break;
							}
						}
						else
						{
							_appNameString = "detection_error";
						}
					}
					catch
					{
						_appNameString = "detection_error";
					}
				}

				return _appNameString;
			}
		}

		#endregion

		public virtual string GetShowVersion(string showCode)
		{
			string version = string.Empty;

			DataTable table = new DataTable();

			using ( SqlConnection connection = new SqlConnection(GetEntConnectionString()) )
			{
				using ( SqlCommand command = new SqlCommand("dbo.spShowSelectByCode", connection) )
				{
					command.CommandType = CommandType.StoredProcedure;
					command.Parameters.Add(new SqlParameter("@ShowCode", showCode));
					using ( SqlDataAdapter adapter = new SqlDataAdapter(command) )
					{
						try
						{
							adapter.Fill(table);
							if ( table.Rows.Count > 0 && table.Rows[0]["DatabaseVersion"] != null )
							{
								version = table.Rows[0]["DatabaseVersion"].ToString();
							}
							else
							{
								throw new Exception("Can't get show version!");
							}
						}
						catch ( Exception ex )
						{
							throw new Exception("Failed getting show version. " + ex.Message);
						}
						finally
						{
							connection.Close();
						}
					}
				}
			}

			return version;
		}
	}
}
