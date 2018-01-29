using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Lowpay.Framework.Business
{
    [Serializable]
    [DataContract(IsReference = false)]
    public class Log : ILogEntity
    {
        #region LogGUID
        /// <summary>
        /// Globally unique identifier by which the log entry
        /// is to be identified.
        /// </summary>
        [DataMember]
        public Guid LogGUID { get; set; }
        #endregion

        #region AssociatedLogEntry
        /// <summary>
        /// The EntLib LogEntry that's associated with this Log entity.  The Log
        /// entity is stored in LogEntry's ExtendedProperties in a special "hidden"
        /// key, so this allows us to keep track of the reverse link.
        /// </summary>
        public LogEntry AssociatedLogEntry { get; set; }
        #endregion

        #region Formatted Messages
        /// <summary>
        /// Additional info formatted as text (XML).
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string FormattedMessage { get; set; }
        /// <summary>
        /// The full XML detail message, in compressed
        /// Bromide format.
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public byte[] BromideFullDetail { get; set; }
        #endregion

        #region Product-Specific Properties
        /// <summary>
        /// Version of the app in which the entry was logged.
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string AppVersion { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string EnvLevel { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public int? sysEventID { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public int? UserBaseID { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string UserIPAddress { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string DeviceID { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string AppRoleName { get; set; }
        #endregion

        #region Enterprise Library Standard Fields
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Guid ActivityId { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string AppDomainName { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public ICollection<string> Categories { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public int EventId { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string MachineName { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string ManagedThreadName { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string Message { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public int Priority { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string ProcessId { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string ProcessName { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Guid? RelatedActivityId { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string Severity { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime TimeStamp { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string Title { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string Win32ThreadId { get; set; }

        public TraceEventType Severity_Enum
        {
            get
            {
                return (TraceEventType)Enum.Parse(typeof(TraceEventType), this.Severity);
            }
            set
            {
                this.Severity = value.ToString();
            }
        }
        #endregion

        #region CloneLog
        private static List<PropertyInfo> ClonableFields = typeof(Log)
            .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy)
            .Where(P => P.CanRead && P.CanWrite && P.GetCustomAttributes<DataMemberAttribute>().Any() && !P.GetIndexParameters().Any())
            .ToList();
        /// <summary>
        /// Create a clone of the log entry.  For simplicity, uses
        /// DataContracts to create the clone.
        /// </summary>
        public ILogEntity CloneLog()
        {
            var Cloned = new Log();
            foreach (var P in ClonableFields)
                P.SetValue(Cloned, P.GetValue(this));
            return Cloned;
        }
        #endregion

        #region PopulateFields
        private static List<Tuple<PropertyInfo, PropertyInfo>> PopulateFieldsMapping = typeof(Log)
            .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy)
            .Where(P => P.CanWrite && P.GetCustomAttributes<DataMemberAttribute>().Any() && !P.GetIndexParameters().Any())
            .Select(P => Tuple.Create(typeof(LogEntry).GetProperty(P.Name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy), P))
            .Where(T => (T.Item1 != null) && T.Item1.CanRead && !T.Item1.GetIndexParameters().Any() && (T.Item1.PropertyType == T.Item2.PropertyType))
            .ToList();
        /// <summary>
        /// Populate the real log entity from the enterprise library
        /// log entry type.  For simplicity, just copy properties that
        /// are DataMembers of Log and match the source in name and
        /// type exactly.
        /// </summary>
        public void PopulateFields(LogEntry entLibEntry)
        {
            foreach (var M in PopulateFieldsMapping)
                M.Item2.SetValue(this, M.Item1.GetValue(entLibEntry));

            this.AssociatedLogEntry = entLibEntry;

            // Copy in corresponding fields.
            this.Severity_Enum = entLibEntry.Severity;
        }
        #endregion

		public bool AutoTruncateStringProperties { get; set; }

	}
}
