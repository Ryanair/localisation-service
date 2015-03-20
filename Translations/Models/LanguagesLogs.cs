using System;
using System.Text;
using System.Collections.Generic;
using System.Collections;



namespace Translations.Models {
    
    public class LanguagesLogs {
		public LanguagesLogs() { }
		public virtual long logId { get; set; }
		public virtual string LangKey { get; set; }
		public virtual string LangText { get; set; }
		public virtual bool IsActive { get; set; }
		public virtual string HashKey { get; set; }
		public virtual string UserName { get; set; }
		public virtual string Action { get; set; }
		public virtual DateTime InsertDate { get; set; }

		public virtual string ToHash
		{
			get
			{
				return string.Format("{0}{1}{2}", LangKey, LangText, IsActive);
			}
		}





	}
}
