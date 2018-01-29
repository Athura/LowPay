using System;
using System.Collections.Generic;
using System.Linq;

namespace RTS.Service.Common
{
	/// <summary>
	/// An exception representing one or more ExecMsgs.  Used by
	/// ExecMsgScope to allow ExecMsgs meeting severity criteria
	/// to interrupt program execution flow.
	/// </summary>
	[Serializable]
	public class ExecMsgException : Exception
	{
		/// <summary>
		/// ExecMsgs that triggered this exception.
		/// </summary>
		public IEnumerable<ExecMsg> ExecMsgs { get; protected set; }

		#region Constructors
		/// <summary>
		/// </summary>
		public ExecMsgException(IEnumerable<ExecMsg> msgs)
			: base(string.Join(Environment.NewLine, msgs.Select(M => M.ToString())))
		{
			ExecMsgs = msgs;
		}
		/// <summary>
		/// </summary>
		public ExecMsgException(params ExecMsg[] msgs)
			: base(string.Join(Environment.NewLine, msgs.Select(M => M.ToString())))
		{
			ExecMsgs = msgs;
		}
		/// <summary>
		/// </summary>
		public ExecMsgException(Exception innerException, IEnumerable<ExecMsg> msgs)
			: base(string.Join(Environment.NewLine, msgs.Select(M => M.ToString())),
			innerException)
		{
			ExecMsgs = msgs;
		}
		/// <summary>
		/// </summary>
		public ExecMsgException(Exception innerException, params ExecMsg[] msgs)
			: base(string.Join(Environment.NewLine, msgs.Select(M => M.ToString())),
			innerException)
		{
			ExecMsgs = msgs;
		}
		#endregion
	}

}
