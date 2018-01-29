using System;
using System.ComponentModel;

namespace RTS.Service.Common
{
	/// <summary>
	/// An enumeration representing the ID's of the different ExecMsg's that
	/// can be raised by the system.  All messages in all applications and
	/// layers need to be registered here.
	/// </summary>
	public enum SysMsgCode : int
	{
#pragma warning disable 1591

		[Description("Unable to find an Event with ShowCode \"{0}\".")]
		EventCtrlrCannotFindEventByCode = 300,

		[Description("Login failed: Incorrect username or password.")]
		InvalidLogin = 1001,
		[Description("You are not authorized to access this action.")]
		UnauthorizedAction = 1002,
		[Description("Password does not match verify password.")]
		PasswordVerifyMismatch = 1003,
		[Description("Auth Token is not valid or has expired.")]
		AuthTokenNotValid = 1004,
	
		[Description("{0}")]
		ExecutionErrorMessageRaised = 2001,
		[Description("[Warning] {0}")]
		ExecutionWarningMessageRaised = 2002,

		[Description("Invalid Company, {0} is a required field.")]
		CompanyInvalidRequiredField = 2011,
		[Description("Invalid Contact, {0} is a required field.")]
		ContactInvalidRequiredField = 2021,
		[Description("Invalid Booth, {0} is a required field.")]
		BoothInvalidRequiredField = 2031,
		[Description("Invalid Order, {0} is a required field.")]
		OrderInvalidRequiredField = 2041,
		[Description("Invalid Purchase, {0} is a required field.")]
		PurchaseInvalidRequiredField = 2051,
		[Description("Invalid Purchase, {0} is a required field.")]
		PaymentInvalidRequiredField = 2061,

		// SPECIAL/OTHER
		[Description("An internal error has occurred.  {0}")]
		InternalError = 999999,
		[Description("{0}")]
		[Obsolete("OtherError should be replaced with a more specific code.")]
		OtherError = 1000000

#pragma warning restore 1591
	}

}
