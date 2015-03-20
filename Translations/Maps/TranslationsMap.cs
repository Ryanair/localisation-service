using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Mapping.ByCode.Conformist;
using NHibernate.Mapping.ByCode;
using Translations.Models;


namespace Translations.Maps {
    
    
    public class TranslationsMap : ClassMapping<Translations.Models.Translations> {
        
        public TranslationsMap() {
			Schema("dbo");
			Lazy(true);
			Id(x => x.TransKey, map => map.Generator(Generators.Assigned));
			Property(x => x.HashCurrent, map => map.Column("hashCurrent"));
			Property(x => x.CreationDate, map => map.Column("creationDate"));
			Bag(x => x.TransLang, colmap => { colmap.Key(x => x.Column("transKey")); colmap.Inverse(true); }, map => { map.OneToMany(); });
			Bag(x => x.TransTag, colmap => { colmap.Key(x => x.Column("transKey")); colmap.Inverse(true); }, map => { map.OneToMany(); });
        }
    }
}
