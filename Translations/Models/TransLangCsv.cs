using System;
using System.Text;
using System.Collections.Generic;
using NHibernate;


namespace Translations.Models
{
	public class TransLangCsv
	{
		public virtual string TransLangKey { get; set; }
		public virtual Languages Languages { get; set; }
		public virtual Translations Translations { get; set; }

		private string _language;
		public virtual string Language
		{
			get { return Languages != null ? Languages.LangKey : this._language; }
			set { this._language = value; }
		}

		public virtual string TransDescription { get; set; }

		private IEnumerable<TransPlural> _transPlural;
		public virtual IEnumerable<TransPlural> TransPlural
		{
			get { return this._transPlural; }
			set { this._transPlural = value; }
		}
		private IEnumerable<TransArray> _transArray;
		public virtual IEnumerable<TransArray> TransArray
		{
			get { return this._transArray; }
			set { this._transArray = value; }
		}


		public virtual string Few { get; set; }
		public virtual string Many { get; set; }
		public virtual string One { get; set; }
		public virtual string Other { get; set; }
		public virtual string Two { get; set; }
		public virtual string Zero { get; set; }

		private DateTime _creationDate;
		public virtual DateTime CreationDate
		{
			get { return this.Translations != null ? this.Translations.CreationDate : this._creationDate; }
			set { this._creationDate = value; }
		}

		//public virtual string Array
		//{
		//	get
		//	{
		//		string arrayDesc = string.Empty;

		//		foreach (TransArray item in TransArray)
		//		{
		//			if (item.IsActive)
		//				arrayDesc = arrayDesc + item.ArrayDescription + ";";
		//		}

		//		return arrayDesc;
		//	}
		//}

	}
}
