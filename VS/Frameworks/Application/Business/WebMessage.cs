using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Framework.Business
{
	public class WebMessage
	{
		public string Text { get; set; }
		public string Severity { get; set; }
		public string MessageTypeCode { get; set; }
		public WebMessage()
		{ }
		public WebMessage(string text)
		{
			this.Text = text;
		}
		public WebMessage(string text, string messageTypeCode)
		{
			this.Text = text;
			this.MessageTypeCode = messageTypeCode;
		}
	}

	public static class WebMessageExtensions
	{
		public static string StringifyMessages(this IEnumerable<ExecutionMessage> msgs)
		{
			return string.Join(System.Environment.NewLine, msgs.Select(m => CleanText(m.Text)));
		}

		public static string StringifyMessages(this ExecutionMessageCollection msgs)
		{
			return msgs.ToArray().StringifyMessages();
		}

		public static IEnumerable<WebMessage> WebMessages(this ExecutionMessageCollection msgs)
		{
			return msgs.ToArray().WebMessages();
		}

		public static IEnumerable<WebMessage> WebMessages(this IEnumerable<ExecutionMessage> msgs)
		{
			return msgs.Select(m => new WebMessage { Text = CleanText(m.Text), Severity = GetSeverity(m), MessageTypeCode = (m.MessageTypeCode) });
		}

		private static string GetSeverity(ExecutionMessage msg)
		{
			if ( msg is ErrorMessage )
				return "Error";
			else if ( msg is WarningMessage )
				return "Warning";
			else
				return "User";
		}

		private static string CleanText(string text)
		{
			return text.Replace("PLEASE NOTE:", string.Empty).Trim();
		}

	}

}
