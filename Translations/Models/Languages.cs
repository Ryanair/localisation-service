using System;
using System.Text;
using System.Collections.Generic;
using System.Collections;



namespace Translations.Models {
    
    public class Languages {
		public Languages() { }
		public virtual string LangKey { get; set; }
		public virtual string LangText { get; set; }
		public virtual bool IsActive { get; set; }

		private IEnumerable<TransLang> _transLang;
		public virtual IEnumerable<TransLang> TransLang
		{
			get { return this._transLang; }
			set { this._transLang = value; }
		}
		private IEnumerable<FileFolder> _fileFolder;
		public virtual IEnumerable<FileFolder> FileFolder
		{
			get { return this._fileFolder; }
			set { this._fileFolder = value; }
		}

		public virtual string HashCurrent { get; set; }

		public virtual string ToHash
		{
			get
			{
				return string.Format("{0}{1}{2}", LangKey, LangText, IsActive);
			}
		}

	}
}
