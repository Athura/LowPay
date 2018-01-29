using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using RTS.Service.Common;

namespace RTS.Service.API
{
	public abstract class ServiceBaseBase
	{
		#region TrapMsgs
		/// <summary>
		/// This method runs a delegate, catches any ExecMsgs raised,
		/// and returns the messages.
		/// </summary>
		/// <param name="toRun">The action to run, wrapped in the exception
		/// handling pattern.</param>
		/// <param name="reThrowInternal">If true (the default), internal errors
		/// will be rethrown, causing the application to abort to the "Our
		/// Apologies" page.  If false, internal errors are not rethrown, and
		/// instead are wrapped in an ExecMsg with the unique Exception ID,
		/// to be used by a developer to track down the error.</param>
		/// <remarks>WARNING: This method RETURNS ExecMsgs.  In general, ExecMsgs should
		/// NOT be returned, but instead should be raised via ExecMsgScope.RaiseMsg,
		/// and caught or captured via ExecMsgScope.ExtractMessages.  Checking a
		/// return value for errors is far too easy for a developer to forget to do,
		/// and the RaiseMsg pattern ensures that this does not happen.  This method
		/// only exists because a standard way of catching various messages, and
		/// handling internal messages, is needed.</remarks>
		public static IEnumerable<ExecMsg> TrapMsgs(Action toRun, bool reThrowInternal = true)
		{
			HashSet<ExecMsg> Msgs = new HashSet<ExecMsg>();
			using ( new ExecMsgScope() )
			{
				try
				{
					ExecMsgScope.ConvertTaggedMsgs(toRun);
				}
				catch ( ExecMsgException ex )
				{
					//ex.Log();
					Msgs.UnionWith(ex.ExecMsgs);
				}
				catch ( ThreadAbortException )
				{
					throw;
				}
				catch ( Exception ex )
				{
					Msgs.Add(new ExecMsg(ExecMsgSeverity.Error, SysMsgCode.InternalError, ex.Message));
					Exception e = ex.InnerException;
					while ( e != null )
					{
						Msgs.Add(new ExecMsg(ExecMsgSeverity.Error, SysMsgCode.InternalError, e.Message));
						e = e.InnerException;
					}
					if ( reThrowInternal )
						throw;
				}
				finally
				{
					Msgs.UnionWith(ExecMsgScope.ExtractMessages());
				}
			}
			return Msgs;
		}
		#endregion
	}
}