using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using App.Framework.Business;
using Lowpay.Framework.Common;

namespace Lowpay.Framework.Business
{
	public class ConnectionManager
	{
		#region | Declarations |

		private const string EnterpriseDatabaseName = "Enterprise";

		private static bool _UseWindowsAuthentication = false;
		private static string _ServerName = "";
		private static string _DatabaseName = "";
		private static string _RegServerName = "";
		//private static string _ShowServerName = "";
		//private static string _ShowDatabaseName = "";

		#endregion

		#region | Construction |
		static ConnectionManager()
		{
			//JEC 11-11/2008 : just adding this static constructor fixed a problem where we were seeing the CRMAppConfig.SettingsCollection
			// being used above (in the static members) before it was initialized.  It only happened in 'release' mode.  I think it
			// had to do with optimization that is turned on in release mode that isn't on in debug mode.  I think it changed the
			// timing for some of the events happing when the app starts up.

			_UseWindowsAuthentication = CRMConfig.GetConfigBoolean("UseWindowsAuthentication", true);

			_ServerName = CRMConfig.GetConfigString("DatabaseServer");
			if ( string.IsNullOrWhiteSpace(_ServerName) )
				_ServerName = CRMConfig.GetConfigString("ExsEnterpriseDBServer");

			_DatabaseName = CRMConfig.GetConfigString("DatabaseName");
			if ( string.IsNullOrWhiteSpace(_DatabaseName) )
				_DatabaseName = CRMConfig.GetConfigString("ExsEnterpriseDBName");

			_RegServerName = CRMConfig.GetConfigString("RegShowDBServer");
			if ( string.IsNullOrWhiteSpace(_RegServerName) )
				_RegServerName = _ServerName;
		}
		#endregion

		#region | Properties |

		#region | ServerName |
		public static string ServerName
		{
			get { return _ServerName; }
			set { _ServerName = value.Trim(); }
		}
		#endregion

		#region | DatabaseName |
		public static string DatabaseName
		{
			get { return _DatabaseName; }
			set { _DatabaseName = value.Trim(); }
		}
		#endregion

		#region | RegistrationServerName |
		public static string RegistrationServerName
		{
			get { return _RegServerName; }
			set { _RegServerName = value.Trim(); }
		}
		#endregion

		#endregion

		#region | GetConnectionString |
		/// <summary>
		/// Get database connection.
		/// </summary>
		/// <returns>Connection string</returns>
		public static string GetConnectionString()
		{
			return ConnectionManager.GetConnectionString(_ServerName, _DatabaseName);
		}
		/// <summary>
		/// Get database connection for the show, Cached.
		/// </summary>
		/// <param name="showCode">The Show Code to get connection for</param>
		/// <returns>Connection string for a show</returns>
		public static string GetConnectionString(string showCode)
		{
			return ConnectionManager.GetConnectionString(showCode, true);
		}
		/// <summary>
		/// Get database connection for the show.
		/// </summary>
		/// <param name="showCode">The Show Code to get connection for</param>
		/// <param name="useCached">False will always reload show connection, true will use previously cached connection</param>
		/// <returns>Connection string for a show</returns>
		public static string GetConnectionString(string showCode, bool useCached)
		{
			ConnectionManagerBase MgrBase = new ConnectionManagerBase(GetRegEntConnectionString());
			if ( !useCached )
				return MgrBase.GetConnectionStringFromDatabase(showCode);
			return MgrBase.GetConnectionString(showCode);
		}
		/// <summary>
		/// Get database connection.
		/// </summary>
		public static string GetConnectionString(string serverName, string databaseName)
		{
			if ( _UseWindowsAuthentication )
			{
				return ConnectionManagerBase.GetWinAuthConnectionString(serverName, databaseName);
			}
			else
			{
				return ConnectionManagerBase.GetGenericConnectionString(serverName, databaseName);
			}
		}
		#endregion

		#region | GetRegEntConnectionString |
		public static string GetRegEntConnectionString()
		{
			return ConnectionManager.GetConnectionString(_RegServerName, EnterpriseDatabaseName);
		}
		#endregion

		#region | GetEnvironmentMasterConnectionString |
		public static string GetEnvironmentMasterConnectionString()
		{
			string ConnString = "";
			DataTable dt = ConnectionManagerBase.ServerTable;
			DataRow[] Rows = dt.Select("ServerTypeCode = '" + CRMConfig.EnvironmentMode + "' AND IsEnvironmentMaster = 1");
			if ( Rows.Length == 1 )
			{
				ConnString = ConnectionManagerBase.GetGenericConnectionString(Rows[0]["ServerName"].ToString(), EnterpriseDatabaseName);
			}
			return ConnString;
		}
		#endregion

		#region | SetServerAndDatabase |
		/// <summary>
		/// Method to set these public properties.
		/// </summary>
		/// <remarks>
		/// JEC: I had to do this to allow me to set these properties from the ExsWindowsService app.
		/// For some reason it would not let me do it using the public fields above.
		/// I would get a null reference error and could not figure out why.
		/// </remarks>
		/// <param name="serverName">the server name to set</param>
		/// <param name="databaseName">the database name to set</param>
		public static void SetServerAndDatabase(string serverName, string databaseName)
		{
			_ServerName = serverName;
			_RegServerName = serverName;
			_DatabaseName = databaseName;
		}
		#endregion
	}
}
