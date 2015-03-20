using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Mapping.ByCode.Conformist;
using NHibernate.Mapping.ByCode;
using Translations.Models;


namespace Translations.Maps
{
	public class PluralMap : ClassMapping<Plural>
	{

		public PluralMap()
		{
			Schema("dbo");
			Lazy(true);
			Id(x => x.PluralKey, map => map.Generator(Generators.Assigned));
			Property(x => x.PluralText, map => map.NotNullable(true));
			Property(x => x.IsActive, map => map.NotNullable(true));
			Bag(x => x.TransPlural, colmap => { colmap.Key(x => x.Column("pluralKey")); colmap.Inverse(true); }, map => { map.OneToMany(); });
		}
	}
}
