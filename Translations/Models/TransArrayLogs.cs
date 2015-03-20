using System;
using System.Text;
using System.Collections.Generic;
using System.Web.Mvc;


namespace Translations.Models
{

	public class TransArrayLogs
	{
		public TransArrayLogs() { }
		public virtual long logId { get; set; }

		public virtual int TransArrayId { get; set; }
		public virtual string TransLangKey { get; set; }
		public virtual string ArrayDescription { get; set; }
		public virtual bool IsActive { get; set; }
		public virtual string HashKey { get; set; }
		public virtual string UserName { get; set; }
		public virtual string Action { get; set; }
		public virtual DateTime InsertDate { get; set; }
		public virtual string ToHash
		{
			get
			{
				return string.Format("{0}{1}{2}", TransArrayId, ArrayDescription, IsActive);
			}
		}

	}
}
