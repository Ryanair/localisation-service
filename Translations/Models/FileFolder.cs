using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Translations.Models
{
	public class FileFolder
	{
		public virtual int FolderLangId { get; set; }
		public virtual string FolderName { get; set; }
		public virtual string FileName { get; set; }
		public virtual Tags Tags { get; set; }
		public virtual Languages Languages { get; set; }

		public virtual string HashCurrent { get; set; }

		public virtual string ToHash
		{
			get
			{
				return string.Format("{0}{1}{2}{3}{4}", FolderLangId, FolderName, FileName, Tags.TagKey, Languages.LangKey);
			}
		}
	}
}