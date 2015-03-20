using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Mapping.ByCode.Conformist;
using NHibernate.Mapping.ByCode;
using Translations.Models;


namespace Translations.Maps
{
	public class LanguagesMap : ClassMapping<Languages>
	{
		public LanguagesMap()
		{
			Schema("dbo");
			Lazy(true);
			Id(x => x.LangKey, map => map.Generator(Generators.Assigned));
			Property(x => x.LangText, map => map.NotNullable(true));
			Property(x => x.IsActive, map => map.NotNullable(true));
			Property(x => x.HashCurrent, map => map.Column("hashCurrent"));
			Bag(x => x.TransLang, colmap => { colmap.Key(x => x.Column("langKey")); colmap.Inverse(true); }, map => { map.OneToMany(); });
			Bag(x => x.FileFolder, colmap => { colmap.Key(x => x.Column("langKey")); colmap.Inverse(true); }, map => { map.OneToMany(); });
		}
	}
}
