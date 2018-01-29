using System;
using System.ComponentModel;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using RTS.Service.Business;
using RTS.Service.Common;

namespace RTS.Service.API
{
	[DisplayName("Authentication Service")]
	[AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
	[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
	[ServiceContract]
	public class AuthService : WebServiceBase
	{
		#region LoginByUsernamePassword
		/// <summary>
		/// Log in using a username and password.
		/// </summary>
		/// <param name="username">Username with which to log in.</param>
		/// <param name="password">Password with which to log in.</param>
		/// <param name="ttlSeconds">Time to live for this token, in seconds.</param>
		/// <returns>A string containing an authentication token.  This token should be passed in as the first "auth"
		/// parameter for operations that require authentication.</returns>
		[DisplayName("Login by Username/Password")]
		[OperationContract]
		[WebGet]
		public ServiceResponse<string> LoginByUsernamePassword(string username, string password, int ttlSeconds)
		{
			return ResultWrapper(() =>
			{
				//UserBase UB = new UserBaseService().LoginByUserNamePassword(username, password);
				//if ( UB == null )
				//    ExecMsgScope.RaiseMsg(ExecMsgSeverity.Error, SysMsgCode.InvalidLogin);
				var TR = AuthService.LoginByUserNamePassword(username, password, (ttlSeconds <= 0) ? TimeSpan.Zero : TimeSpan.FromSeconds(ttlSeconds));
				if ( TR == null )
					ExecMsgScope.RaiseMsg(ExecMsgSeverity.Error, SysMsgCode.InvalidLogin);
				return TR.ToString();
			});
		}
		#endregion

		#region LoginByAuthToken
		/// <summary>
		/// Create a new auth token, given a previous auth token.
		/// </summary>
		/// <param name="auth">The original authentication token with which to identify yourself.</param>
		/// <param name="ttlSeconds">New time to live for this token, in seconds.</param>
		/// <returns>A string containing an authentication token.  This token should be passed in as the first "auth"
		/// parameter for operations that require authentication.</returns>
		[DisplayName("Renew Existing Auth Token")]
		[OperationContract]
		[WebGet]
		public ServiceResponse<string> LoginByAuthToken(string auth, int ttlSeconds)
		{
			return AuthResultWrapper(auth, () =>
			{
				var TR = AuthService.LoginByAuthToken(auth, (ttlSeconds <= 0) ? TimeSpan.Zero : TimeSpan.FromSeconds(ttlSeconds));
				if ( TR == null )
					ExecMsgScope.RaiseMsg(ExecMsgSeverity.Error, SysMsgCode.InvalidLogin);
				return TR.ToString();
			});
		}
		#endregion

		#region GetUserNameFromAuthToken
		/// <summary>
		/// Retrieve the user name from the given auth token.
		/// </summary>
		/// <remarks>
		/// Extract the User's Name from a valid auth token.  If the auth token has expired,
		/// then an error of "Token has expired" is returned.
		/// </remarks>
		/// <param name="auth">The authentication token to exract the user name from.</param>
		[DisplayName("Extract User Name from existing Auth Token")]
		[OperationContract]
		[WebGet]
		public ServiceResponse<string> GetUserNameFromAuthToken(string auth)
		{
			return AuthResultWrapper(auth, () =>
			{
				var TR = AuthService.GetUserNameFromAuthToken(auth);
				if ( TR == null )
					ExecMsgScope.RaiseMsg(ExecMsgSeverity.Error, SysMsgCode.AuthTokenNotValid);
				return TR;
			});
		}
		#endregion

	}
}
