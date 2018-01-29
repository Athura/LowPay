using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using App.Framework.Business;
using Lowpay.Framework.Business;
using RTS.Service.Common;

namespace RTS.Service.Business
{
	public class ServiceBase
	{
		#region FormatDataToXml - commented
		//public string FormatDataToXml(DataSet ds)
		//{
		//    string Xml = "";
		//    if ( ds.Tables.Count > 0 )
		//        if ( ds.Tables[0].Rows.Count > 0 )
		//            Xml = ds.GetXml();
		//    return Xml;
		//}
		#endregion

		#region RaiseEXSMessages
		/// <summary>
		/// Converts the ExecutionMessageCollection into ExecMsg objects, then uses
		/// ExecMsgScope.RaiseMsg to raise the messages.
		/// </summary>
		/// <param name="msgs">List of EXS Messages.</param>
		protected void RaiseEXSMessages(ExecutionMessageCollection msgs)
		{
			var Errors = LogEXSMessages(msgs);
			ExecMsgScope.RaiseMsg(Errors);
		}
		#endregion

		#region LogEXSMessages
		/// <summary>
		/// Converts the ExecutionMessageCollection into ExecMsg objects, then uses
		/// ExecMsgScope.RaiseMsg to raise the messages.
		/// </summary>
		/// <param name="msgs">List of EXS Messages.</param>
		protected IEnumerable<ExecMsg> LogEXSMessages(ExecutionMessageCollection msgs)
		{
			var Errors = ConvertEXSMessages(msgs);
			EXSLogger.Log(string.Join(Environment.NewLine, Errors.Select(E => E.ToString())),
				"EXS ExecutionMessages",
				Errors.Any(E => E.Severity >= ExecMsgSeverity.Problem) ? LogSeverity.Warning : LogSeverity.Verbose);
			return Errors;
		}
		#endregion

		#region ConvertEXSMessages
		/// <summary>
		/// Convert errors into ExecMsg objects.
		/// </summary>
		/// <param name="msgs">List of EXS Messages.</param>
		private IEnumerable<ExecMsg> ConvertEXSMessages(ExecutionMessageCollection msgs)
		{
			var TR = new List<ExecMsg>();
			msgs.OfType<ExecutionMessage>().ToList()
				.ForEach((M) =>
				{
					if ( M is ErrorMessage )
						TR.Add(new ExecMsg(ExecMsgSeverity.Error, SysMsgCode.ExecutionErrorMessageRaised, M.Text.Replace("PLEASE NOTE: ", "").Replace("[EXS Error] ", "")));
					else
						TR.Add(new ExecMsg(ExecMsgSeverity.Warning, SysMsgCode.ExecutionWarningMessageRaised, M.Text.Replace("PLEASE NOTE: ", "")));
				});
			return TR;
		}
		#endregion
	}
}
