using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Lowpay.Framework.Business
{
	//
	// Summary:
	//     Represents a log message. Contains the common properties that are required for
	//     all log messages.
	[XmlRoot("logEntry")]
	public class LogEntry : ICloneable
	{
		//
		// Summary:
		//     Initialize a new instance of a Microsoft.Practices.EnterpriseLibrary.Logging.LogEntry
		//     class.
		public LogEntry()
		{
		}
		//
		// Summary:
		//     Create a new instance of Microsoft.Practices.EnterpriseLibrary.Logging.LogEntry
		//     with a full set of constructor parameters
		//
		// Parameters:
		//   message:
		//     Message body to log. Value from ToString() method from message object.
		//
		//   category:
		//     Category name used to route the log entry to a one or more trace listeners.
		//
		//   priority:
		//     Only messages must be above the minimum priority are processed.
		//
		//   eventId:
		//     Event number or identifier.
		//
		//   severity:
		//     Log entry severity as a Microsoft.Practices.EnterpriseLibrary.Logging.LogEntry.Severity
		//     enumeration. (Unspecified, Information, Warning or Error).
		//
		//   title:
		//     Additional description of the log entry message.
		//
		//   properties:
		//     Dictionary of key/value pairs to record.
		public LogEntry(object message, string category, int priority, int eventId, TraceEventType severity, string title, IDictionary<string, object> properties)
		{
		}
		//
		// Summary:
		//     Create a new instance of Microsoft.Practices.EnterpriseLibrary.Logging.LogEntry
		//     with a full set of constructor parameters
		//
		// Parameters:
		//   message:
		//     Message body to log. Value from ToString() method from message object.
		//
		//   categories:
		//     Collection of category names used to route the log entry to a one or more sinks.
		//
		//   priority:
		//     Only messages must be above the minimum priority are processed.
		//
		//   eventId:
		//     Event number or identifier.
		//
		//   severity:
		//     Log entry severity as a Microsoft.Practices.EnterpriseLibrary.Logging.LogEntry.Severity
		//     enumeration. (Unspecified, Information, Warning or Error).
		//
		//   title:
		//     Additional description of the log entry message.
		//
		//   properties:
		//     Dictionary of key/value pairs to record.
		public LogEntry(object message, ICollection<string> categories, int priority, int eventId, TraceEventType severity, string title, IDictionary<string, object> properties)
		{
		}

		//
		// Summary:
		//     Gets the default title for an entry.
		public static string DefaultTitle { get; }
		//
		// Summary:
		//     The System.AppDomain in which the program is running
		public string AppDomainName { get; set; }
		//
		// Summary:
		//     Tracing activity id as a string to support WMI Queries
		public string ActivityIdString { get; }
		//
		// Summary:
		//     Gets the error message with the Microsoft.Practices.EnterpriseLibrary.Logging.LogEntry
		public string ErrorMessages { get; }
		//
		// Summary:
		//     Related activity id
		public Guid? RelatedActivityId { get; set; }
		//
		// Summary:
		//     Tracing activity id
		public Guid ActivityId { get; set; }
		//
		// Summary:
		//     Read-only property that returns the timeStamp formatted using the current culture.
		public string TimeStampString { get; }
		//
		// Summary:
		//     Dictionary of key/value pairs to record.
		public IDictionary<string, object> ExtendedProperties { get; set; }
		//
		// Summary:
		//     The Win32 Thread ID for the current thread.
		public string Win32ThreadId { get; set; }
		//
		// Summary:
		//     The name of the .NET thread.
		public string ManagedThreadName { get; set; }
		//
		// Summary:
		//     The name of the current running process.
		public string ProcessName { get; set; }
		//
		// Summary:
		//     The Win32 process ID for the current running process.
		public string ProcessId { get; set; }
		//
		// Summary:
		//     Date and time of the log entry message.
		public DateTime TimeStamp { get; set; }
		//
		// Summary:
		//     Category names used to route the log entry to a one or more trace listeners.
		//     This readonly property is available to support WMI queries
		public string[] CategoriesStrings { get; }
		//
		// Summary:
		//     Additional description of the log entry message.
		public string Title { get; set; }
		//
		// Summary:
		//     Gets the string representation of the Microsoft.Practices.EnterpriseLibrary.Logging.LogEntry.Severity
		//     enumeration.
		public string LoggedSeverity { get; }
		//
		// Summary:
		//     Log entry severity as a Microsoft.Practices.EnterpriseLibrary.Logging.LogEntry.Severity
		//     enumeration. (Unspecified, Information, Warning or Error).
		public TraceEventType Severity { get; set; }
		//
		// Summary:
		//     Event number or identifier.
		public int EventId { get; set; }
		//
		// Summary:
		//     Importance of the log message. Only messages whose priority is between the minimum
		//     and maximum priorities (inclusive) will be processed.
		public int Priority { get; set; }
		//
		// Summary:
		//     Category name used to route the log entry to a one or more trace listeners.
		public ICollection<string> Categories { get; set; }
		//
		// Summary:
		//     Message body to log. Value from ToString() method from message object.
		public string Message { get; set; }
		//
		// Summary:
		//     Name of the computer.
		public string MachineName { get; set; }

		//
		// Summary:
		//     Gets the current process name.
		//
		// Returns:
		//     The process name.
		public static string GetProcessName()
		{
			return string.Empty;
		}
		//
		// Summary:
		//     Add an error or warning message to the start of the messages string builder.
		//
		// Parameters:
		//   message:
		//     Message to be added to this instance
		public virtual void AddErrorMessage(string message)
		{
		}
		//
		// Summary:
		//     Creates a new Microsoft.Practices.EnterpriseLibrary.Logging.LogEntry that is
		//     a copy of the current instance.
		//
		// Returns:
		//     A new LogEntry that is a copy of the current instance.
		//
		// Remarks:
		//     If the dictionary contained in Microsoft.Practices.EnterpriseLibrary.Logging.LogEntry.ExtendedProperties
		//     implements System.ICloneable, the resulting Microsoft.Practices.EnterpriseLibrary.Logging.LogEntry
		//     will have its ExtendedProperties set by calling Clone(). Otherwise the resulting
		//     Microsoft.Practices.EnterpriseLibrary.Logging.LogEntry will have its ExtendedProperties
		//     set to null.
		public object Clone()
		{
			return null;
		}
		//
		// Summary:
		//     Returns a System.String that represents the current Microsoft.Practices.EnterpriseLibrary.Logging.LogEntry,
		//     using a default formatting template.
		//
		// Returns:
		//     A System.String that represents the current Microsoft.Practices.EnterpriseLibrary.Logging.LogEntry.
		public override string ToString()
		{
			return string.Empty;
		}
	}
}
