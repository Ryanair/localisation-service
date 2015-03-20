using System;
using System.Text;
using System.Collections.Generic;


namespace Translations.Models
{

	public class Translations
	{
		public Translations() { }
		public virtual string TransKey { get; set; }
		public virtual DateTime CreationDate { get; set; }

		private IEnumerable<TransLang> _transLang;
		public virtual IEnumerable<TransLang> TransLang
		{
			get { return this._transLang; }
			set { this._transLang = value; }
		}


		private IEnumerable<TransTag> _transTag;
		public virtual IEnumerable<TransTag> TransTag
		{
			get { return this._transTag; }
			set { this._transTag = value; }
		}

		public virtual string HashCurrent { get; set; }
		public virtual string ToHash
		{
			get
			{
				return string.Format("{0}", TransKey);
			}
		}


	}
}
