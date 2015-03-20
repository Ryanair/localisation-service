using System;
using System.Text;
using System.Collections.Generic;


namespace Translations.Models
{
	public class Plural
	{
		public Plural() { }
		public virtual string PluralKey { get; set; }
		public virtual string PluralText { get; set; }
		public virtual bool IsActive { get; set; }

		private IEnumerable<TransPlural> _transPlural;
		public virtual IEnumerable<TransPlural> TransPlural
		{
			get { return this._transPlural; }
			set { this._transPlural = value; }
		}
	}


}
