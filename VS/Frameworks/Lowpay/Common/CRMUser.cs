using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using App.Framework.Business;
using App.Framework.Business.Security;

namespace Lowpay.Framework.Common
{
	public class CRMUser : UserIdentity
	{
		#region | Construction |
		public CRMUser() : base()
		{
			Phone = string.Empty;
		}
		#endregion

		#region | PrivilegeList |
		/// <summary>
		/// Holds the list of Privilege Names (strings) for the current user
		/// </summary>
		private ArrayList InnerPrivilegeList = new ArrayList();
		public ArrayList PrivilegeList
		{
			get { return InnerPrivilegeList; }
			set { InnerPrivilegeList = value; }
		}
		#endregion

		#region | RoleList |
		/// <summary>
		/// Holds the list of Role Names (strings) for the current user
		/// </summary>
		private ArrayList InnerRoleList = new ArrayList();
		public ArrayList RoleList
		{
			get { return InnerRoleList; }
			set { InnerRoleList = value; }
		}
		#endregion

		#region | FirstLastName |
		public string FirstLastName
		{
			get
			{
				if ( this.Firstname.IsEmpty() && this.Lastname.IsEmpty() )
					return this.UserName;
				else if ( this.Firstname.IsEmpty() )
					return this.Lastname;
				else
					return (this.Firstname + " " + this.Lastname).Trim();
			}
		}
		#endregion

		#region | LastFirstName |
		public string LastFirstName
		{
			get
			{
				if ( this.Firstname.IsEmpty() && this.Lastname.IsEmpty() )
					return this.UserName;
				else if ( this.Firstname.IsEmpty() )
					return this.Lastname;
				else
					return (this.Lastname + ", " + this.Firstname).Trim();
			}
		}
		#endregion

		#region | KeyerInitials |
		public string KeyerInitials
		{
			get
			{
				string Keyer = this.UserName;
				int IndexPos = Keyer.IndexOf(@"\");
				if ( IndexPos > -1 )
					Keyer = Keyer.Substring(IndexPos + 1);

				return Keyer;
			}
		}
		#endregion

		#region | UserNameNoDomain |
		public string UserNameNoDomain
		{
			get
			{
				return this.KeyerInitials;
			}
		}
		#endregion

		#region | Phone |
		public string Phone { get; set; }
		#endregion

		#region | HasPrivilege |
		public bool HasPrivilege(string privilegeName)
		{
			int i = this.PrivilegeList.IndexOf(privilegeName);
			return (i > -1);
		}
		#endregion

		#region | PrimaryRoleLevel |
		/// <summary>
		/// Holds the list of Role Names (strings) for the current user
		/// </summary>
		private int InnerPrimaryRoleLevel = -1;
		public int PrimaryRoleLevel
		{
			get { return InnerPrimaryRoleLevel; }
		}
		#endregion

		#region | PrimaryRoleName |
		private string InnerPrimaryRoleName = "";
		/// <summary>
		/// Returns the primary role in the case of multiple roles
		/// </summary>
		/// <remarks>
		/// Each user can have several EXS Roles.  It is important to assign a "hierarchy" of
		/// roles so that the "Main" or "Highest privilege" role is set as primary.  This is
		/// done very 
		/// </remarks>
		public string PrimaryRoleName
		{
			get
			{
				// Only go here one time.
				if ( InnerPrimaryRoleName.Length == 0 )
				{
					InnerPrimaryRoleLevel = -1;
					int RoleLevel = 0;

					foreach ( string RoleName in this.RoleList )
					{
						if ( RoleName.Equals("System Administrator", StringComparison.CurrentCultureIgnoreCase) )
							RoleLevel = 10;
						else if ( RoleName.Equals("EXS Management", StringComparison.CurrentCultureIgnoreCase) )
							RoleLevel = 5;
						else if ( RoleName.StartsWith("ACRM Level1 Access", StringComparison.CurrentCultureIgnoreCase) )
							RoleLevel = 9;
						else if ( RoleName.StartsWith("ACRM Level2 Access", StringComparison.CurrentCultureIgnoreCase) )
							RoleLevel = 8;
						else if ( RoleName.StartsWith("ACRM Level3 Access", StringComparison.CurrentCultureIgnoreCase) )
							RoleLevel = 7;
						else if ( RoleName.StartsWith("ACRM Level4 Access", StringComparison.CurrentCultureIgnoreCase) )
							RoleLevel = 6;
						else if ( RoleName.StartsWith("ACRM Level5 Access", StringComparison.CurrentCultureIgnoreCase) )
							RoleLevel = 4;
						else
							RoleLevel = 0;

						if ( RoleLevel > InnerPrimaryRoleLevel )
						{
							InnerPrimaryRoleLevel = RoleLevel;
							InnerPrimaryRoleName = RoleName;
						}
					}
				}

				return InnerPrimaryRoleName;
			}
		}
		#endregion

		#region | Itemized Permissions |

		public bool CanEditFieldSet1
		{
			get { return this.HasPrivilege("prvACRMCanEditFieldSet1"); }
		}
		public bool CanEditFieldSet2
		{
			get { return this.HasPrivilege("prvACRMCanEditFieldSet2"); }
		}
		public bool CanEditFieldSet3
		{
			get { return this.HasPrivilege("prvACRMCanEditFieldSet3"); }
		}
		public bool CanEditFieldSet4
		{
			get { return this.HasPrivilege("prvACRMCanEditFieldSet4"); }
		}
		public bool CanManageCommissions
		{
			get { return this.HasPrivilege("prvExsManageCommissions"); }
		}
		public bool CanManagePublicLists
		{
			get { return this.HasPrivilege("prvExsConfigurePublicLists"); }
		}
		public bool CanEmailBlast
		{
			get { return this.HasPrivilege("prvExsExecuteEmailBlast"); }
		}
		public bool CanPrintConfirmations
		{
			get { return this.HasPrivilege("prvExsPrintConfirmations"); }
		}
		public bool CanSettlePayments
		{
			get { return this.HasPrivilege("prvExsSettlePayments"); }
		}
		public bool CanSearchCampaignStrategies
		{
			get { return this.HasPrivilege("prvExsReadEmailBlast"); }
		}
		public bool CanImportExhibitors
		{
			get
			{
				if ( this.HasPrivilege("prvExsImportExhibitors") )
					return true;

				// This is temporary until a permission can be defined.
				switch ( this.UserNameNoDomain.ToLower() )
				{
					case "johnsonp":
					case "mooreja":
					case "crouchj":
						return true;
					default:
						return false;
				}
			}
		}
		public bool CanAccessCustomFieldManager
		{
			get { return this.HasPrivilege("prvACRMAccessCustomFieldManager"); }
		}
		public bool CanAccessSurveyBuilder
		{
			get { return this.HasPrivilege("prvACRMAccessSurveyBuilder"); }
		}
		public bool CanAccessCampaignManagement
		{
			get { return this.HasPrivilege("prvACRMAccessCampaignManagement"); }
		}
		public bool CanAccessQueryBuilder
		{
			get { return this.HasPrivilege("prvACRMAccessQueryBuilder"); }
		}
		public bool CanAccessReportingFull
		{
			get { return this.HasPrivilege("prvACRMAccessReportingFull"); }
		}
		public bool CanAccessReportingLimited
		{
			get { return this.HasPrivilege("prvACRMAccessReportingLimited"); }
		}
		public bool CanAccessImportList
		{
			get { return this.HasPrivilege("prvACRMAccessImportList"); }
		}
		public bool CanExportReports
		{
			get { return this.HasPrivilege("prvACRMExportReports"); }
		}
		public bool CanMergeCompanies
		{
			get { return this.HasPrivilege("prvACRMMergeCompanies"); }
		}
		public bool CanMergeContacts
		{
			get { return this.HasPrivilege("prvACRMMergeContacts"); }
		}
		public bool CanTransferContacts
		{
			get { return this.HasPrivilege("prvACRMTransferContacts"); }
		}
		public bool CanEditTripNotes
		{
			get { return this.HasPrivilege("prvACRMEditTripNotes"); }
		}
		public bool CanEditVIPContact
		{
			get { return this.HasPrivilege("prvACRMEditVIPContact"); }
		}
		public bool CanEditRegStatus
		{
			get { return this.HasPrivilege("prvACRMEditRegStatus"); }
		}
		public bool CanExportTargetList
		{
			get { return this.HasPrivilege("prvACRMExportTargetList"); }
		}
		public bool CanRegisterContact
		{
			get { return this.HasPrivilege("prvACRMRegisterContact"); }
		}
		public bool CanEditRestrictedCompanies
		{
			get { return this.HasPrivilege("prvACRMEditRestrictedCompanies"); }
		}
		public bool CanEditRestrictedContacts
		{
			get { return this.HasPrivilege("prvACRMEditRestrictedContacts"); }
		}
		public bool CanDeleteCompany
		{
			get { return this.HasPrivilege("prvACRMDeleteCompany"); }
		}
		public bool CanDeleteContact
		{
			get { return this.HasPrivilege("prvACRMDeleteContact"); }
		}
		public bool CanInactivateCompany
		{
			get { return (this.PrimaryRoleLevel >= 7); }
		}
		public bool CanInactivateContact
		{
			get { return (this.PrimaryRoleLevel >= 7); }
		}
		public bool CanEditShowContact
		{
			get
			{
				if ( this.HasPrivilege("prvACRMCanEditShowContact") )
					return true;

				// PDJ 7/1/2013 Permission has been added
				switch ( this.UserNameNoDomain.ToLower() )
				{
					case "johnsonp":
					case "mooreja":
					case "crouchj":
					case "marinia":
					case "robinsonr":
					case "atkinsonj":
					case "cooks":
					case "demarcos":    // PDJ: 08/08/11
					case "kleina":      // PDJ: 08/08/11
					case "pentonya":    // PDJ: 08/08/11
					case "chadwickj":   // PDJ: 09/21/11
					case "elnahalc":    // PDJ: 09/21/11
						return true;
					default:
						return false;
				}
			}
		}
		public bool CanManageRegSync
		{
			get
			{
				if ( this.HasPrivilege("prvACRMCanManageRegSync") )
					return true;

				// PDJ 7/1/2013 Permission has been added
				switch ( this.UserNameNoDomain.ToLower() )
				{
					case "johnsonp":
					case "mooreja":
					case "crouchj":
					case "hsiaoc":
					case "lintond":
					case "marinia":
					case "robinsonr":
					case "chadwickj":   // PDJ: 09/21/11
					case "elnahalc":    // PDJ: 09/21/11
						return true;
					default:
						return false;
				}
			}
		}
		public bool CanRunBatchNomination
		{
			get
			{
				if ( this.HasPrivilege("prvACRMCanRunBatchNomination") )
					return true;

				// PDJ 7/1/2013 Permission has been added
				switch ( this.UserNameNoDomain.ToLower() )
				{
					case "johnsonp":
					case "mooreja":
					case "crouchj":
					case "hsiaoc":
					case "lintond":
					case "marinia":
					case "robinsonr":
					case "chadwickj":   // PDJ: 09/21/11
					case "elnahalc":    // PDJ: 09/21/11
						return true;
					default:
						return false;
				}
			}
		}
		public bool CanManageHousing
		{
			get
			{
				if ( this.HasPrivilege("prvExsCanManageHousing") )
					return true;

				// PDJ 7/1/2013 Permission has been added
				switch ( this.UserNameNoDomain.ToLower() )
				{
					case "johnsonp":
					case "mooreja":
					case "crouchj":
					case "hsiaoc":
					case "lintond":
						return true;
					default:
						return false;
				}
			}
		}

		#endregion
	}
}
