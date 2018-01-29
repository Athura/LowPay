using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using RTS.Service.Business;
using RTS.Service.Common;

namespace RTS.Service.API
{
	[DataContract(Namespace = "", IsReference = false)]
	public class ServiceResponse
	{
		[DataMember(IsRequired = false, EmitDefaultValue = false)]
		public IEnumerable<ExecMsg> ExecMsgs { get; set; }
	}

	[DataContract(Namespace = "", IsReference = false)]
	public class ServiceResponse<T> : ServiceResponse
	{
		[DataMember(IsRequired = false, EmitDefaultValue = false)]
		public T ReturnValue { get; set; }

		public static ServiceResponse<T> Create(Func<T> populate)
		{
			ServiceResponse<T> Response = new ServiceResponse<T>();
			Response.ExecMsgs = ServiceBaseBase.TrapMsgs(() => Response.ReturnValue = populate.Invoke(), false);
			return Response;
		}
	}

	[DataContract(Namespace = "", IsReference = false)]
	public class ServiceResponseVoid : ServiceResponse
	{
		public static ServiceResponseVoid Create(Action toExecute)
		{
			ServiceResponseVoid Response = new ServiceResponseVoid();
			Response.ExecMsgs = ServiceBaseBase.TrapMsgs(() => toExecute.Invoke(), false);
			return Response;
		}
	}
}