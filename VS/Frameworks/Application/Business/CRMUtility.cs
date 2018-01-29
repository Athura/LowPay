using App.Framework.Business;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace App.Framework.Business
{
	/// <summary>
	/// CRM Common Generic functions not UI specific.
	/// </summary>
	public class CRMUtility
	{
		#region | IsNumeric |
		/// <summary>
		/// Is the object numeric.
		/// </summary>
		public static bool IsNumeric(object Expression)
		{
			// Variable to collect the Return value of the TryParse method.
			bool isNum;

			// Define variable to collect out parameter of the TryParse method. If the conversion fails, the out parameter is zero.
			double retNum;

			// The TryParse method converts a string in a specified style and culture-specific format to its double-precision floating point number equivalent.
			// The TryParse method does not generate an exception if the conversion fails. If the conversion passes, True is returned. If it does not, False is returned.
			isNum = Double.TryParse(Convert.ToString(Expression), System.Globalization.NumberStyles.Any, System.Globalization.NumberFormatInfo.InvariantInfo, out retNum);
			return isNum;
		}
		#endregion

		#region | IsValidDate |
		/// <summary>
		/// Determines if a date is valid. 
		/// </summary>
		public static bool IsValidDate(string dateTime)
		{
			bool ReturnVal = true;
			try
			{
				DateTime tmp = Convert.ToDateTime(dateTime);
			}
			catch
			{
				ReturnVal = false;
			}
			return ReturnVal;
		}
		#endregion

		#region | IsEmailFake |
		/// <summary>
		/// Check for fake email domains
		/// </summary>
		/// <param name="emailAddr">Email address to validate</param>
		public static bool IsEmailFake(string emailAddr)
		{
			emailAddr = emailAddr.Trim();
			return emailAddr.StartsWith("test123", StringComparison.CurrentCultureIgnoreCase)
				|| emailAddr.EndsWith(".test", StringComparison.CurrentCultureIgnoreCase)
				|| emailAddr.EndsWith("@test", StringComparison.CurrentCultureIgnoreCase)
				|| emailAddr.EndsWith(".example", StringComparison.CurrentCultureIgnoreCase)
				|| emailAddr.EndsWith("@example", StringComparison.CurrentCultureIgnoreCase)
				|| emailAddr.EndsWith(".invalid", StringComparison.CurrentCultureIgnoreCase)
				|| emailAddr.EndsWith("@invalid", StringComparison.CurrentCultureIgnoreCase)
				|| emailAddr.EndsWith(".localhost", StringComparison.CurrentCultureIgnoreCase)
				|| emailAddr.EndsWith("@localhost", StringComparison.CurrentCultureIgnoreCase)
				|| emailAddr.EndsWith(".example.com", StringComparison.CurrentCultureIgnoreCase)
				|| emailAddr.EndsWith("@example.com", StringComparison.CurrentCultureIgnoreCase)
				|| emailAddr.EndsWith(".example.org", StringComparison.CurrentCultureIgnoreCase)
				|| emailAddr.EndsWith("@example.org", StringComparison.CurrentCultureIgnoreCase)
				|| emailAddr.EndsWith(".example.net", StringComparison.CurrentCultureIgnoreCase)
				|| emailAddr.EndsWith("@example.net", StringComparison.CurrentCultureIgnoreCase)
				|| emailAddr.EndsWith(".experient-test.com", StringComparison.CurrentCultureIgnoreCase)
				|| emailAddr.EndsWith("@experient-test.com", StringComparison.CurrentCultureIgnoreCase);
		}
		#endregion

		#region | IsEmailValid |
		/// <summary>
		/// Validate the email address
		/// </summary>
		/// <param name="emailAddr">Email address to validate</param>
		/// <param name="checkForFakes">Check fake email addresses, when true will test and fail fakes</param>
		/// <param name="checkForProd">Check Production, when true will and not Production will only pass Experient emails</param>
		public static bool IsEmailValid(string emailAddr, bool checkForFakes = false, bool checkForProd = false)
		{
			if ( emailAddr.IsEmpty() )
				return false;
			if ( checkForFakes && CRMUtility.IsEmailFake(emailAddr) )
				return false;
			if ( checkForProd && !CRMUtility.IsEmailWhiteListed(emailAddr) )
				return false;
			return EmailRegex.IsMatch(emailAddr);
		}
		private static Regex EmailRegex = new Regex(EMAIL_REGEX_PATTERN, RegexOptions.Compiled | RegexOptions.IgnoreCase);
		private const string EMAIL_REGEX_PATTERN =
			@"^[\w!#$%&'*+\-/=?\^_`{|}~]+(\.[\w!#$%&'*+\-/=?\^_`{|}~]+)*"
			+ "@"
			+ @"((([\w]+([-\w]*[\w]+)*\.)+[a-zA-Z]{2,9})|"
			+ @"((([01]?[0-9]{1,2}|2[0-4][0-9]|25[0-5]).){3}[01]?[0-9]{1,2}|2[0-4][0-9]|25[0-5]))\z";
		#endregion

		#region | IsEmailWhiteListed |
		/// <summary>
		/// Check for fake email domains
		/// </summary>
		/// <param name="emailAddr">Email address to validate</param>
		public static bool IsEmailWhiteListed(string emailAddr)
		{
			emailAddr = emailAddr.Trim();
			return (emailAddr.EndsWith("@experient-inc.com", StringComparison.CurrentCultureIgnoreCase)
				|| emailAddr.Equals("pdjmwj@gmail.com", StringComparison.CurrentCultureIgnoreCase)
				|| emailAddr.Equals("pdjmwj@live.com", StringComparison.CurrentCultureIgnoreCase)
				|| emailAddr.Equals("sioconnellan@gmail.com", StringComparison.CurrentCultureIgnoreCase));
		}
		#endregion

		#region | CreateDataTable |
		public static DataTable CreateDataTable<T>(IEnumerable<T> list)
		{
			Type type = typeof(T);
			var properties = type.GetProperties();

			var toFill = new DataTable(type.Name);
			foreach ( PropertyInfo info in properties )
			{
				if ( !info.Name.Equals("ExtensionData", StringComparison.CurrentCultureIgnoreCase) )
					toFill.Columns.Add(new DataColumn(info.Name, Nullable.GetUnderlyingType(info.PropertyType) ?? info.PropertyType));
			}

			foreach ( T entity in list )
			{
				int i = 0;
				object[] values = new object[toFill.Columns.Count];
				foreach ( PropertyInfo info in properties )
				{
					if ( !info.Name.Equals("ExtensionData", StringComparison.CurrentCultureIgnoreCase) )
					{
						values[i] = info.GetValue(entity);
						i++;
					}
				}
				toFill.Rows.Add(values);
			}

			return toFill;
		}
		#endregion
	}
}
