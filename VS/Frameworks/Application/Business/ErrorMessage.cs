using System;

namespace App.Framework.Business
{
	/// <summary>
	/// Represents a class used for displaying messages via a user interface.
	/// </summary>
	/// <remarks>
	/// Use ErrorMessage to convey that the current execution may not be finished in its current state.
	/// <para>An ErrorMessage should be used for display purposes <b>only,</b>
	/// and should not be used to drive code direction.</para>
	/// <seealso cref="WarningMessage"/>
	/// </remarks>
	// Greg Wood - Friday, June 18, 2000
	// This class is a barebones implementation and probably needs to be extended
	// to fit additional scenarios as needed.
	[Serializable]
	public class ErrorMessage : ExecutionMessage
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <example>
		/// <code>
		/// ExecutionMessageCollection ToReturn = new ExecutionMessageCollection();
		/// ToReturn.Add(new ErrorMessage("Registrant ID not found."));
		/// </code>
		/// </example>
		/// <param name="text">Message text string.</param>
		/// <param name="originatorType">The class type that created the message object</param>
		/// <param name="suppressOnSave">Specifies whether this message should be suppress during a save</param>
		/// <param name="messageTypeCode">Message type</param>
		public ErrorMessage(string text, Type originatorType = null, bool suppressOnSave = false, string messageTypeCode = "")
			: base(text, originatorType, suppressOnSave, messageTypeCode)
		{
			this.Text = "PLEASE NOTE: " + this.Text;
		}
	}
}
