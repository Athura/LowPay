using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Xml.Linq;
using System.Web;
using Lowpay.Framework.Common;

namespace Lowpay.Framework.Business
{
	#region | LogSeverity |
	/// <summary>
	/// Severity of the information being logged by ConnectionsLogger.
	/// </summary>
	public enum LogSeverity
	{
		/// <summary>
		/// Critical error, meaning system cannot recover and app will probably crash.
		/// </summary>
		Critical = 0,

		/// <summary>
		/// Error - exception may or may not have recovered.
		/// </summary>
		Error = 1,

		/// <summary>
		/// Warning message - conditions which may be problematic, but are non-fatal.
		/// </summary>
		Warning = 2,

		/// <summary>
		/// Informational message - conditions which are not thought to be problematic, but are noteworthy.
		/// </summary>
		Information = 3,

		/// <summary>
		/// Extra info message - conditions which are probably of little or no importance for most purposes.
		/// </summary>
		Verbose = 4
	}
	#endregion

	#region | EXSLogger |
	/// <summary>
	/// System message logging for EXS.  This will log errors, warnings and informational,
	/// and various other severity of messages.  Some messages, such as exceptions, may have special
	/// handling.  A large amount of XML environment information is also dumped into the log.
	/// Logging is passed through the Microsoft Enterprise Library, so additional listeners such
	/// as email can be setup via config.
	/// </summary>
	public static class EXSLogger
	{
		/// <summary>
		/// Allows extra information to be added to the log entry.
		/// </summary>
		public static event EventHandler LogExtraInfo = null;

		#region Private Static Fields
		private static object InitLock = new object();
		private static bool IsInitialized;
		//private static LogWriter _AppLogger = null;
		#endregion

		#region Initialize
		/// <summary>
		/// Initialize logging, if it's not already up and running.  Moving this out from the
		/// static constructor allows it to be called explicitly to be sure the logger has been
		/// initialized early enough to catch some exceptions before events are logged.
		/// </summary>
		public static void Initialize()
		{
			lock ( InitLock )
			{
				if ( IsInitialized )
					return;

				try
				{
					// First, try to create the logging components using the application config file, allowing
					// override and fine-grained control over options.
					//_AppLogger = EnterpriseLibraryContainer.Current.GetInstance<LogWriter>();
					IsInitialized = true;
				}
				catch ( Exception ex )
				{
					// If config file does not contain logging configuration, try to create some internal defaults.
					try
					{
						TraceOptions AllOptions = (TraceOptions)Enum.GetValues(typeof(TraceOptions)).OfType<int>().Aggregate(0, (X, Y) => X | Y);

						//DictionaryConfigurationSource InternalConfigurationSource = new DictionaryConfigurationSource();
						//LoggingSettings Settings = new LoggingSettings();
						//InternalConfigurationSource.Add(LoggingSettings.SectionName, Settings);
						//Settings.TracingEnabled = true;

						//// Our custom log formatter that converts extended dictionary data to XML for database storage.
						//CustomFormatterData LogFormatter = new CustomFormatterData();
						//LogFormatter.Name = typeof(EXSLogFormatter).Name;
						//LogFormatter.Type = typeof(EXSLogFormatter);
						//Settings.Formatters.Add(LogFormatter);

						//// The entity trace listener, which records log entries to the database using a POCO entity layer.
						//CustomTraceListenerData EntityListener = new CustomTraceListenerData();
						//EntityListener.Name = typeof(EXSLogEntityTraceListener).Name;
						//EntityListener.Filter = LocalConfig.LogDatabaseLevel;
						//EntityListener.Type = typeof(EXSLogEntityTraceListener);
						//EntityListener.ListenerDataType = typeof(CustomTraceListenerData);
						//EntityListener.TraceOutputOptions = AllOptions;
						//EntityListener.Formatter = typeof(EXSLogFormatter).Name;
						//Settings.TraceListeners.Add(EntityListener);
						//Settings.SpecialTraceSources.AllEventsTraceSource.TraceListeners.Add(new TraceListenerReferenceData { Name = EntityListener.Name });

						//// A text formatter to create simple email message bodies from log entries.
						//TextFormatterData TextFormatter = new TextFormatterData();
						//TextFormatter.Name = typeof(TextFormatter).Name;
						//TextFormatter.Type = typeof(TextFormatter);
						//TextFormatter.Template = string.Empty;
						//Settings.Formatters.Add(TextFormatter);

						//// The email trace listener, to send email alerts to developers on serious-enough messages.
						//CustomTraceListenerData EmailListener = new CustomTraceListenerData();
						//EmailListener.Name = typeof(EXSLogEmailTraceListener).Name;
						//EmailListener.Filter = LocalConfig.LogEmailLevel;
						//EmailListener.Type = typeof(EXSLogEmailTraceListener);
						//EmailListener.ListenerDataType = typeof(CustomTraceListenerData);
						//EmailListener.TraceOutputOptions = AllOptions;
						//EmailListener.Formatter = typeof(TextFormatter).Name;
						//Settings.TraceListeners.Add(EmailListener);
						//Settings.SpecialTraceSources.AllEventsTraceSource.TraceListeners.Add(new TraceListenerReferenceData { Name = EmailListener.Name });

						//IServiceLocator Container = EnterpriseLibraryContainer.CreateDefaultContainer(InternalConfigurationSource);
						//_AppLogger = Container.GetInstance<LogWriter>();

						IsInitialized = true;
					}
					catch ( Exception ex2 )
					{
						// If both methods of creating configuration failed, throw an exception referencing both prior exceptions.
						Exception E = new Exception("Failed to create an Enterprise Library logging components from config, and failed "
							+ "to create one from hard-coded fallback configuration.", ex2);
						E.Data["ExceptionFromConfig"] = ex;
						E.Data["ExceptionFromHardCode"] = ex2;
						throw E;
					}
				}
			}
		}
		#endregion

		#region Constructor
		/// <summary>
		/// </summary>
		static EXSLogger()
		{
			Initialize();
		}
		#endregion

		#region ExtraEnvironmentInfo
		private const string EXTRA_ENVIRONMENT_INFO = "EXSLogger__EXTRA_ENVIRONMENT_INFO";
		/// <summary>
		/// A place to put additional thread-local (CallContext-local) environment info for
		/// Exception and other logging purposes.
		/// </summary>
		public static Dictionary<string, object> ExtraEnvironmentInfo
		{
			get
			{
				return new Dictionary<string, object>();
				//return ContextStorage.Get<Dictionary<string, object>>(EXTRA_ENVIRONMENT_INFO, () => new Dictionary<string, object>());
			}
		}
		#endregion

		#region BuildBaseEnvironmentalInfo
		private static void DictTryAdd(IDictionary<string, object> dict, string key, Func<object> getInfo)
		{
			try { dict[key] = getInfo.Invoke(); }
			catch ( Exception ex ) { dict[key] = ex; }
		}
		private static void DictTryPopulate<T>(IDictionary<string, object> dict) where T : IExtraInformationProvider, new()
		{
			try { new T().PopulateDictionary(dict); }
			catch ( Exception ex ) { dict[typeof(T).Name] = ex; }
		}
		/// <summary>
		/// Populates a Dictionary with information about our runtime environment.
		/// </summary>
		private static void BuildBaseEnvironmentalInfo(IDictionary<string, object> dict)
		{
			Dictionary<string, object> EnvDict = new Dictionary<string, object>();
			dict["Environment"] = EnvDict;
			//DictTryPopulate<DebugInformationProvider>(EnvDict);
			//DictTryPopulate<ManagedSecurityContextInformationProvider>(EnvDict);
			//DictTryPopulate<UnmanagedSecurityContextInformationProvider>(EnvDict);
			//DictTryAdd(EnvDict, "AssemblyFullNameEntry", () => Assembly.GetEntryAssembly().InvokeUnlessDefault(A => A.FullName, null));
			//DictTryAdd(EnvDict, "AssemblyFullNameExecuting", () => Assembly.GetExecutingAssembly().InvokeUnlessDefault(A => A.FullName, null));
			//DictTryAdd(EnvDict, "AssemblyFullNameCalling", () => Assembly.GetCallingAssembly().InvokeUnlessDefault(A => A.FullName, null));
			DictTryAdd(EnvDict, "CommandLine", () => Environment.CommandLine);
			DictTryAdd(EnvDict, "CurrentDirectory", () => Environment.CurrentDirectory);
			DictTryAdd(EnvDict, "HasShutdownStarted", () => Environment.HasShutdownStarted);
			DictTryAdd(EnvDict, "Is64BitOperatingSystem", () => Environment.Is64BitOperatingSystem);
			DictTryAdd(EnvDict, "Is64BitProcess", () => Environment.Is64BitProcess);
			DictTryAdd(EnvDict, "MachineName", () => Environment.MachineName);
			DictTryAdd(EnvDict, "OSVersion", () => Environment.OSVersion);
			DictTryAdd(EnvDict, "ProcessorCount", () => Environment.ProcessorCount);
			DictTryAdd(EnvDict, "SystemDirectory", () => Environment.SystemDirectory);
			DictTryAdd(EnvDict, "SystemPageSize", () => Environment.SystemPageSize);
			DictTryAdd(EnvDict, "TickCount", () => Environment.TickCount);
			DictTryAdd(EnvDict, "UserDomainName", () => Environment.UserDomainName);
			DictTryAdd(EnvDict, "UserInteractive", () => Environment.UserInteractive);
			DictTryAdd(EnvDict, "UserName", () => Environment.UserName);
			DictTryAdd(EnvDict, "Version", () => Environment.Version);
			DictTryAdd(EnvDict, "WorkingSet", () => Environment.WorkingSet);

			Dictionary<string, object> ConfigDict = new Dictionary<string, object>();
			dict["LogConfig"] = ConfigDict;
			foreach ( PropertyInfo PI in typeof(LocalConfig).GetProperties(BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Static)
				.Where(P => P.CanRead && !P.GetIndexParameters().Any()) )
				DictTryAdd(ConfigDict, PI.Name, () => PI.GetValue(null, null));

			HttpContext HttpCtx = HttpContext.Current;
			if ( HttpCtx != null )
			{
				Dictionary<string, object> HttpDict = new Dictionary<string, object>();
				dict["HttpContext"] = HttpDict;
				DictTryAdd(HttpDict, "RequestServerVariables", () => new NameValueCollection(HttpCtx.Request.ServerVariables));
				DictTryAdd(HttpDict, "RequestQueryString", () => HttpCtx.Request.QueryString);
				DictTryAdd(HttpDict, "RequestForm", () => HttpCtx.Request.Form);
				DictTryAdd(HttpDict, "RequestCookies", () => HttpCtx.Request.Cookies);
			}

			if ( LogExtraInfo != null )
				LogExtraInfo(new object(), new EventArgs());

			lock ( ExtraEnvironmentInfo )
				foreach ( KeyValuePair<string, object> P in ExtraEnvironmentInfo )
					dict[P.Key] = P.Value;
		}
		#endregion

		#region ExceptionSummarySubstitutions
		private static Dictionary<string, string> _ExceptionSummarySubstitutions = null;
		/// <summary>
		/// Exception messages that are much more verbose than necessary for the amount of information
		/// they actually contain, and the corresponding short code strings to be substituted for
		/// them, in the exception summary message.
		/// </summary>
		private static Dictionary<string, string> ExceptionSummarySubstitutions
		{
			get
			{
				if ( _ExceptionSummarySubstitutions == null )
				{
					Dictionary<string, string> D = new Dictionary<string, string>();
					D["Object reference not set to an instance of an object."] = "NULLREF";
					D["Exception of type 'System.Web.HttpException' was thrown."] = "HTTPEX";
					D["Exception of type 'System.Web.HttpUnhandledException' was thrown."] = "HTTPUN";
					D["Exception has been thrown by the target of an invocation."] = "INVOKE";
					D["An error occurred while executing the command definition. See the inner exception for details."] = "ENTCMD";
					D["An error occurred while reading from the store provider's data reader. See the inner exception for details."] = "ENTREAD";
					D["The underlying provider failed on Open."] = "SQLOPEN";
					D["An error occurred while starting a transaction on the provider connection. See the inner exception for details."] = "SQLTRANS";
					D["Timeout expired. The timeout period elapsed prior to completion of the operation or the server is not responding."] = "TIMEOUT";
					D["Timeout expired.  The timeout period elapsed prior to obtaining a connection from the pool.  This may "
						+ "have occurred because all pooled connections were in use and max pool size was reached."] = "SQLPOOL";
					_ExceptionSummarySubstitutions = D;
				}
				return _ExceptionSummarySubstitutions;
			}
		}
		#endregion

		#region LogCore
		/// <summary>
		/// Exception Key
		/// </summary>
		public const string EXCEPTION_KEY = "Exception";
		private static void LogCore(Exception ex, string message, string title, LogSeverity severity,
			IDictionary<string, object> additionalInfo, int priority)
		{
			LogEntry MyLog = new LogEntry();
			MyLog.Message = message;
			MyLog.Title = title;
			MyLog.Severity = severity.ToTraceEventType();
			MyLog.EventId = 0;
			MyLog.Priority = priority;
			MyLog.AppDomainName = LocalConfig.UnifiedDeployment_Application;

			Log MyRealLog = new Log();
			MyRealLog.AutoTruncateStringProperties = true;
			//int Parsed;
			MyRealLog.LogGUID = Guid.NewGuid();
			MyRealLog.AppVersion = LocalConfig.ProductVersion;
			try { MyRealLog.EnvLevel = LocalConfig.UnifiedDeployment_Environment; }
			catch { }
			try
			{
				if ( (HttpContext.Current != null) && (HttpContext.Current.Request != null)
					&& (HttpContext.Current.Request.UserHostAddress != null) )
					try { MyRealLog.UserIPAddress = HttpContext.Current.Request.TrueRemoteAddress().ToString(); }
					catch { MyRealLog.UserIPAddress = HttpContext.Current.Request.UserHostAddress.ToString(); }
			}
			catch { }

			Dictionary<string, object> ExtDictionary;
			if ( additionalInfo != null )
				ExtDictionary = new Dictionary<string, object>(additionalInfo);
			else
				ExtDictionary = new Dictionary<string, object>();
			BuildBaseEnvironmentalInfo(ExtDictionary);
			if ( ex != null )
				ExtDictionary[EXCEPTION_KEY] = ex;
			MyLog.ExtendedProperties = ExtDictionary;

			//MyLog.SetLogEntity(MyRealLog);
			//_AppLogger.Write(MyLog);
		}
		#endregion

		#region Log
		/// <summary>
		/// Log the Message with specified attributes.
		/// </summary>
		/// <param name="message">Message to log.</param>
		/// <param name="title">A brief title for the message.</param>
		/// <param name="severity">Severity of event, used by config filters.</param>
		/// <param name="priority">Message priority, possibly used in config filters.</param>
		/// <param name="additionalInfo">An optional Dictionary of objects to be included in the
		/// logging information.  The public properties of the object will be included in the
		/// log entry.</param>
		static public void Log(string message, string title, LogSeverity severity = LogSeverity.Error,
			IDictionary<string, object> additionalInfo = null, int priority = 1)
		{
			LogCore(null, message, title, severity, additionalInfo, priority);
		}
		/// <summary>
		/// Log an exeception with specified attributes.
		/// </summary>
		/// <param name="ex">Exception to log.</param>
		/// <param name="severity">Severity of event, used by config filters.</param>
		/// <param name="priority">Message priority, possibly used in config filters.</param>
		/// <param name="additionalInfo">An optional Dictionary of objects to be included in the
		/// logging information.  The public properties of the object will be included in the
		/// log entry.</param>
		static public void Log(Exception ex, LogSeverity severity = LogSeverity.Error,
			IDictionary<string, object> additionalInfo = null, int priority = 1)
		{
			int MsgLen = 255;	// typeof(Log).GetProperty("Message").FromAttribute<DataObjectFieldAttribute, int>(A => A.Length, () => 255);
			StringBuilder Msg = new StringBuilder();
			for ( Exception e = ex; (e != null) && (Msg.Length <= MsgLen); e = e.InnerException )
			{
				if ( Msg.Length > 0 )
					Msg.Append(" ==> ");

				// Certain exceptions include common strings that are verbose, but don't provide
				// much information about the actual error.  Replace those with short aliases.
				string M = null;
				if ( ExceptionSummarySubstitutions.TryGetValue(e.Message, out M) )
					Msg.Append("$" + M);
				else
					Msg.Append(e.Message);
			}
			if ( Msg.Length > MsgLen )
			{
				Msg.Length = MsgLen - 3;
				Msg.Append("...");
			}

			LogCore(ex, Msg.ToString(), ex.GetType().Name, severity, additionalInfo, priority);
		}
		#endregion

		#region CreateAdditionalInfoList
		/// <summary>
		/// Helper method to create a Dictionary for extra/additional information
		/// to be logged with the error.  Parameters allow you to insert the
		/// first item into the list easily.
		/// </summary>
		public static IDictionary<string, object> CreateAdditionalItemList(string firstItemKey, object firstItemObject)
		{
			var ToReturn = new Dictionary<string, object>();
			ToReturn.Add(firstItemKey, firstItemObject);
			return ToReturn;
		}
		#endregion
	}
	#endregion
}
