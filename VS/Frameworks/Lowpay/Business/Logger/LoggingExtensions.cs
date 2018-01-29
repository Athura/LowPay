using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using Lowpay.Framework.Common;

namespace Lowpay.Framework.Business
{
	/// <summary>
	/// Various extension methods to assist with log handling.
	/// </summary>
	public static class LoggingExtensions
	{
		#region ToTraceEventType
		/// <summary>
		/// Converts a LogSeverity to a TraceEventType for logging purposes.
		/// </summary>
		public static TraceEventType ToTraceEventType(this LogSeverity severity)
		{
			switch ( severity )
			{
				case LogSeverity.Critical:
					return TraceEventType.Critical;
				case LogSeverity.Error:
					return TraceEventType.Error;
				case LogSeverity.Warning:
					return TraceEventType.Warning;
				case LogSeverity.Information:
					return TraceEventType.Information;
			}

			// SUENA: 2010-06-09 14:38 - If they added an event type to LogSeverity and
			// didn't add it here, we could try to look for one with a matching name.
			TraceEventType FallBack = TraceEventType.Error;
			Enum.TryParse<TraceEventType>(severity.ToString(), out FallBack);

			// Default to error in the unlikely event that someone added some new types
			// to our list and didn't update this code.
			return FallBack;
		}
		#endregion

		#region Exception.Log
		/// <summary>
		/// Log an Exception to the Connections Logging System.
		/// </summary>
		/// <seealso cref="EXSLogger"/>
		/// <param name="ex">Exception to log.</param>
		/// <param name="severity">Severity of event, used by config filters.</param>
		/// <param name="additionalInfo">An optional Dictionary of objects to be included in the
		/// logging information.  The public properties of the object will be included in the
		/// log entry.</param>
		/// <param name="priority">Message priority, possibly used in config filters.</param>
		public static void Log(this Exception ex, LogSeverity severity = LogSeverity.Error,
			IDictionary<string, object> additionalInfo = null, int priority = 1)
		{
			EXSLogger.Log(ex, severity, additionalInfo, priority);
		}
		#endregion
	}
}
