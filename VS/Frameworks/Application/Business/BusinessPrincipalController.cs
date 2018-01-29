using App.Framework.Business.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Framework.Business
{
	public class BusinessPrincipalController
	{
		private string _ConnectionString = "";

		#region | Construction |
		/// <summary>
		/// Instantiate the DataAdapter using the connection string
		/// </summary>
		/// <param name="connectionString">Defines the Database to connect to</param>
		public BusinessPrincipalController(string connectionString)
		{
			_ConnectionString = connectionString;
		}
		#endregion

		public void LoginFull(ref UserIdentity identity)
		{

		}

		public UserIdentity GetUserIdentity(string joinedName)
		{
			return new UserIdentity();
		}
	}
}
