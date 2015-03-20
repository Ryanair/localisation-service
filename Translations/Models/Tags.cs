using System;
using System.Text;
using System.Collections.Generic;


namespace Translations.Models {
    
    public class Tags {
        public Tags() { }
		public virtual string TagKey { get; set; }
		public virtual string TagText { get; set; }
		public virtual bool IsActive { get; set; }

		private IEnumerable<FileFolder> _fileFolder;
		public virtual IEnumerable<FileFolder> FileFolder
		{
			get { return this._fileFolder; }
			set { this._fileFolder = value; }
		}

		private IEnumerable<TransTag> _transTag;
		public virtual IEnumerable<TransTag> TransTag
		{
			get { return this._transTag; }
			set { this._transTag = value; }
		}

		public virtual string HashCurrent { get; set; }

		public virtual string ToHash
		{
			get {
				return string.Format("{0}{1}{2}", TagKey, TagText, IsActive);
			}
		}


    }
}
