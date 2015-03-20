using System;
using System.Text;
using System.Collections.Generic;


namespace Translations.Models {
    
    public class TransTag {
		public virtual string TransTagKey { get; set; }
		public virtual Tags Tags { get; set; }
		public virtual Translations Translations { get; set; }
		public virtual bool IsActive { get; set; }
		public virtual string HashCurrent { get; set; }
		public virtual string ToHash
		{
			get
			{
				return string.Format("{0}{1}", TransTagKey, IsActive);
			}
		}
    }
}
