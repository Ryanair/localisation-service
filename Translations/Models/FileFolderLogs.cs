using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Translations.Models
{
	public class FileFolderLogs
	{
		public FileFolderLogs() { }
		public virtual long logId { get; set; }
		public virtual int FolderLangId { get; set; }
		public virtual string FolderName { get; set; }
		public virtual string FileName { get; set; }
		public virtual string HashKey { get; set; }
		public virtual string UserName { get; set; }
		public virtual string Action { get; set; }
		public virtual string TagKey { get; set; }
		public virtual string LangKey { get; set; }
		public virtual DateTime InsertDate { get; set; }

		public virtual string ToHash
		{
			get
			{
				return string.Format("{0}{1}{2}{3}{4}", FolderLangId, FolderName, FileName, TagKey, LangKey);
			}
		}
	}
}