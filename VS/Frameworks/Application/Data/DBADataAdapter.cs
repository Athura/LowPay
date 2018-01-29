using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Framework.Data
{
	/// <summary>
	/// Summary description for DBADataAdapter.
	/// </summary>
	public class DBADataAdapter
	{
		private string _ConnectionString = "";

		#region | Construction |
		/// <summary>
		/// Instantiate the DataAdapter using the connection string
		/// </summary>
		/// <param name="connectionString">Defines the Database to connect to</param>
		public DBADataAdapter(string connectionString)
		{
			_ConnectionString = connectionString;
		}
		#endregion
	}
}
