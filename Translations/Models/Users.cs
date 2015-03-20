using System;
using System.Text;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace Translations.Models {
    
    public class Users {

		[Required(ErrorMessage = "User Name Required")]
        public virtual string Username { get; set; }
		[Required(ErrorMessage = "Name Required")]
        public virtual string Name { get; set; }
		[Required(ErrorMessage = "SurName Required")]
        public virtual string Surname { get; set; }
		[Required(ErrorMessage = "Password Required")]
        public virtual string Password { get; set; }
    }
}
