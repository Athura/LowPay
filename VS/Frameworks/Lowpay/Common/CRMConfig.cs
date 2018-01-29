using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using App.Framework.Business;

namespace Lowpay.Framework.Common
{
	/// <summary>
	/// Dynamic application configuration options for the CRM system
	/// </summary>
	public static class CRMConfig
	{
		#region | enum: eKeyName |
		public enum eKeyName
		{
			ApplicationMode = 1,
			AttachmentsDirectory,

			AWS3AccessKey,
			AWS3BucketKeyDSE,
            AWS3BucketKeyCE,
            AWS3BucketName,
			AWS3SecretKey,
			AWS3BucketURL,

			BlasterDoSend,
			BlasterDoUpload,
			BlasterDSEContentPath,
			BlasterDSEContentURL,
			BlasterFTPExternalPath,
			BlasterFTPInternalPath,

			// Options: CustomField 
			CFIDCompanyInfoList,
			CFIDCompanyScrnList,
			CFIDCompanySource,
			CFIDCompanyTier,
			CFIDContactInfoList,
			CFIDContactScrnList,
			CFIDContactSource,
			CFIDGeneralLedger,
			CFIDOrderSourceCode,
			CFIDPromoCode,
			CFIDResponseMethod,
			CFIDStrategySubType,

			Company1LabelText,
			Company2LabelText,
			CompanyName,
			CompanyProductsExhibitorURL,
			CompanyURL,

			ConnectionsActDocURL,
			ConnectionsManagerURL,
			ConnectionsPlatformURL,
			ConnectionsPortalURL,
			ConnxExhPortalAuthToken,
			ConnxExhServiceAuthToken,
			ConnxServiceDomain,
			ConnxServicePassword,
			ConnxServiceUserName,

			//creditCardProcessorApplicationConnectionTimeout,
			//creditCardProcessorApplicationName,
			//creditCardProcessorApplicationPassword,
			//creditCardProcessorApplicationVersion,
			//creditCardProcessorApplicationWebServiceUrl,
			//creditCardProcessorAuthAcceptedTransStatus,
			//creditCardProcessorRealtimeAuthentication,

			CPOrderSourceCodeDefault,
			CustomerPortalDataSalesOutputFolder,
			CustomerPortalURL,
			CustomPrintoutAllowedChars,
			CustomRuleList,
			DataSalesLeadCountURL,
			DefaultOwnershipRequired,
			DefaultTrackItApplication,
			DefaultTrackItTechnician,
			DupeCheckEmail,
			DupeCheckEmailCompany,
			EmailCustomerService,
			EmailDoNotReply,
			EPDownForMaintenance,
			EPExcludedDownloadShows,
			ExpoCardWebURL,
			ExsApplicationMode,
			ApplicationCode,
			FaxBroadcastEmailAccount,
			FaxSingleEmailAccount,
			ForceUpperCase,
			FormMainMenuBar,
			FormMainTitle,
			ImportListLibraryPath,
			ImportUploadPath,

			IsBoothTabVisible,
			IsImportShowEnabled,
			IsInaDelMrgTfrImpEnabled,
			IsOnsite,
			IsOrderTabVisible,
			IsSnapShotTabVisible,
			IsTabCompanyVisible,
			IsTabContactVisible,
			IsThirdPartyVisible,
			IsVIPVisible,

			MaritzImpactDimensionsURL,
			MaxDownloadCount,
			MaxDownloadMin,

			MerchantServer,			// Obsolete
			MerchantServerPort,     // Obsolete

			NoConfirmationShowCodes,
			NoConfSwapProductCodes,
			OrderSourceCodeTeleMarketing,

			PCIAllowSaveOnlyTransaction,
			PCICCUIWinFormDownloadBaseUrl,
			PCIErrorThreshold,
			PCIErrorThresholdFloor,
			PCISecurePaymentBaseUrl,
			PCIShowCode,
			PCIShowKey,
			PCITestMode,
			PCIWebServiceBaseUrl,

			PeerReportAddonFiles,
			PeerReportExportPath,
			PeerReportSamplePage,
			PeerReportServerUrl,

			PhoneCustomerService,
			PhoneCustomerServiceIntl,

			ProductCodesLead,
			ProductCodesSwap,

			QueryBuilderCompanyView,
			QueryBuilderContactView,
			QueryBuilderSubQueryViews,

			ReaderShortenedLineLimit,
			ReaderStandardLineLimit,
			RegReporterURL,
			ReportingLoginURL,

			SearchColumnsCompany,
			SecurityEnabled,
			SendEmails,
			ShortenedLineReaderProdIDList,
			ShowManagerURL,
			SmtpServer,

			SSRSPassword,
			SSRSUsername,
			SyncA2ZAuthKey,
			SyncA2ZPassword,
			SyncA2ZUserName,
			SystemGoLiveDate,

			TargetListExportFromEmailAddress,
			TargetListExportFileType,
			TargetListExportFTPPath,

			TemplatesDirectory,
			UnifiedDeployment_Environment,
			UseFuzzyLookup,
			UseOutlookEmail,
			UserGuideFilePath,
			ValidCreditCards,

			VertexEnabled,
			VertexWebServiceURL,
			
			AppLauncherExeLocation,			// ACRM
			AuditLogCompanyCFieldList,      // ACRM
			AuditLogContactCFieldList,      // ACRM
			DefaultOwnershipCode,           // ACRM
			IsPurchaseHistoryEnabled,       // ACRM
			ShowGroupCode,                  // ACRM
			ShowOwnerCode,                  // ACRM
		}
		#endregion

		#region | enum: eApplicationMode |
		public enum eApplicationMode
		{
			EXS = 1,
			CVCG,
			FASH,
			LICS,
			MOTR
		}
		#endregion

		#region | enum: eEnvironmentMode |
		public enum eEnvironmentMode
		{
			PROD = 1,
			MAINT,
			QA,
			DEV
		}
		#endregion

		#region | enum: eDupeCheckEmailOption |
		/// <summary>
		/// The type of DupCheckEmail
		/// </summary>
		public enum eDupeCheckEmailOption
		{
			OFF,
			WARN,
			ERR
		}
		#endregion

		#region | Declarations |

		private static string _ApplicationCode = "";
		private static string _ApplicationMode = "";
		private static string _EnvironmentMode = "";
		private static string _EnvironmentTypeCode = "";

		#endregion

		#region | Construction |
		static CRMConfig()
		{
			Clear();
		}
		#endregion

		#region | Clear |
		/// <summary>
		/// Clear configuration.
		/// </summary>
		private static void Clear()
		{
			_ApplicationCode = "";
			_ApplicationMode = "";
			_EnvironmentMode = "";
			_EnvironmentTypeCode = "";
		}
		#endregion

		#region | Reset |
		/// <summary>
		/// Load / Reset CRM resources
		/// </summary>
		public static void Reset()
		{
			Clear();
		}
		#endregion

		#region | GetConfigBoolean |
		/// <summary>
		/// Get config ItemValue from CRMConfigSettings.
		/// </summary>
		public static bool GetConfigBoolean(CRMConfig.eKeyName keyName, bool defaultValue = false)
		{
			return GetConfigBoolean(keyName.ToString(), defaultValue);
		}
		/// <summary>
		/// Get config ItemValue from CRMConfigSettings.
		/// </summary>
		public static bool GetConfigBoolean(string keyName, bool defaultValue = false)
		{
			bool ToReturn = defaultValue;
			try
			{
				string itemValue = GetConfigString(keyName);
				if ( !itemValue.IsEmpty() )
				{
					if ( itemValue == "1"
						|| itemValue.Equals("Y", StringComparison.CurrentCultureIgnoreCase)
						|| itemValue.Equals("YES", StringComparison.CurrentCultureIgnoreCase) )
						ToReturn = true;
					else
						ToReturn = Convert.ToBoolean(itemValue);
				}
			}
			catch
			{
				ToReturn = defaultValue;
			}
			return ToReturn;
		}
		#endregion

		#region | GetConfigInt32 |
		/// <summary>
		/// Get config ItemValue from CRMConfigSettings.
		/// </summary>
		public static int GetConfigInt32(CRMConfig.eKeyName keyName, int defaultValue = 0)
		{
			return GetConfigInt32(keyName.ToString(), defaultValue);
		}
		/// <summary>
		/// Get config ItemValue from CRMConfigSettings.
		/// </summary>
		public static int GetConfigInt32(string keyName, int defaultValue = 0)
		{
			int ToReturn = defaultValue;
			try
			{
				string itemValue = GetConfigString(keyName);
				if ( !itemValue.IsEmpty() )
					ToReturn = Convert.ToInt32(itemValue);
			}
			catch
			{
				ToReturn = defaultValue;
			}
			return ToReturn;
		}
		#endregion

		#region | GetConfigString |
		/// <summary>
		/// Get config ItemValue from CRMConfigSettings.
		/// </summary>
		public static string GetConfigString(CRMConfig.eKeyName keyName, string defaultValue = "")
		{
			return GetConfigString(keyName.ToString(), defaultValue);
		}
		public static string GetConfigString(string keyName, string defaultValue = "")
		{
			string ToReturn = defaultValue.Trim();
			try
			{
				string itemValue = CRMConfig.SettingsCollection[keyName.Trim()].Trim();
				if ( !itemValue.IsEmpty() )
					ToReturn = itemValue;
			}
			catch
			{
				ToReturn = defaultValue;
			}
			return ToReturn;
		}
		#endregion

		#region | SettingsCollection |
		public static NameValueCollection SettingsCollection
		{
			get
			{
				if ( ConfigSettingsBase.SettingsCollectionBase == null )
				{
					// PDJ 10/14/08: Per John Crouch, 
					// A setting in connection manager in Release appears
					// to be referencing config settings prior to being loaded.
					// Unsure why Debug is okay and Release is sometimes NOT okay.
					// Added this message to help determine when this is happening.
					//throw new Exception("ConfigSettingsBase.SettingsCollectionBase is null.");
					return new NameValueCollection();
				}
				return ConfigSettingsBase.SettingsCollectionBase;
			}
		}
		#endregion

		#region | Properties |

		#region | ApplicationCode |
		/// <summary>
		/// Get a value that uniquely identiifies the application code used to 
		/// identify the application in Experient
		/// </summary>
		public static string ApplicationCode
		{
			get
			{
				if ( _ApplicationCode.IsEmpty() )
					_ApplicationCode = GetConfigString(eKeyName.ApplicationCode).ToUpper();
				return _ApplicationCode;
			}
		}
		#endregion
		#region | ApplicationMode |
		/// <summary>
		/// Get a value that uniquely identiifies the client for the CRM database
		/// </summary>
		public static string ApplicationMode
		{
			get
			{
				if ( _ApplicationMode.IsEmpty() )
					_ApplicationMode = GetConfigString(eKeyName.ApplicationMode).ToUpper();
				return _ApplicationMode;
			}
		}
		#endregion
		#region | EnvironmentMode |
		/// <summary>
		/// Get a value that identifies the environment in which the database is running PROD, MAINT, QA, DEV
		/// </summary>
		public static string EnvironmentMode
		{
			get
			{
				if ( _EnvironmentMode.IsEmpty() )
					_EnvironmentMode = GetConfigString(eKeyName.ExsApplicationMode).ToUpper();
				return _EnvironmentMode;
			}
		}
		#endregion
		#region | UnifiedDeployment_Environment |
		/// <summary>
		/// Get a value that identifies the environment in which the database is running PROD, MAINT, QA, DEV
		/// </summary>
		public static string UnifiedDeployment_Environment
		{
			get
			{
				if ( _EnvironmentTypeCode.IsEmpty() )
					_EnvironmentTypeCode = GetConfigString(eKeyName.UnifiedDeployment_Environment).ToUpper();
				return _EnvironmentTypeCode;
			}
		}
		#endregion

		#region | SmtpServer |
		/// <summary>
		/// Get smtp email server for quick one off emails
		/// </summary>
		public static string SmtpServer
		{
			get
			{
				const string DefaultServer = @"smtp3.experientevent.com";
				return GetConfigString(eKeyName.SmtpServer, DefaultServer);
			}
		}
		#endregion
		#region | UseOutlookEmail |
		/// <summary>
		/// Gets a value that flags the application to send adhoc email using Outlook 
		/// </summary>
		public static bool UseOutlookEmail
		{
			get
			{
				if ( CRMConfig.IsCitrixEnvironment )
					return false;
				return GetConfigBoolean(eKeyName.UseOutlookEmail);
			}
		}
		#endregion
		#region | EmailDoNotReply |
		/// <summary>
		/// Get CRM do not reply email address
		/// </summary>
		public static string EmailDoNotReply
		{
			get { return GetConfigString(eKeyName.EmailDoNotReply, @"donotreply@experient-inc.com"); }
		}
		#endregion

		#region | IsCitrixEnvironment |
		public static bool IsCitrixEnvironment
		{
			get
			{
				string CitrixSetting = System.Environment.GetEnvironmentVariable("RegNetMultiUser");
				return (CitrixSetting == "1");
			}
		}
		#endregion
		#region | IsProduction |
		/// <summary>
		/// Gets a value that flags the application as production or not
		/// </summary>
		public static bool IsProduction
		{
			get { return CRMConfig.EnvironmentMode.Equals(eEnvironmentMode.PROD.ToString(), StringComparison.CurrentCultureIgnoreCase); }
		}
		#endregion

		#endregion

		#region | Properties: AWS3 |

		#region | AWS3AccessKey |
		/// <summary>
		/// Amazon S3 Access Key
		/// </summary>
		public static string AWS3AccessKey
		{
			get { return GetConfigString(eKeyName.AWS3AccessKey, ""); }
		}
		#endregion

		#region | AWS3SecretKey |
		/// <summary>
		/// Amazon S3 Secret Key
		/// </summary>
		public static string AWS3SecretKey
		{
			get { return GetConfigString(eKeyName.AWS3SecretKey, ""); }
		}
		#endregion

		#region | AWS3BucketName |
		/// <summary>
		/// Amazon S3 bucket name, folder arrangement
		/// </summary>
		public static string AWS3BucketName
		{
			get
			{
				string itemValue = GetConfigString(eKeyName.AWS3BucketName, "");
				if ( !itemValue.IsEmpty() )
					itemValue = string.Format(itemValue, EnvironmentMode);
				return itemValue.ToLower();
			}
		}
		#endregion

		#region | AWS3BucketKeyDSE |
		/// <summary>
		/// Amazon S3 bucket key for data sales exhibitor
		/// </summary>
		public static string AWS3BucketKeyDSE
		{
			get { return GetConfigString(eKeyName.AWS3BucketKeyDSE, "").ToLower(); }
		}
        #endregion

        #region | AWS3BucketKeyCE |
        /// <summary>
        /// Amazon S3 bucket key for data sales exhibitor
        /// </summary>
        public static string AWS3BucketKeyCE
        {
            get { return GetConfigString(eKeyName.AWS3BucketKeyCE, "").ToLower(); }
        }
        #endregion

        #region | AWS3BucketURL |
        /// <summary>
        /// Amazon S3 S3 bucket URL for file public access 
        /// </summary>
        public static string AWS3BucketURL
		{
			get { return GetConfigString(eKeyName.AWS3BucketURL, "").ToLower(); }
		}
		#endregion

		#endregion
	}
}
