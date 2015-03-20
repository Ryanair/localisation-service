using NHibernate;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using Translations.Models;

namespace Translations.Helpers
{
	public static class Core
	{

		#region from BD
		public static SelectList ListLanguages
		{
			get
			{
				using (ISession session = NHibernateSession.OpenSession())
				{
					List<Languages> _langs = session.CreateSQLQuery("exec GetLanguages")
						.AddEntity(typeof(Languages))
						.List<Languages>() as List<Languages>;
					return new SelectList(_langs.Select(x => new { value = x.LangKey, text = x.LangText }), "value", "text");
				}
			}
		}
		public static SelectList ListTags
		{
			get
			{
				using (ISession session = NHibernateSession.OpenSession())
				{
					List<Tags> _tags = session.CreateSQLQuery("exec GetTags")
						.AddEntity(typeof(Tags))
						.List<Tags>() as List<Tags>;
					return new SelectList(_tags.Select(x => new { value = x.TagKey, text = x.TagText }), "value", "text");
				}
			}
		}
		public static SelectList ListPlurals
		{
			get
			{
				using (ISession session = NHibernateSession.OpenSession())
				{
					List<Plural> _plural = session.CreateSQLQuery("exec GetPlural")
						.AddEntity(typeof(Plural))
						.List<Plural>() as List<Plural>;
					return new SelectList(_plural.Select(x => new { value = x.PluralKey, text = x.PluralText }), "value", "text");
				}
			}
		}
		public static bool ValidateTranslation(string key)
		{
			using (ISession session = NHibernateSession.OpenSession())
			{
				Translations.Models.Translations trans = session.CreateSQLQuery("exec ValidateTranslation :key")
					.AddEntity(typeof(Translations.Models.Translations))
					.SetParameter("key", key).UniqueResult<Translations.Models.Translations>();
				return trans != null && !string.IsNullOrEmpty(trans.TransKey);
			}
		}
		#endregion

		#region Custom Lists
		public static SelectList ListExportTypes
		{
			get
			{
				Dictionary<string, string> options = new Dictionary<string, string>() 
				{
					{ "xml", "XML" },
					//{ "json", "Json" },
					//{ "plain", "Plain" },
					
					//{"xls","XLS"},
					//{"xlsx","XLSX"},
					{ "csv", "CSV" }
				};
				return new SelectList(options.Select(x => new { value = x.Key, text = x.Value }), "value", "text", options.First().Key);
			}
		}
		#endregion

		public static User getUser
		{
			get
			{
				User _user = HttpContext.Current.Session[System.Configuration.ConfigurationManager.AppSettings["userSessionKey"].ToString()] as User;
				return _user;
			}
		}

		public static bool LogsActive
		{
			get
			{
				bool active = false;
				return bool.TryParse(System.Configuration.ConfigurationManager.AppSettings["activateLogsOnBD"], out active);
			}
		}

		public static DataTable ConvertToDataTable<T>(this IEnumerable<T> data)
		{
			PropertyDescriptorCollection props =
						   TypeDescriptor.GetProperties(typeof(T));
			DataTable table = new DataTable();
			for (int i = 0; i < props.Count; i++)
			{
				PropertyDescriptor prop = props[i];

				if (!prop.Name.Equals("Languages", StringComparison.InvariantCultureIgnoreCase) &&
						!prop.Name.Equals("Translations", StringComparison.InvariantCultureIgnoreCase) &&
						!prop.Name.Equals("TransPlural", StringComparison.InvariantCultureIgnoreCase) &&
						!prop.Name.Equals("TransArray", StringComparison.InvariantCultureIgnoreCase))
				{
					if (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
						table.Columns.Add(prop.Name, prop.PropertyType.GetGenericArguments()[0]);
					else
						table.Columns.Add(prop.Name, prop.PropertyType);
				}
			}
			object[] values = new object[table.Columns.Count];
			foreach (T item in data)
			{
				int j = 0;
				for (int i = 0; i < values.Length; i++)
				{
				if (!props[i].Name.Equals("Languages", StringComparison.InvariantCultureIgnoreCase) &&
						!props[i].Name.Equals("Translations", StringComparison.InvariantCultureIgnoreCase) &&
						!props[i].Name.Equals("TransPlural", StringComparison.InvariantCultureIgnoreCase) &&
						!props[i].Name.Equals("TransArray", StringComparison.InvariantCultureIgnoreCase))
					{
						values[j] = props[i].GetValue(item);
						j++;
					}
				}
				table.Rows.Add(values);
			}
			return table;
		}

		public static List<string> SplitCSV(string input)
		{
			List<string> line = new List<string>();
			//"(?<=^|,)(\"(?:[^\"]|\"\")*\"|[^,]*)"
			//"(([^,^\'])*(\'.*\')*([^,^\'])*)(,|$)"
			Regex csvSplit = new Regex("(?<=^|,)(\"(?:[^\"]|\"\")*\"|[^,]*)", RegexOptions.Compiled);

			foreach (Match match in csvSplit.Matches(input))
			{
				// \"accept\"

				//line.Add(match.Value.TrimStart(','));

				string cleanVal = match.Value.Replace("\\", "").Replace("\"",string.Empty);

				//	output = '"' + output.Replace("\"", "\"\"") + '"';


				line.Add(cleanVal);
			}
			return line;
		}


	}
}