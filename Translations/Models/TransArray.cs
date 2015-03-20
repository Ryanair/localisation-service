using System;
using System.Text;
using System.Collections.Generic;
using System.Web.Mvc;


namespace Translations.Models {
    
    public class TransArray {
		public TransArray() { }
		public virtual int TransArrayId { get; set; }
		public virtual TransLang TransLang { get; set; }
		public virtual string ArrayDescription { get; set; }
		public virtual bool IsActive { get; set; }

		public virtual string HashCurrent { get; set; }
		public virtual string ToHash
		{
			get
			{
				return string.Format("{0}{1}{2}", TransArrayId, ArrayDescription, IsActive);
			}
		}
    }
}
