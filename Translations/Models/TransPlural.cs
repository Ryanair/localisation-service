using System;
using System.Text;
using System.Collections.Generic;


namespace Translations.Models
{
	public class TransPlural
	{
		public virtual string TransPluralKey { get; set; }
		public virtual TransLang TransLang { get; set; }
		public virtual Plural Plural { get; set; }
		public virtual string PluralDescription { get; set; }
		public virtual bool IsActive { get; set; }

		public virtual string HashCurrent { get; set; }
		public virtual string ToHash
		{
			get
			{
				return string.Format("{0}{1}{2}", TransPluralKey, PluralDescription, IsActive);
			}
		}
	}
}
