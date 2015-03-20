using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Mapping.ByCode.Conformist;
using NHibernate.Mapping.ByCode;
using Translations.Models;

namespace Translations.Maps
{
	public class TransLangCsvMap : ClassMapping<TransLangCsv>
	{

		public TransLangCsvMap()
		{
			Table("Trans_Lang");
			Schema("dbo");
			Lazy(true);
			Id(x => x.TransLangKey, map => { map.Column("trans_langKey"); map.Generator(Generators.Assigned); });
			Property(x => x.TransDescription, map => map.NotNullable(true));
			ManyToOne(x => x.Languages, map => { map.Column("langKey"); map.Cascade(Cascade.None); });
			ManyToOne(x => x.Translations, map => { map.Column("transKey"); map.Cascade(Cascade.None); });
			Bag(x => x.TransPlural, colmap => { colmap.Key(x => x.Column("trans_langKey")); colmap.Inverse(true); }, map => { map.OneToMany(); });
			Bag(x => x.TransArray, colmap => { colmap.Key(x => x.Column("trans_langKey")); colmap.Inverse(true); }, map => { map.OneToMany(); });
		}
	}
}
