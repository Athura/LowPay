using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Lowpay.Framework.Business;
using RTS.Service.Business;
using RTS.Service.Common;

namespace RTS.Service.API
{
	public class WebServiceBase
	{
		#region ResultWrapper
		public static ServiceResponse<T> ResultWrapper<T>(Func<T> toExecute)
		{
			//Global.AutoInitializeDMContext();
			return ServiceResponse<T>.Create(toExecute);
		}
		public static ServiceResponseVoid ResultWrapper(Action toExecute)
		{
			//Global.AutoInitializeDMContext();
			return ServiceResponseVoid.Create(toExecute);
		}
		#endregion

		#region AuthWrapper
		public static void AuthWrapper(string auth, Action toExecute)
		{
			var Token = SecurityService.CheckAuthToken(auth);
			if ( Token == null )
				ExecMsgScope.RaiseMsg(ExecMsgSeverity.Error, SysMsgCode.AuthTokenNotValid);
			// Throw this token info into the context so logging can include it if needed.
			// Using the item [] instead of Add() method will replace an existing item in the Dictionary, where
			// the Add method throws an exception if there is already an item with the same name in the Dictionary.
			EXSLogger.ExtraEnvironmentInfo["UserName"] = Token.UserName;
			EXSLogger.ExtraEnvironmentInfo["ExpDateUTC"] = Token.ExpiresUTC;
			Token.ExecuteAsUser(toExecute);
		}
		public static T AuthWrapper<T>(string auth, Func<T> toExecute)
		{
			return toExecute.WrapAction(A => AuthWrapper(auth, A)).Invoke();
		}
		#endregion

		#region AuthResultWrapper
		public static ServiceResponse<T> AuthResultWrapper<T>(string auth, Func<T> toExecute)
		{
			return ResultWrapper<T>(() => AuthWrapper<T>(auth, toExecute));
		}
		public static ServiceResponseVoid AuthResultWrapper(string auth, Action toExecute)
		{
			return ResultWrapper(() => AuthWrapper(auth, toExecute));
		}
		#endregion

		#region AuthPermissionWrapper
		public static void AuthPermissionWrapper(string auth, Action toExecute)
		{
			AuthWrapper(auth, () =>
			{
				// We have decided that Authentication is good enough for access
				// to this site.  So we don't implement any special permission code
				// here.
				toExecute.Invoke();
			});
		}
		public static T AuthPermissionWrapper<T>(string auth, Func<T> toExecute)
		{
			return toExecute.WrapAction(A => AuthPermissionWrapper(auth, A)).Invoke();
		}
		#endregion

		#region AuthPermissionResultWrapper
		public static ServiceResponse<T> AuthPermissionResultWrapper<T>(string auth, Func<T> toExecute)
		{
			return ResultWrapper<T>(() => AuthPermissionWrapper<T>(auth, toExecute));
		}
		public static ServiceResponseVoid AuthPermissionResultWrapper(string auth, Action toExecute)
		{
			return ResultWrapper(() => AuthPermissionWrapper(auth, toExecute));
		}
		#endregion
	}
}