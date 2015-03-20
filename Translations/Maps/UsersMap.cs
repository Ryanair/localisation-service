using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Mapping.ByCode.Conformist;
using NHibernate.Mapping.ByCode;
using Translations.Models;


namespace Translations.Maps {
    
    
    public class UsersMap : ClassMapping<Users> {
        
        public UsersMap() {
			Schema("dbo");
			Lazy(true);
			Id(x => x.Username, map => map.Generator(Generators.Assigned));
			Property(x => x.Name, map => map.NotNullable(true));
			Property(x => x.Surname, map => map.NotNullable(true));
			Property(x => x.Password, map => map.NotNullable(true));
        }
    }
}
