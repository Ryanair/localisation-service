using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;

namespace Translations.Helpers
{
	public class CsvExport<T> where T : class
	{
		public List<T> Objects;

		public CsvExport(List<T> objects)
		{
			Objects = objects;
		}

		public string Export()
		{
			return Export(true);
		}

		public string Export(bool includeHeaderLine)
		{

			StringBuilder sb = new StringBuilder();
			//Get properties using reflection.
			IList<PropertyInfo> propertyInfos = typeof(T).GetProperties();

			if (includeHeaderLine)
			{
				//add header line.
				foreach (PropertyInfo propertyInfo in propertyInfos)
				{
					if (!propertyInfo.Name.Equals("Languages", StringComparison.InvariantCultureIgnoreCase) &&
						!propertyInfo.Name.Equals("Translations", StringComparison.InvariantCultureIgnoreCase) &&
						!propertyInfo.Name.Equals("TransPlural", StringComparison.InvariantCultureIgnoreCase) &&
						!propertyInfo.Name.Equals("TransArray", StringComparison.InvariantCultureIgnoreCase))
						sb.Append(propertyInfo.Name).Append(",");
				}
				sb.Remove(sb.Length - 1, 1).AppendLine();
			}

			//add value for each property.
			foreach (T obj in Objects)
			{
				foreach (PropertyInfo propertyInfo in propertyInfos)
				{
					if (!propertyInfo.Name.Equals("Languages", StringComparison.InvariantCultureIgnoreCase) &&
						!propertyInfo.Name.Equals("Translations", StringComparison.InvariantCultureIgnoreCase) &&
						!propertyInfo.Name.Equals("TransPlural", StringComparison.InvariantCultureIgnoreCase) &&
						!propertyInfo.Name.Equals("TransArray", StringComparison.InvariantCultureIgnoreCase))
						sb.Append(MakeValueCsvFriendly(propertyInfo.GetValue(obj, null), propertyInfo.Name.Equals("TransDescription", StringComparison.InvariantCultureIgnoreCase)
							|| propertyInfo.Name.Equals("Few", StringComparison.InvariantCultureIgnoreCase)
							|| propertyInfo.Name.Equals("Many", StringComparison.InvariantCultureIgnoreCase)
							|| propertyInfo.Name.Equals("One", StringComparison.InvariantCultureIgnoreCase)
							|| propertyInfo.Name.Equals("Other", StringComparison.InvariantCultureIgnoreCase)
							|| propertyInfo.Name.Equals("Two", StringComparison.InvariantCultureIgnoreCase)
							|| propertyInfo.Name.Equals("Zero", StringComparison.InvariantCultureIgnoreCase))).Append(",");
				}
				sb.Remove(sb.Length - 1, 1).AppendLine();
			}

			return sb.ToString();
		}

		//export to a file.
		public void ExportToFile(string path)
		{
			File.WriteAllText(path, Export());
		}

		//export as binary data.
		public byte[] ExportToBytes()
		{
			return Encoding.UTF8.GetBytes(Export());
		}

		//get the csv value for field.
		private string MakeValueCsvFriendly(object value,bool parse = false)
		{
			if (value == null) return "";
			if (value is Nullable && ((INullable)value).IsNull) return "";

			if (value is DateTime)
			{
				if (((DateTime)value).TimeOfDay.TotalSeconds == 0)
					return ((DateTime)value).ToString("yyyy-MM-dd");
				return ((DateTime)value).ToString("yyyy-MM-dd HH:mm:ss");
			}
			string output = value.ToString();

			if (parse)
				output = '"' + output.Replace("\"", "\"\"") + '"';

			return output;

		}
	}

}