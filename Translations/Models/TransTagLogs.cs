using System;
using System.Text;
using System.Collections.Generic;


namespace Translations.Models {

	public class TransTagLogs
	{
		public TransTagLogs() { }
		public virtual long logId { get; set; }
		public virtual string TransTagKey { get; set; }
		public virtual string TagKey { get; set; }
		public virtual string TransKey { get; set; }
		public virtual bool IsActive { get; set; }
		public virtual string HashKey { get; set; }
		public virtual string UserName { get; set; }
		public virtual string Action { get; set; }
		public virtual DateTime InsertDate { get; set; }

		public virtual string ToHash
		{
			get
			{
				return string.Format("{0}{1}", TransTagKey, IsActive);
			}
		}
    }
}
