using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Data.SqlClient;
using System.Data.Common;
using System.Reflection;
using System.Diagnostics;
using App.Framework.Business;

namespace Lowpay.Framework.Common
{
	/// <summary>
	/// This is the master configuration interpreter that reads config values from
	/// the appSettings element within the application config file, and encapsulates logic
	/// for default values when settings are not specified in the config file.
	/// </summary>
	public static class LocalConfig
	{
		#region AssemblyGet
		/// <summary>
		/// Helper to try to extract a value from an assembly attribute.  Assume that any
		/// Connections assembly should be equally able to provide the correct value, since
		/// most AssemblyInfo.cs data is in a single shared file.
		/// </summary>
		private static TResult AssemblyGet<TAttr, TResult>(Func<TAttr, TResult> getVal)
		{
			Assembly Assm = Assembly.GetEntryAssembly()
				?? Assembly.GetCallingAssembly()
				?? Assembly.GetExecutingAssembly();
			TResult Result = Assm.GetCustomAttributes(typeof(TAttr), true)
				.OfType<TAttr>()
				.Select(getVal)
				.FirstOrDefault();
			return Result;
		}
		#endregion

		#region ConfigGet
		private static Dictionary<string, string> ConfigCache = new Dictionary<string, string>();
		/// <summary>
		/// Get a config value from the appSettings section of application config
		/// file.  If not found, try the fallback function.  If not found and fallback
		/// not provided or returns null, throw an exception.
		/// </summary>
		private static string ConfigGet(string key, Func<string> fallback = null)
		{
			string Value = null;
			lock ( ConfigCache )
				ConfigCache.TryGetValue(key, out Value);
			if ( Value != null )
				return Value;

			lock ( ConfigurationManager.AppSettings )
			{
				if ( ConfigurationManager.AppSettings.Count < 1 )
				{
					ConfigurationManager.RefreshSection("appSettings");
					ConfigurationManager.GetSection("appSettings");
				}
				if ( ConfigurationManager.AppSettings.Count < 1 )
					throw new Exception("Missing required appSettings configuration section.");
				Value = ConfigurationManager.AppSettings[key];
			}

			if ( Value == null )
			{
				string ExcPref = "Configuration missing appSetting key \"" + key + "\", ";
				if ( fallback == null )
					throw new Exception(ExcPref + "no fallback specified.");

				try { Value = fallback.Invoke(); }
				catch ( Exception ex ) { throw new Exception(ExcPref + "fallback threw exception: " + ex.Message, ex); }
				if ( Value == null )
					throw new Exception(ExcPref + "fallback returned null.");
			}

			lock ( ConfigCache )
				ConfigCache[key] = Value;
			return Value;
		}
		#endregion

		#region ProductVersion
		/// <summary>
		/// Current version of the product from assembly metadata.  This will automatically be set to vM.N.C.B by the
		/// TFS neutral build, where M.N is the manually-configured branch ID, C is the changeset number,
		/// and B is the numerical part of the build ID.  This value is only meaningful on neutral builds.
		/// </summary>
		public static string ProductVersion
		{
			get
			{
				string Version = AssemblyGet<AssemblyFileVersionAttribute, string>(A => A.Version)
					?? AssemblyGet<AssemblyVersionAttribute, string>(A => A.Version)
					?? "0.0.0.0";
				return "v" + Version;
			}
		}
		#endregion

		#region ProductName
		/// <summary>
		/// Get a statically-defined name for the product.  Note that this does NOT honor the database
		/// configsettings for branding, so should only be used where those config settings are inaccessible.
		/// </summary>
		public static string ProductName { get { return ConfigGet("ProductName", () => "EXS"); } }
		#endregion

		#region CopyrightNotice
		/// <summary>
		/// Get the copyright notice for the product.  Fall back on the value compiled into
		/// the assemblies, then finally to a hard-coded value.
		/// </summary>
		public static string CopyrightNotice
		{
			get
			{
				return ConfigGet("CopyrightNotice", () =>
					AssemblyGet<AssemblyCopyrightAttribute, string>(A => A.Copyright)
						?? ("Copyright ©2006-" + DateTime.Now.Year.ToString() + " Experient, Inc."));
			}
		}
		#endregion

		#region UnifiedDeployment_Environment
		/// <summary>
		/// A code representing the type of the environment within which this app is running.
		/// </summary>
		public static string UnifiedDeployment_Environment { get { return ConfigGet("UnifiedDeployment_Environment"); } }
		#endregion

		#region UnifiedDeployment_Application
		/// <summary>
		/// A code representing the type of the application that is running.
		/// </summary>
		public static string UnifiedDeployment_Application { get { return ConfigGet("UnifiedDeployment_Application"); } }
		#endregion

		#region LogDatabaseLevel
		/// <summary>
		/// Get the minimum log severity level for writing entries to the log database.
		/// </summary>
		public static SourceLevels LogDatabaseLevel
		{
			get
			{
				const SourceLevels DEFAULT = SourceLevels.All;
				return ConfigGet("LogDatabaseLevel", () =>
				{
					switch ( UnifiedDeployment_Environment.Trim().ToUpper() )
					{
						case "PROD":
							return SourceLevels.Information.ToString();
						default:
							return DEFAULT.ToString();
					}
				}).EnumTryParse<SourceLevels>(DEFAULT);
			}
		}
		#endregion

		#region LogEmailLevel
		/// <summary>
		/// Get the minimum log severity level for sending email alerts.
		/// </summary>
		public static SourceLevels LogEmailLevel
		{
			get
			{
				const SourceLevels DEFAULT = SourceLevels.Critical;
				return ConfigGet("LogEmailLevel", () =>
				{
					switch ( UnifiedDeployment_Environment.Trim().ToUpper() )
					{
						case "LOCALDEV":
							return SourceLevels.Off.ToString();
						case "PROD":
							return SourceLevels.Error.ToString();
						default:
							return DEFAULT.ToString();
					}
				}).EnumTryParse<SourceLevels>(DEFAULT);
			}
		}
		#endregion

		#region LogEmailTo
		/// <summary>
		/// Get the address to which to send log email alerts.
		/// </summary>
		public static string LogEmailTo { get { return ConfigGet("LogEmailTo", () => "SWAPEmailAlertGroup@experient-inc.com"); } }
		#endregion

		#region LogEmailFrom
		/// <summary>
		/// Get the address from which to send log email alerts.
		/// </summary>
		public static string LogEmailFrom { get { return ConfigGet("LogEmailFrom", () => "donotreply@experient-inc.com"); } }
		#endregion

		#region LogEmailSmtp
		/// <summary>
		/// Get the SMTP server to use when sending log email alerts.
		/// </summary>
		public static string LogEmailSmtp { get { return ConfigGet("LogEmailSmtp", () => "smtp3.experientevent.com"); } }
		#endregion

		#region DatabaseNameLog
		/// <summary>
		/// The database name of the Log database.
		/// </summary>
		public static string DatabaseNameLog
		{
			get
			{
				return ConfigGet("DatabaseNameLog", () => "ConnectionsLog");
			}
		}
		#endregion

		#region SqlServer
		/// <summary>
		/// The shared SQL server for both Main and Log connections, if a
		/// more specific one is not specified.
		/// </summary>
		public static string SqlServer
		{
			get
			{
				return ConfigGet("SqlServer", () =>
					{
						switch ( UnifiedDeployment_Environment.Trim().ToUpper() )
						{
							case "LOCALDEV":
							case "DEV":
								return "FRQASWAPSQL01.EXPOEXCHANGE.COM\\DEVSQL";
							case "LT":
								return "frltsql01.expoexchange.com\\SQL2008";
							case "QA":
								return "FRQASWAPSQL01.EXPOEXCHANGE.COM\\QASQL";
							case "MAINT":
								return "FRQASWAPSQL01.EXPOEXCHANGE.COM";
							case "PROD":
								return "FRSWAPSQL01.expoexchange.com";
							default:
								throw new Exception("Unrecognized UnifiedDeployment_Environment \"" + UnifiedDeployment_Environment + "\".");
						}
					});
			}
		}
		#endregion

		#region SqlServerLog
		/// <summary>
		/// Override the shared SQL server name for the Log database.
		/// </summary>
		public static string SqlServerLog { get { return ConfigGet("SqlServerLog", () => SqlServer); } }
		#endregion

		#region ProviderConnString
		/// <summary>
		/// Inner provider connection string skeleton shared by both Main and Log connections.
		/// </summary>
		public static string ProviderConnString { get { return ConfigGet("ProviderConnString", () => "Integrated Security=True;MultipleActiveResultSets=True"); } }
		#endregion

		#region ProviderConnStringLog
		/// <summary>
		/// Override the inner provider connection string for the Log connection.
		/// </summary>
		public static string ProviderConnStringLog { get { return ConfigGet("ProviderConnStringLog", () => ProviderConnString); } }
		#endregion

		#region BuildSQLConnString
		/// <summary>
		/// Build the inner SQL provider connection string
		/// </summary>
		private static string BuildSQLConnString(string connString, string serverName, string databaseName)
		{
			SqlConnectionStringBuilder Inner = new SqlConnectionStringBuilder();
			Inner.ConnectionString = connString;
			Inner.DataSource = serverName;
			Inner.InitialCatalog = databaseName;
			Inner.ApplicationName = string.Join(" ", new string[] { ProductName, UnifiedDeployment_Environment, UnifiedDeployment_Application }
				.Where(X => !string.IsNullOrWhiteSpace(X)));
			return Inner.ConnectionString;
		}
		#endregion

		#region SQLConnectionStringLog
		/// <summary>
		/// SQL EF connection string for the Log database entity connection.
		/// </summary>
		public static string SQLConnectionStringLog
		{
			get
			{
				return ConfigGet("SQLConnectionStringLog", () =>
					BuildSQLConnString(ProviderConnStringLog, SqlServerLog, DatabaseNameLog));
			}
		}
		#endregion

		#region BuildEntityConnString
		/// <summary>
		/// Build an EF connection string from the given selected parameters.
		/// </summary>
		private static string BuildEntityConnString(string metaRes, string sqlConnString)
		{
			DbConnectionStringBuilder Outer = new DbConnectionStringBuilder();
			Outer["metadata"] = string.Join("|", new string[] { "ssdl", "csdl", "msl" }.Select(S => metaRes + S));
			Outer["provider"] = "System.Data.SqlClient";
			Outer["provider connection string"] = sqlConnString;

			return Outer.ConnectionString;
		}
		#endregion

		#region EntityMetadataPrefixLog
		/// <summary>
		/// Prefix for Entity model location for Main database connection.
		/// </summary>
		public static string EntityMetadataPrefixLog
		{
			get
			{
				return ConfigGet("EntityMetadataPrefixLog",
					() => "res://ExpoExchange.EXS.Logging.Model/Log.");
			}
		}
		#endregion

		#region EntityConnectionStringLog
		/// <summary>
		/// Entity EF connection string for the Log database entity connection.
		/// </summary>
		public static string EntityConnectionStringLog
		{
			get
			{
				return ConfigGet("EntityConnectionStringLog", () => BuildEntityConnString(EntityMetadataPrefixLog, SQLConnectionStringLog));
			}
		}
		#endregion
	}
}
