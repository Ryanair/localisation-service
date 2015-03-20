using System;
using System.Text;
using System.Collections.Generic;


namespace Translations.Models
{
	public class TransLang
	{
		public virtual string TransLangKey { get; set; }
		public virtual Languages Languages { get; set; }
		public virtual Translations Translations { get; set; }
		public virtual string TransDescription { get; set; }
		public virtual bool IsActive { get; set; }
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

		public virtual string HashCurrent { get; set; }

		public virtual string ToHash
		{
			get
			{
				return string.Format("{0}{1}{2}", TransLangKey, TransDescription, IsActive);
			}
		}

	}
}
