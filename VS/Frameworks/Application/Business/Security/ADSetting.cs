using System;

namespace App.Framework.Business.Security
{
	/// <summary>
	/// Summary description for ADSetting.
	/// </summary>
	public class ADSetting
	{
		private string _adPath ="" ; 
		private string _adUser = "" ; 
		private string _adUserFullyQualified;
		private string _adPassword = "" ; 
		private string _adServer = "" ; 

		public ADSetting()
		{
		}
		public string ADPath 
		{
			get{ return _adPath; }
			set{ _adPath = value; }
		} 
		/// <summary>
		/// User in the domain
		/// </summary>
		public string ADUser 
		{
			get{ return _adUser; }
			set{ _adUser = value; }
		} 
		/// <summary>
		/// The fully qualified named used for logging on the the appropriate server
		/// </summary>
		public string ADUserFullyQualified
		{
			get{ return _adUserFullyQualified; }
			set{ _adUserFullyQualified = value; }
		} 
		public string ADPassword 
		{
			get{ return _adPassword; }
			set{ _adPassword = value; }
		} 
		public string ADServer 
		{
			get{ return _adServer; }
			set{ _adServer = value; }
		} 
		
	}
}
