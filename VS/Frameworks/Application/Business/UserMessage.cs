using System;

namespace App.Framework.Business
{
	///<summary>
	/// <para>Represents a class used for displaying messages via a user interface.</para>
	/// </summary>
	///<remarks>
	/// <para>Use UserMessage to convey that the current execution may be finished but in an altered state.</para>
	/// <para>A UserMessage should be used for display purposes <b>only,</b>
	/// and should not be used to drive code direction.</para>
	/// <seealso cref="ErrorMessage"/>
	/// </remarks>
	// Greg Wood - Friday, June 18, 2000
	// This class is a barebones implementation and probably needs to be extended
	// to fit additional scenarios as needed.
	[Serializable]
	public class UserMessage : ExecutionMessage
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <example>
		/// <code>
		/// ExecutionMessageCollection ToReturn = new ExecutionMessageCollection();
		/// ToReturn.Add(new UserMessage("Item not found."));
		/// </code>
		/// </example>
		/// <param name="text">Message text string.</param>
		/// <param name="originatorType">The class type that created the message object</param>
		/// <param name="suppressOnSave">Specifies whether this message should be suppress during a save</param>
		/// <param name="messageTypeCode">Message Type</param>
		public UserMessage(string text, Type originatorType = null, bool suppressOnSave = false, string messageTypeCode = "")
			: base(text, originatorType, suppressOnSave, messageTypeCode)
		{
		}
	}
}
