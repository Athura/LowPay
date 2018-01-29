using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text.RegularExpressions;

namespace RTS.Service.Common
{
	/// <summary>
	/// A scope with which to receive ExecMsgs raised by code, and
	/// also providing a static interface for raising ExecMsgs.
	/// </summary>
	public class ExecMsgScope : IDisposable
	{
		#region Fields/Properties
		/// <summary>
		/// The minimum severity level at which a message is escalated to an
		/// exception, if there is no scope to catch it, or if it bubbles out
		/// of scope.
		/// </summary>
		public const ExecMsgSeverity MinThrowOutOfScopeSeverity = ExecMsgSeverity.Problem;
		/// <summary>
		/// The minimum severity level at which a message is escalated to an
		/// exception, regardless of whether a scope is available to receive it.
		/// </summary>
		public const ExecMsgSeverity MinThrowImmediatelySeverity = ExecMsgSeverity.Error;
		/// <summary>
		/// The scope that is one stack layer below this one.
		/// </summary>
		protected ExecMsgScope OuterContext { get; set; }
		/// <summary>
		/// ExecMsg's stored within this scope.
		/// </summary>
		protected HashSet<ExecMsg> Messages;
		#endregion

		#region Property Current
		/// <summary>
		/// Returns the current scope.
		/// </summary>
		protected static ExecMsgScope Current
		{
			get { return CallContext.GetData(typeof(ExecMsgScope).FullName) as ExecMsgScope; }
			set { CallContext.SetData(typeof(ExecMsgScope).FullName, value); }
		}
		#endregion

		#region Constructor
		/// <summary>
		/// Creates a new message scope.  This scope becomes
		/// the current scope.  Use inside a using(...) clause.
		/// </summary>
		public ExecMsgScope()
		{
			OuterContext = Current;
			Current = this;

			Messages = new HashSet<ExecMsg>();
		}
		#endregion

		#region Dispose - IDisposable
		/// <summary>
		/// Pops a scope off the stack.
		/// </summary>
		public void Dispose()
		{
			Current = OuterContext;
			RaiseMsg(Messages);
		}
		#endregion

		#region ExtractMessages
		/// <summary>
		/// Extract messages from the current scope, based on criteria specified.
		/// </summary>
		public static IEnumerable<ExecMsg> ExtractMessages(Func<ExecMsg, bool> where)
		{
			ExecMsgScope Ctx = Current;
			if ( Ctx != null )
			{
				IEnumerable<ExecMsg> Msgs = Ctx.Messages.Where(where);
				Ctx.Messages = new HashSet<ExecMsg>(Ctx.Messages.Except(Msgs));
				return Msgs;
			}
			return new ExecMsg[0];
		}

		/// <summary>
		/// Extract all messages from current scope.
		/// </summary>
		/// <returns></returns>
		public static IEnumerable<ExecMsg> ExtractMessages()
		{
			return ExtractMessages(X => true);
		}
		#endregion

		#region ThrowMsgs
		/// <summary>
		///  Throw an error if any of the messages are above the Minimum Severity
		///  Threshold (which is currently 'error').
		/// </summary>
		protected static void ThrowMsgs(IEnumerable<ExecMsg> msgs)
		{
			IEnumerable<ExecMsg> ToThrow = msgs.Where(M => M.Severity >= MinThrowOutOfScopeSeverity);
			if ( ToThrow.Any() )
				throw new ExecMsgException(ToThrow);
		}
		#endregion

		#region RaiseMsg
		/// <summary>
		/// Raise one or more messages.
		/// </summary>
		public static void RaiseMsg(IEnumerable<ExecMsg> Msgs)
		{
			ExecMsgScope Ctx = Current;
			if ( Ctx == null )
			{
				ThrowMsgs(Msgs);
				return;
			}

			HashSet<ExecMsg> ToThrow = new HashSet<ExecMsg>();
			HashSet<ExecMsg> ToHold = new HashSet<ExecMsg>();
			foreach ( ExecMsg Msg in Msgs )
				if ( Msg.Severity >= MinThrowImmediatelySeverity )
					ToThrow.Add(Msg);
				else
					ToHold.Add(Msg);
			Ctx.Messages.UnionWith(ToHold);
			ThrowMsgs(ToThrow);
		}

		/// <summary>
		/// Raise one or more messages.
		/// </summary>
		public static void RaiseMsg(params ExecMsg[] msgs)
		{
			RaiseMsg((IEnumerable<ExecMsg>)msgs);
		}

		/// <summary>
		/// Raise a single message, using specified criteria for the
		/// new message.
		/// </summary>
		/// <param name="severity">Severity of message.</param>
		/// <param name="msgID">MessageCode</param>
		/// <param name="msgParams">Optional parameters used with the message content (string)
		/// that is defined for this message code.</param>
		public static void RaiseMsg(ExecMsgSeverity severity, SysMsgCode msgID, params object[] msgParams)
		{
			RaiseMsg(new ExecMsg(severity, msgID, msgParams));
		}
		#endregion

		#region ConvertTaggedMsgs
		/// <summary>
		/// Certain external components, such as data stores or web services, may throw
		/// error or exception conditions that are converted by .NET into undifferentiated
		/// Exceptions.  To allow them to raise ExecMsgs, we've provided a special string
		/// tagging system; this method detects those string tags and converts the exceptions
		/// into the appropriate ExecMsgs, if present.
		/// </summary>
		/// <param name="toRun">The code to execute with ExecMsg tag support.  If a tagged
		/// exception is thrown by this code, it will be converted to an ExecMsg and raised
		/// into the current ExecMsgScope automatically.</param>
		public static void ConvertTaggedMsgs(Action toRun)
		{
			try
			{
				toRun.Invoke();
			}
			catch ( ExecMsgException )
			{
				// Pass-through any ExecMsgExceptions, which are already in the correct format.
				throw;
			}
			catch ( Exception ex )
			{
				// If any unexpected internal exception happens while processing the
				// original exception, make sure we don't lose the original exception;
				// it will be attached to the processing error message.
				List<ExecMsg> Msgs = new List<ExecMsg>();
				try
				{
					// Regex for the special tag to identify errors that are meant to be
					// converted to ExecMsgs.  This is because some external components,
					// such as web services or data stores, may be unable to throw ExecMsgs
					// directly through the channel we're using to communicate with them.
					Regex Rx = new Regex(Regex.Escape(@"{{ExecMsg:") + @"(\S+)"
						+ Regex.Escape(@":") + @"(\S+)" + Regex.Escape(@":")
						+ @"(.*?)" + Regex.Escape(@"}}"));

					// Loop through the exception stack (recurse into inner exceptions) and
					// process each for ExecMsgs.
					for ( ; ex != null; ex = ex.InnerException )
						foreach ( Match M in Rx.Matches(ex.Message).OfType<Match>().Where(M => M.Success) )
						{
							// Separate out the groups from the regex match.  Sometimes it seems that special
							// groups (such as the "whole match" group) get added to the start of the groups
							// list, so we're only interested in the last 3.  There should always be at least
							// these three groups.
							Group[] SelectedGroups = M.Groups.OfType<Group>().Where(G => G.Value != null).ToArray();
							if ( SelectedGroups.Length < 3 )
								throw new Exception("Incorrect number of groups in " + (M.Value ?? string.Empty));
							SelectedGroups = SelectedGroups.Skip(SelectedGroups.Count() - 3).ToArray();

							// Try to parse out the message severity, as either a case-insensitive name from
							// the enum, or as an integer equivalent.  Throw an exception on an invalid value.
							ExecMsgSeverity Sev;
							if ( !Enum.TryParse(SelectedGroups[0].Value.Trim(), true, out Sev) )
							{
								int SevValue;
								if ( int.TryParse(SelectedGroups[0].Value.Trim(), out SevValue)
									&& Enum.IsDefined(typeof(ExecMsgSeverity), SevValue) )
									Sev = (ExecMsgSeverity)SevValue;
								else
									throw new Exception("Invalid severity level in " + (M.Value ?? string.Empty));
							}

							// Try to parse out the message ID, as either a case-insensitive name from
							// the enum, or as an integer equivalent.  Throw an exception on an invalid value.
							SysMsgCode MsgCode;
							if ( !Enum.TryParse(SelectedGroups[1].Value.Trim(), true, out MsgCode) )
							{
								int MsgCodeValue;
								if ( int.TryParse(SelectedGroups[1].Value.Trim(), out MsgCodeValue)
									&& Enum.IsDefined(typeof(ExecMsgSeverity), MsgCodeValue) )
									MsgCode = (SysMsgCode)MsgCodeValue;
								else
									throw new Exception("Invalid SysMsgCode in " + (M.Value ?? string.Empty));
							}

							// Add the message to the list of messages to be raised.
							Msgs.Add(new ExecMsg(Sev, MsgCode, SelectedGroups[2].Value ?? string.Empty));
						}
				}
				catch ( Exception ey )
				{
					throw new Exception("Exception while processing an exception for ExecMsg tags: " + ey.ToString(), ex);
				}

				// If we were able to transform any exceptions into ExecMsgs, then we
				// assume that any other content in the exception is just noise from rethrowing
				// and rewrapping, and only raise the messages.  If we didn't find any messages,
				// then continue to treat this as an internal error and re-raise it.
				if ( (Msgs != null) && Msgs.Any() )
					ExecMsgScope.RaiseMsg(Msgs);
				else
					throw;
			}
		}
		#endregion
	}
}
