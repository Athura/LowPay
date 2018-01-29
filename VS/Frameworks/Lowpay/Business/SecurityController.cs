using App.Framework.Business;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lowpay.Framework.Common;
using App.Framework.Business.Security;
using Lowpay.Framework.Data;

namespace Lowpay.Framework.Business
{
	/// <summary>
	/// Hook into the BusinessPrincipalController methods for authentication of users.
	/// </summary>
	public class SecurityController
	{
		#region | Declarations |

		private string _ConnectionString = "";

		#endregion

		#region | Construction |
		public SecurityController()
		{
			this._ConnectionString = ConnectionManager.GetConnectionString();
		}
		#endregion

		#region | Logon |
		/// <summary>
		/// Leveraging the base components in BusinessPrincipalController, logon the 
		/// current user and fill the User info (ie, name and email).
		/// </summary>
		/// <param name="user">EXSUser with UserName not null</param>
		public void Logon(ref CRMUser user)
		{
			// Nothing working, out aha here...
			if ( user.UserName.IsEmpty() )
			{
				throw new Exception("Invalid UserName, must be specified to login.");
			}

			var Identity = new UserIdentity() { UserName = user.UserName };
			var Ctrl = new BusinessPrincipalController(this._ConnectionString);
			Ctrl.LoginFull(ref Identity);
			if ( Identity == null )
			{
				throw new Exception(string.Format("Invalid User, '{0}' not found in Enterprise database on {1}.", user.UserName, ConnectionManager.RegistrationServerName));
			}

			user.UserID = Identity.UserID;
			user.UserName = Identity.UserName;
			user.Lastname = Identity.Lastname;
			user.Firstname = Identity.Firstname;
			user.Email = Identity.Email;
			user.Roles = Identity.Roles;

			this.LoadRolesAndPrivileges(user);
		}
		#endregion

		#region | LogonWithApplications |
		/// <summary>
		/// Leveraging the base components in BusinessPrincipalController, logon the 
		/// current user and fill the User info (ie, name and email).
		/// </summary>
		/// <param name="user">EXSUser with UserName not null</param>
		public void LogonWithApplications(ref CRMUser user)
		{
			this.Logon(ref user);
		}
		#endregion

		#region | LoadRolesAndPrivileges |
		private ExecutionMessage[] LoadRolesAndPrivileges(CRMUser user)
		{
			var Msgs = new ExecutionMessageCollection();
			try
			{
				DataSet Roles = null;
				new SecurityDataAdapter(_ConnectionString)
					.LoadRolesAndPrivileges(out Roles, user.UserID);

				// Load RoleNames
				user.RoleList = new System.Collections.ArrayList();
				foreach ( DataRow row in Roles.Tables[0].Rows )
					user.RoleList.Add(row["RoleName"].ToString());

				// Load Privilege Names
				user.PrivilegeList = new System.Collections.ArrayList();
				foreach ( DataRow row in Roles.Tables[1].Rows )
					user.PrivilegeList.Add(row["PrivilegeName"].ToString());
			}
			catch ( Exception ex )
			{
				Msgs.Add(new ErrorMessage(ex.FormatMessage("LoadApplicationUsers")));
			}
			return Msgs.ToArray();
		}
		#endregion

		#region | LoadApplicationUsers |
		/// <summary>
		/// Load the CRM users
		/// </summary>
		/// <param name="tableToFill">The data table to fill</param>
		/// <param name="applicationCode">The applicaton code to load by</param>
		public ExecutionMessage[] LoadApplicationUsers(out DataTable toFill, string applicationCode)
		{
			var Msgs = new ExecutionMessageCollection();
			toFill = null;
			try
			{
				new SecurityDataAdapter(this._ConnectionString)
					.LoadApplicationUsers(out toFill, applicationCode);
			}
			catch ( Exception ex )
			{
				Msgs.Add(new ErrorMessage(ex.FormatMessage("LoadApplicationUsers")));
			}
			return Msgs.ToArray();
		}
		#endregion
	}
}
