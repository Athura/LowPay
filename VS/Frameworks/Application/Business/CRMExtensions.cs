using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;

namespace App.Framework.Business
{
	public static class CRMExtensions
	{
		#region | enum: GetAttribute |
		public static TAttribute GetAttribute<TAttribute>(this Enum value)
			where TAttribute : Attribute
		{
			var type = value.GetType();
			var name = Enum.GetName(type, value);
			return type.GetField(name)
				.GetCustomAttributes(false)
				.OfType<TAttribute>()
				.SingleOrDefault();
		}
		#endregion

		#region | enum: GetDesc |
		public static string GetDesc(this Enum value)
		{
			FieldInfo fi = value.GetType().GetField(value.ToString());

			DescriptionAttribute[] attributes =
				(DescriptionAttribute[])fi.GetCustomAttributes(
				typeof(DescriptionAttribute),
				false);

			if ( attributes != null && attributes.Length > 0 )
				return attributes[0].Description;
			else
				return value.ToString();
		}
		#endregion

		#region | List<string>: Contains |
		/// <summary>
		/// Returns a value indicating whether a specified substring occurs within this string using case option.
		/// </summary>
		public static bool Contains(this List<string> source, string toCheck, StringComparison comp)
		{
			bool IsFound = false;
			if ( !toCheck.IsEmpty() )
			{
				foreach ( string s in source )
				{
					if ( s.Equals(toCheck, comp) )
					{
						IsFound = true;
						break;
					}
				}
			}
			return IsFound;
		}
		#endregion

		#region | string: Contains |
		/// <summary>
		/// Returns a value indicating whether a specified substring occurs within this string using case option.
		/// </summary>
		public static bool Contains(this string source, string toCheck, StringComparison comp)
		{
			if ( string.IsNullOrEmpty(source) || string.IsNullOrEmpty(toCheck) )
				return false;
			return source.IndexOf(toCheck, comp) >= 0;
		}
		#endregion

		#region | string: EnumTryParse |
		/// <summary>
		/// Tries to parse a string as an enum value.  Returns parsed value, or
		/// default value if parsing fails.
		/// </summary>
		/// <typeparam name="TEnum">Enum value type into which to parse.</typeparam>
		/// <param name="toParse">String to parse as enum.</param>
		/// <param name="defValue">An optional alternate default value, instead of default(TEnum).</param>
		public static TEnum EnumTryParse<TEnum>(this string toParse, TEnum defValue = default(TEnum))
			 where TEnum : struct
		{
			TEnum Val = defValue;
			if ( toParse == null )
				return Val;
			Enum.TryParse<TEnum>(toParse, true, out Val);
			return Val;
		}
		#endregion

		#region | string: IsDate |
		/// <summary>
		/// Determines if a date is valid. 
		/// </summary>
		public static bool IsDate(this string value)
		{
			try
			{
				DateTime tmp = Convert.ToDateTime(value);
			}
			catch
			{
				return false;
			}
			return true;
		}
		#endregion

		#region | string: IsEmpty |
		/// <summary>
		/// Determines if a string is empty 
		/// </summary>
		public static bool IsEmpty(this string value)
		{
			return string.IsNullOrWhiteSpace(value);
		}
		#endregion

		#region | object: IsNull |
		/// <summary>
		/// Determines if an object is Null
		/// </summary>
		public static bool IsNull(this object value)
		{
			return value == null;
		}
		#endregion

		#region | string: Replace |
		/// <summary>
		/// Replace a value in a string with comparison options
		/// </summary>
		public static string Replace(this string str, string oldValue, string newValue, StringComparison comparisonType)
		{
			int prevPos = 0;
			string retval = str;

			// find the first occurence of oldValue
			int pos = retval.IndexOf(oldValue, comparisonType);

			while ( pos > -1 )
			{
				// remove oldValue from the string
				retval = retval.Remove(pos, oldValue.Length);

				// insert newValue in it's place
				retval = retval.Insert(pos, newValue);

				// check if oldValue is found further down
				prevPos = pos + newValue.Length;
				pos = retval.IndexOf(oldValue, prevPos, comparisonType);
			}

			return retval;
		}
		#endregion

		#region | string: IsNumeric |
		/// <summary>
		/// Is the string numeric
		/// </summary>
		public static bool IsNumeric(this string value)
		{
			double Parsed;
			return Double.TryParse(value,
				System.Globalization.NumberStyles.Any,
				System.Globalization.NumberFormatInfo.InvariantInfo,
				out Parsed);
		}
		#endregion

		#region | string: ToEnum |
		public static T ToEnum<T>(this string value)
		{
			//return (T)Enum.Parse(typeof(T), value);

			Contract.Requires(typeof(T).IsEnum);
			Contract.Requires(value != null);
			Contract.Requires(Enum.IsDefined(typeof(T), value));
			return (T)Enum.Parse(typeof(T), value);
		}
		#endregion

		#region | string: ToTitleCase |
		/// <summary>
		/// Convert string to title case XXXXX => Xxxxxx
		/// </summary>
		public static string ToTitleCase(this string str)
		{
			CultureInfo cultureInfo = Thread.CurrentThread.CurrentCulture;
			TextInfo textInfo = cultureInfo.TextInfo;
			return textInfo.ToTitleCase(str);
		}
		#endregion

		#region | DateTime: ToMinDate |
		public static void ToMinDate(this DateTime value)
		{
			value = DateTime.MinValue;
		}
		#endregion

		#region | DateTime: ToStringDate |
		public static string ToStringDate(this DateTime value)
		{
			return value == DateTime.MinValue ? "" : value.ToString("MM/dd/yyyy");
		}
		#endregion

		#region | DateTime: ToStringDateTime |
		public static string ToStringDateTime(this DateTime value)
		{
			return value == DateTime.MinValue ? "" : value.ToString("MM/dd/yy hh:mm tt");
		}
		#endregion

		#region | object: ToStringOrEmpty |
		/// <summary>
		/// Convert an object or null to a string, never returning
		/// a null string.
		/// </summary>
		public static string ToStringOrEmpty(this object o)
		{
			try
			{
				if ( o == null )
					return string.Empty;
				return (o.ToString() ?? string.Empty).Trim();
			}
			catch
			{
				return string.Empty;
			}
		}
		#endregion

		#region | IEnumerable: ToHashSet |
		/// <summary>
		/// Wraps the enumerable in a HashSet.
		/// </summary>
		public static HashSet<T> ToHashSet<T>(this IEnumerable<T> toHash)
		{
			return new HashSet<T>(toHash);
		}
		/// <summary>
		/// Wraps the enumerable in a HashSet.
		/// </summary>
		public static HashSet<T> ToHashSet<T>(this IEnumerable<T> toHash, IEqualityComparer<T> comparer)
		{
			return new HashSet<T>(toHash, comparer);
		}
		#endregion

		#region | DataTable: ColumnRemove |
		public static void ColumnRemove(this DataTable dt, string columnName)
		{
			try
			{
				dt.Columns.Remove(columnName);
			}
			catch
			{
				// Do nothing.....
			}
		}
		#endregion

		#region | DataTable: ColumnRename |
		public static void ColumnRename(this DataTable dt, string oldColumnName, string newColumnName)
		{
			try
			{
				dt.Columns[oldColumnName].ColumnName = newColumnName;
				dt.AcceptChanges();
			}
			catch
			{
				// Do nothing.....
			}
		}
		#endregion

		#region | DataTable: ToList |
		/// <summary>
		/// Return a list of T types from the DataTable.
		/// </summary>
		/// <remarks>
		/// Any Property in the type with a matching column name in the
		/// DataTable is populated in the object list returned.
		/// </remarks>
		/// <typeparam name="T">The type of object to return.</typeparam>
		/// <param name="dt">Source DataTable</param>
		/// <param name="objAction">Optional action to be performed on each object after
		/// it has been created, but before it is added to the list.</param>
		/// <returns>List of T objects.</returns>
		public static List<T> ToList<T>(this DataTable dt, Action<T> objAction = null) where T : new()
		{
			List<T> TR = new List<T>();
			
			// Get mapping of properties to columns that we want to convert.
			var PI = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
				.Where(P => P.CanWrite && !P.GetIndexParameters().Any() && dt.Columns.Contains(P.Name))
				.Select(P => new Tuple<PropertyInfo, int>(P, dt.Columns.IndexOf(P.Name)))
				.ToList();

			foreach ( DataRow DR in dt.Rows )
			{
				T NewObj = new T();
				PI.ForEach((P) =>
				{
					// Got to do some strange stuff for DBNull values in the DataTable
					if ( P.Item1.PropertyType.IsGenericType && P.Item1.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>) )
					{
						dynamic objValue = System.Activator.CreateInstance(P.Item1.PropertyType);
						objValue = Convert.IsDBNull(DR[P.Item2]) ? null : DR[P.Item2];
						P.Item1.SetValue(NewObj, (object)objValue, null);
					}
					else
					{
						P.Item1.SetValue(NewObj, Convert.IsDBNull(DR[P.Item2]) ? null : DR[P.Item2], null);
					}
				});

				// Perform custom action on the new object
				if ( objAction != null )
					objAction.Invoke(NewObj);

				TR.Add(NewObj);
			}

			return TR;
		}
		#endregion

		#region | DataTable: ToXml |
		/// <summary>
		/// Convert a DataTable to xml string
		/// </summary>
		/// <param name="table">Table to convert</param>
		/// <returns>Eml string</returns>
		public static string ToXml(this DataTable table)
		{
			if ( table == null )
			{
				return null;
			}
			else
			{
				using ( var sw = new StringWriter() )
				using ( var tw = new XmlTextWriter(sw) )
				{
					// Must set name for serialization to succeed
					//table.TableName = @"MyTable";

					// --
					// http://bit.ly/a75DK7

					tw.Formatting = Formatting.Indented;
					tw.WriteStartDocument();
					tw.WriteStartElement(@"data");

					((IXmlSerializable)table).WriteXml(tw);

					tw.WriteEndElement();
					tw.WriteEndDocument();

					tw.Flush();
					tw.Close();
					sw.Flush();

					return sw.ToString();
				}
			}
		}
		#endregion
	}
}
