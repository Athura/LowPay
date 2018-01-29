using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Web;
using System.Web.Services.Protocols;
using System.Xml.Linq;
using RTS.Service.Common;

namespace RTS.Service.API
{
	public interface IWebServiceAuth
	{
		string username { get; set; }
		string password { get; set; }
	}

	public class SOAPAuth : SoapHeader, IWebServiceAuth
	{
		public string username { get; set; }
		public string password { get; set; }
	}

	[DataContract(Namespace = ServiceAttributes.WEB_SERVICE_NAMESPACE, IsReference = true)]
	public class WCFAuth : IWebServiceAuth
	{
		[DataMember]
		public string username { get; set; }

		[DataMember]
		public string password { get; set; }
	}
}
