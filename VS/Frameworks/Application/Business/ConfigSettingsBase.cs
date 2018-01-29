using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using System.Data;

namespace App.Framework.Business
{
	/// <summary>
	/// Summary description for ConfigSettingsBase.
	/// </summary>
	public class ConfigSettingsBase
	{
		// The application configuration settings from web.config are stored in this collection
		public static NameValueCollection SettingsCollectionBase;
		private static HybridDictionary _InternalSettings;

		public virtual void LoadFileSettings()
		{
			_InternalSettings = new HybridDictionary();

			// Read vals from CONFIG File
			// VS2005
			NameValueCollection ConfigFileItems = System.Configuration.ConfigurationManager.AppSettings;

			lock ( _InternalSettings.SyncRoot )
			{
				foreach ( string keyName in ConfigFileItems.AllKeys )
				{
					// DSH - 03.15.2005
					// Somehow, the value in the keyname of SqlServer and Database is being 
					// doubled up.  So, we are going to explicitly set the key.
					// This either creates the key/value pairing, or replaces an existing 
					// key’s value with the new value.
					_InternalSettings.Remove(keyName);
					_InternalSettings.Add(keyName, ConfigFileItems[keyName]);
					//SettingsCollectionBase[keyName] = ConfigFileItems[keyName];
				}

				NameValueCollection TempSettingsCollection = new NameValueCollection();

				foreach ( string key in _InternalSettings.Keys )
				{
					TempSettingsCollection[key] = _InternalSettings[key].ToString();
				}

				if ( SettingsCollectionBase == null )
					SettingsCollectionBase = new NameValueCollection();

				lock ( SettingsCollectionBase )
				{
					SettingsCollectionBase = TempSettingsCollection;
				}
			}
		}

		//		public void LoadDatabaseSettings(NameValueCollection ConfigDatabaseItems)
		//		{
		//			foreach(string keyName in ConfigDatabaseItems.AllKeys)
		//				SettingsCollectionBase[keyName] = ConfigDatabaseItems[keyName];
		//		}

		public void LoadDatabaseSettings(DataTable table)
		{

			lock ( _InternalSettings.SyncRoot )
			{
				foreach ( DataRow row in table.Rows )
				{
					string key = row["KeyName"].ToString();
					string item = row["ItemValue"].ToString();

					//SettingsCollectionBase[key] = item;

					_InternalSettings.Remove(key);
					_InternalSettings.Add(key, item);
				}

				foreach ( string keyName in _InternalSettings.Keys )
				{
					SettingsCollectionBase[keyName] = _InternalSettings[keyName].ToString();
				}
			}
		}

		#region LoadSettingsCollection

		private static HybridDictionary _loadSettings;

		public virtual NameValueCollection LoadSettingsCollection(DataTable dbSettings)
		{
			_loadSettings = new HybridDictionary();

			// Read vals from CONFIG File
			// VS2005
			NameValueCollection ConfigFileItems = System.Configuration.ConfigurationManager.AppSettings;

			lock ( _loadSettings.SyncRoot )
			{
				NameValueCollection TempSettingsCollection = new NameValueCollection();

				// file settings
				foreach ( string keyName in ConfigFileItems.AllKeys )
				{
					_loadSettings.Remove(keyName);
					_loadSettings.Add(keyName, ConfigFileItems[keyName]);
				}

				// database settings
				foreach ( DataRow row in dbSettings.Rows )
				{
					string key = row["KeyName"].ToString();
					string item = row["ItemValue"].ToString();

					_loadSettings.Remove(key);
					_loadSettings.Add(key, item);
				}

				foreach ( string keyName in _loadSettings.Keys )
				{
					TempSettingsCollection[keyName] = _loadSettings[keyName].ToString();
					SettingsCollectionBase[keyName] = _loadSettings[keyName].ToString();
				}

				return TempSettingsCollection;
			}
		}

		#endregion
	}
}
