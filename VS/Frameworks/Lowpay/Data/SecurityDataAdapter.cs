using App.Framework.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lowpay.Framework.Data
{
    public class SecurityDataAdapter : DBADataAdapter
	{
		#region | Construction |
		public SecurityDataAdapter(string connectionString)
			: base(connectionString)
		{ }
		#endregion

		#region | LoadRolesAndPrivileges |
		public void LoadRolesAndPrivileges(out DataSet toFill, int userID)
		{
			toFill = new DataSet();

			//var Proc = new SqlServerStoredProcedure("spUserLoadRolesAndPrivileges");
			//Proc.Parameters.Add(base.CreateParameter("@UserID", userID));
			//base.Engine.ExecuteDataSet(Proc, toFill);
		}
		#endregion

		#region | LoadApplicationUsers |
		/// <summary>
		/// Load the CRM users
		/// </summary>
		/// <param name="toFill">The data table to fill</param>
		/// <param name="applicationCode">The applicaton code to load by</param>
		public void LoadApplicationUsers(out DataTable toFill, string applicationCode)
		{
			toFill = new DataTable("ApplicationUser");

			//// Define select
			//var QueryBuilder = new SelectSqlQueryBuilder();
			//QueryBuilder.ResultSet = new DbResultSet("vwApplicationUsers");

			//// Define where
			//QueryBuilder.ResultSet.SearchExpression &= new DbExpression(QueryBuilder.ResultSet.CreateColumn("ApplicationCode"), DbOperator.EqualTo, applicationCode);

			//// Define order
			//DbOrderColumn OrderCol = QueryBuilder.ResultSet.CreateOrderColumn("LastName");
			//QueryBuilder.ResultSet.OrderList.Add(OrderCol);
			//OrderCol = QueryBuilder.ResultSet.CreateOrderColumn("FirstName");
			//QueryBuilder.ResultSet.OrderList.Add(OrderCol);

			//// Run query
			//DbQuery Qry = (DbQuery)QueryBuilder.BuildQuery();
			//try
			//{
			//	base.Engine.ExecuteDataTable(Qry, toFill);
			//}
			//catch //( Exception ex )
			//{
			//	throw;
			//}
			//finally
			//{
			//	base.Engine.Dispose();
			//}
		}
		#endregion
	}
}
