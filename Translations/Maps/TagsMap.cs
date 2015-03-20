using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Mapping.ByCode.Conformist;
using NHibernate.Mapping.ByCode;
using Translations.Models;


namespace Translations.Maps {
    
    
    public class TagsMap : ClassMapping<Tags> {
        
        public TagsMap() {
			Schema("dbo");
			Lazy(true);
			Id(x => x.TagKey, map => map.Generator(Generators.Assigned));
			Property(x => x.TagText, map => map.NotNullable(true));
			Property(x => x.IsActive, map => map.NotNullable(true));
			Property(x => x.HashCurrent, map => map.Column("hashCurrent"));
			Bag(x => x.FileFolder, colmap => { colmap.Key(x => x.Column("tagKey")); colmap.Inverse(true); }, map => { map.OneToMany(); });
			Bag(x => x.TransTag, colmap => { colmap.Key(x => x.Column("tagKey")); colmap.Inverse(true); }, map => { map.OneToMany(); });
			
		}
    }
}
