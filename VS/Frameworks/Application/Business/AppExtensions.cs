using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace App.Framework.Business
{
	public static class AppExtensions
	{
		#region | Exception: FormatMessage |
		/// <summary>
		/// Standardize form error messages.
		/// </summary>
		public static string FormatMessage(this Exception ex)
		{
			return AppExtensions.FormatMessage(ex, string.Empty);
		}
		public static string FormatMessage(this Exception ex, string routineName)
		{
			if ( ex == null )
				return string.Empty;

			// Record error trap
			//ex.Log();

			var Msgs = new List<string>();

			if ( !routineName.IsEmpty() )
				Msgs.Add(routineName.Trim());

			// Loop thru Messages
			Exception HoldException = ex;
			var Str = new StringBuilder();
			while ( ex != null )
			{
				if ( !Msgs.Contains(ex.Message, StringComparison.CurrentCultureIgnoreCase) )
				{
					Msgs.Add(ex.Message);
					Str.AppendLine(ex.Message);
				}
				if ( !Msgs.Contains(ex.Source, StringComparison.CurrentCultureIgnoreCase) )
				{
					Msgs.Add(ex.Source);
					Str.AppendLine(ex.Source);
				}
				if ( !Msgs.Contains(ex.StackTrace, StringComparison.CurrentCultureIgnoreCase) )
				{
					Msgs.Add(ex.StackTrace);
					Str.AppendLine(ex.StackTrace);
				}
				ex = ex.InnerException;
			}
			//Str.AppendLine(string.Format("Log GUID: {0}", LastLogGUID));

			return Str.ToString();
		}
		#endregion

		#region | Execution: FormatMessage |
		/// <summary>
		/// Standardize form error messages.
		/// </summary>
		public static string FormatMessage(this ExecutionMessageCollection messagesList, bool concatOneLine = false)
		{
			return AppExtensions.FormatMessage(messagesList, string.Empty, concatOneLine);
		}
		public static string FormatMessage(this ExecutionMessageCollection messagesList, string routineName, bool concatOneLine = false)
		{
			if ( messagesList.Count == 0 )
				return string.Empty;

			var Msgs = new List<string>();

			if ( !routineName.IsEmpty() )
				Msgs.Add(routineName.Trim());

			// Loop thru Messages
			var Str = new StringBuilder();
			foreach ( ExecutionMessage Msg in messagesList )
			{
				string s = Msg.Text.Replace("PLEASE NOTE: ", "");
				if ( !Msgs.Contains(s, StringComparison.CurrentCultureIgnoreCase) )
				{
					Msgs.Add(s);
					if ( concatOneLine )
						Str.Append(string.Format("{0}{1}", Str.Length == 0 ? "" : " | ", s));
					else
						Str.AppendLine(s);
				}
			}

			return Str.ToString();
		}
		#endregion

		#region | object: InvokeUnlessDefault |
		/// <summary>
		/// Invoke an action if the input is not default.  For reference
		/// types, default means null.
		/// </summary>
		public static void InvokeUnlessDefault<TObj>(this TObj obj, Action<TObj> toRun)
		{
			if ( !object.Equals((object)obj, (object)default(TObj)) )
				toRun.Invoke(obj);
		}
		/// <summary>
		/// Invoke a function if the input is not its type's default value.
		/// For reference types, default means null.  Returns the result of
		/// the function if it was invoked, or the specified default value
		/// (or default for the result type) if not invoked.
		/// </summary>
		public static TResult InvokeUnlessDefault<TObj, TResult>(this TObj obj, Func<TObj, TResult> toRun, TResult defaultResult = default(TResult))
		{
			TResult Result = defaultResult;
			obj.InvokeUnlessDefault(O => { Result = toRun.Invoke(O); });
			return Result;
		}
		#endregion
	}
}
