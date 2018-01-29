using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lowpay.Framework.Business
{
	//
	// Summary:
	//     This interface describes the minimum interface necessary for an entity to effectively
	//     be used as a "log entry" entity, for storing LogEntry data into the database,
	//     as received from EntLib. An actual entity should have the necessary properties
	//     to store any relevant fields from LogEntry, as well as any custom properties
	//     relevant to its use.
	public interface ILogEntity
	{
		//
		// Summary:
		//     An identifier that uniquely identifies this log entry across all environments
		//     and all products. A new unique ID will be assigned by the LogController when
		//     log messages are created.
		Guid LogGUID { get; set; }
		//
		// Summary:
		//     A field that will hold an XML string containing all of the serialized extended
		//     and environmental information for the log entry.
		string FormattedMessage { get; set; }

		//
		// Summary:
		//     Create a "shallow" copy of this entity, with all all "primitive" (i.e. column-bound)
		//     fields copied from the source object (same as with EntityCopyFieldsFrom()) but
		//     no navigation properties copied.
		//
		// Returns:
		//     Cloned entity.
		ILogEntity CloneLog();
		//
		// Summary:
		//     Map any applicable fields from the EntLib built-in LogEntry to this entity's
		//     corresponding properties.
		void PopulateFields(LogEntry entLibEntry);
	}

	//
	// Summary:
	// Defines a method to populate an System.Collections.Generic.IDictionary`2 with
	// helpful diagnostic information.
	public interface IExtraInformationProvider
	{
		//
		// Summary:
		//     Populates an System.Collections.Generic.IDictionary`2 with helpful diagnostic
		//     information.
		//
		// Parameters:
		//   dict:
		//     Dictionary containing extra information used to initialize the Microsoft.Practices.EnterpriseLibrary.Logging.ExtraInformation.IExtraInformationProvider
		//     instance
		void PopulateDictionary(IDictionary<string, object> dict);
	}

}
