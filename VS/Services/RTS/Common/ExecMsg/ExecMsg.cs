using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace RTS.Service.Common
{
	/// <summary>
	/// Represents a run-time condition, in a format suitable for both
	/// human and machine consumption.  Can represent a wide range of
	/// conditions, including different types of fatal errors, and different
	/// types of non-fatal warning and other messages.
	/// </summary>
	[Serializable]
	[DataContract(IsReference = false, Namespace = "")]
	public class ExecMsg : IComparable
	{
		#region MessageID
		/// <summary>
		/// A numerical ID identifying this message (though some parts of
		/// the message text may differ).  Allows ExecMsgs to be machine-consumable
		/// (i.e. automating response to them is possible).
		/// </summary>
		[DataMember(EmitDefaultValue = false, IsRequired = false)]
		public int MessageID { get; protected set; }
		#endregion

		#region Severity
		/// <summary>
		/// The severity of the message.  Higher severity values are more severe,
		/// and imply escalating response; severities of Error or above will
		/// generally abort execution.
		/// </summary>
		[DataMember(EmitDefaultValue = false, IsRequired = false)]
		public ExecMsgSeverity Severity { get; protected set; }
		#endregion

		#region Message
		/// <summary>
		/// Gets the human-readable text content of this message.
		/// </summary>
		[DataMember(EmitDefaultValue = false, IsRequired = false)]
		public string Message { get; protected set; }
		#endregion

		#region Constructors
		/// <summary>
		/// </summary>
		public ExecMsg(ExecMsgSeverity msgSeverity, SysMsgCode msgID, params object[] msgParams) :
			this(msgSeverity, (int)msgID, msgParams)
		{ }
		/// <summary>
		/// </summary>
		public ExecMsg(ExecMsgSeverity msgSeverity, int msgID, params object[] msgParams)
		{
			MessageID = msgID;
			Severity = msgSeverity;
			Message = (msgParams != null)
				? string.Format(SysMsgLoader.GetMessageContent(MessageID), msgParams).Trim()
				: SysMsgLoader.GetMessageContent(MessageID).Trim();
		}
		public ExecMsg(ExecMsgSeverity msgSeverity, ExecMsg msgOld)
		{
			MessageID = msgOld.MessageID;
			Severity = msgSeverity;
			Message = msgOld.Message;
		}
		protected ExecMsg()
		{
		}
		#endregion

		#region Equality Checks
		/// <summary>
		/// Gets a HashCode for this ExecMsg.  The Severity, MessageID, and
		/// Message text are all taken into account.
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode()
		{
			return (Severity.ToString() + "\0" + MessageID.ToString() + "\0" + Message).GetHashCode();
		}
		/// <summary>
		/// Determines if two messages are the equal.  They are considered
		/// equal if they have the same Severity, MessageID, and Message text.
		/// </summary>
		public override bool Equals(object obj)
		{
			if ( (obj == null) || !(obj is ExecMsg) )
				return false;
			ExecMsg msg = (ExecMsg)obj;
			return Severity.Equals(msg.Severity) && MessageID.Equals(msg.MessageID) && Message.Equals(msg.Message);
		}
		#endregion

		#region ToString
		/// <summary>
		/// Returns the ExecMsg converted to a human-readable string.
		/// This includes the Severity, MessageID, and Message text.
		/// </summary>
		public override string ToString()
		{
			string TR = string.Format("{0} #{1}: {2}", Severity.ToString(), MessageID, Message);
			return TR;
		}
		#endregion

		#region Parse
		/// <summary>
		/// Try to parse a series of ExecMsgs from a string.  The ExecMsgs
		/// must be in the form created by ExecMsg.ToString(), one per line
		/// (newline-delimited).
		/// </summary>
		/// <param name="msgs">A string containing ToString()'ed ExecMsgs to parse.</param>
		/// <returns>The list of ExecMsgs that were sucessfully parsed.  Extraneous
		/// text is ignored.</returns>
		public static List<ExecMsg> Parse(string msgs)
		{
			List<ExecMsg> Msgs = new List<ExecMsg>();
			foreach ( Match M in Regex.Matches(msgs, @"^\s*(\S+)\s+#(\d+):\s+(.*)$", RegexOptions.Multiline) )
			{
				string[] Groups = M.Groups.OfType<Group>().Select(G => G.Value)
					.Reverse().Take(3).Reverse().ToArray();
				if ( Groups.Length < 3 )
					continue;
				ExecMsgSeverity Sev;
				if ( !Enum.TryParse<ExecMsgSeverity>(Groups[0], true, out Sev) )
					continue;
				int MsgID = 0;
				if ( !int.TryParse(Groups[1], out MsgID) )
					continue;
				Msgs.Add(new ExecMsg()
				{
					Severity = Sev,
					MessageID = MsgID,
					Message = Groups[2]
				});
			}
			return Msgs;
		}
		#endregion

		#region Comparison
		/// <summary>
		/// Compares two ExecMsgs for display sorting purposes.
		/// Messages with higher severity are first, then messages
		/// with lower MessageID's.  Finally, if multiple messages
		/// with the same ID are in the list, they are compared
		/// by the Message text, so they always appear in the same
		/// canonical order.
		/// </summary>
		public int CompareTo(object obj)
		{
			if ( (obj == null) || !(obj is ExecMsg) )
				return -1;
			ExecMsg msg = (ExecMsg)obj;
			int C = -Severity.CompareTo(msg.Severity);
			if ( C != 0 )
				return C;
			C = MessageID.CompareTo(msg.MessageID);
			if ( C != 0 )
				return C;
			return Message.CompareTo(msg.Message);
		}
		#endregion
	}
}
