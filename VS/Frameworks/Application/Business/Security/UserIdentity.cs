using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Framework.Business.Security
{
	[Serializable]
	public class UserIdentity
	{
		#region | Constructors |
		public UserIdentity()
		{
		}
		#endregion

		#region | Private Members |
		private int _UserID;
		private string _UserName = string.Empty;
		private string _Firstname = string.Empty;
		private string _Lastname = string.Empty;
		private string _Email = string.Empty;
		private string _UserType = string.Empty;
		ArrayList _roles = new ArrayList();
		#endregion

		#region | Public Properties |
		public int UserID
		{
			get
			{
				return this._UserID;
			}
			set
			{
				this._UserID = value;
			}
		}

		public string UserName
		{
			get
			{
				return this._UserName;
			}
			set
			{
				this._UserName = value;
			}
		}

		public string Firstname
		{
			get
			{
				return this._Firstname;
			}
			set
			{
				this._Firstname = value;
			}
		}

		public string Lastname
		{
			get
			{
				return this._Lastname;
			}
			set
			{
				this._Lastname = value;
			}
		}

		public string Email
		{
			get
			{
				return this._Email;
			}
			set
			{
				this._Email = value;
			}
		}

		public string UserType
		{
			get
			{
				return this._UserType;
			}
			set
			{
				this._UserType = value;
			}
		}

		public ArrayList Roles
		{
			get
			{
				return _roles;
			}
			set
			{
				_roles = value;
			}
		}


		#endregion
	}
}
