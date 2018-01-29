
namespace RTS.Service.Common
{
	/// <summary>
	/// Indicates a level of severity for an ExecMsg.  These are comparable,
	/// i.e. it is possible to check for Severity &gt;= Error or (Severity &gt; Info)
	/// &amp;&amp; (Severity &lt;= Warning).  Higher severity implies a more serious
	/// issue, requiring more immediate intervention, or aborting the process,
	/// while lower severity indicates messages that are more informational and
	/// less critical.
	/// </summary>
	public enum ExecMsgSeverity
	{
		/// <summary>
		/// A problem has occurred, and code execution needs to be aborted
		/// immediately; raising an Error throws an ExecMsgException, which
		/// can be caught by an appropriate catch{ } handler.
		/// </summary>
		Error = 1000,

		/// <summary>
		/// A problem has occurred, and code execution can continue, but the
		/// problem should be addressed later.  If this message is caught in
		/// an ExecMsgScope, extracted, and displayed, then, like a Warning,
		/// it does NOT cause any code abort.  However, if it is not dealt with
		/// programmatically (if it escapes from any scope) it becomes an
		/// Exception.
		/// </summary>
		Problem = 500,

		/// <summary>
		/// A problem has occurred, but code execution was able to continue,
		/// and the user should be made aware of the issue, but no further
		/// intervention is required if the results of the operation are as
		/// expected.
		/// </summary>
		Warning = 100,

		/// <summary>
		/// No problem has occurred, but the user is being asked to confirm
		/// an operation before it continues.
		/// </summary>
		Question = 20,

		/// <summary>
		/// Some informational message to be displayed to the user, but does
		/// not imply a problem, and does not require any intervention.  Success
		/// is already implied by an absence of messages, so these are only for
		/// unexpected conditions.
		/// </summary>
		Info = 10,

		///// <summary>
		///// Internal informational messages... not used...?
		///// </summary>
		//Debug = 1
	}
}
