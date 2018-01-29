using System;
using System.Runtime.Serialization;

namespace App.Framework.Business
{
	#region ExecutionMessage
	/// <summary>
	/// An abstract base class used for displaying messages via a user interface.
	/// </summary>
	/// <remarks>
	/// An ExecutionMessage should be used for display purposes only, and should not be used to drive
	/// code direction.
	/// <seealso cref="WarningMessage"/>
	/// <seealso cref="ErrorMessage"/>
	/// </remarks>
	// Greg Wood - Friday, June 18, 2000
	// This class is a barebones implementation and probably needs to be extended
	// to fit additional scenarios as needed.
	[Serializable]
    [KnownType(typeof(ErrorMessage))]
    [KnownType(typeof(WarningMessage))]
    [KnownType(typeof(UserMessage))]
    [DataContract]
	public abstract class ExecutionMessage
	{
		private string _Text;
		private Type _OriginatorType;
		private bool _SuppressOnSave = false;
		private string _MessageTypeCode = string.Empty;

		#region Constructors
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="text">Message text string.</param>
		/// <param name="originatorType">The class type that created the message object</param>
		/// <param name="suppressOnSave">Specifies whether this message should be suppress during a save</param>
		/// <param name="messageTypeCode">Message type</param>
		public ExecutionMessage(string text, Type originatorType = null, bool suppressOnSave = false, string messageTypeCode = "")
		{
			this._Text = text;
			this._OriginatorType = originatorType;
			this._SuppressOnSave = suppressOnSave;
			this._MessageTypeCode = messageTypeCode;
		}
		#endregion

		#region Property Text
		/// <summary>
		/// Text string for this message object.
		/// </summary>
        [DataMember]
		public string Text
		{
			get
			{
				return this._Text;
			}
			set
			{
				this._Text = value;
			}
		}
		#endregion

		#region Property OriginatorType
		public Type OriginatorType
		{
			get
			{
				return this._OriginatorType;
			}
			set
			{
				this._OriginatorType = value;
			}
		}
		#endregion

		#region Property SuppressOnSave
        [DataMember]
        public bool SuppressOnSave
		{
			get
			{
				return this._SuppressOnSave;
			}
			set
			{
				this._SuppressOnSave = value;
			}
		}
		#endregion

		#region Property MessageTypeCode
		public string MessageTypeCode
		{
			get { return this._MessageTypeCode; }
			set { this._MessageTypeCode = value; }
		}
		#endregion
	}
	#endregion
}
