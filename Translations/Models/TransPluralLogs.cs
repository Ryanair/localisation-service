using System;
using System.Text;
using System.Collections.Generic;


namespace Translations.Models
{
	public class TransPluralLogs
	{
		public TransPluralLogs() { }
		public virtual long logId { get; set; }

		public virtual string TransPluralKey { get; set; }
		public virtual string TransLangKey { get; set; }
		public virtual string PluralKey { get; set; }
		public virtual string PluralDescription { get; set; }
		public virtual bool IsActive { get; set; }
		public virtual string HashKey { get; set; }
		public virtual string UserName { get; set; }
		public virtual string Action { get; set; }
		public virtual DateTime InsertDate { get; set; }

		public virtual string ToHash
		{
			get
			{
				return string.Format("{0}{1}", TransPluralKey, IsActive);
			}
		}
	}
}
