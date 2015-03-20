using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Mapping.ByCode.Conformist;
using NHibernate.Mapping.ByCode;
using Translations.Models;


namespace Translations.Maps
{
	public class TransArrayMap : ClassMapping<TransArray>
	{
		public TransArrayMap()
		{
			Schema("dbo");
			Lazy(true);
			Id(x => x.TransArrayId, map => map.Generator(Generators.Identity));
			Property(x => x.ArrayDescription, map => map.NotNullable(true));
			Property(x => x.IsActive, map => map.NotNullable(true));
			Property(x => x.HashCurrent, map => map.Column("hashCurrent"));
			ManyToOne(x => x.TransLang, map =>
			{
				map.Column("trans_langKey");
				//map.PropertyRef("TransLangkey");
				map.Cascade(Cascade.None);
			});
		}
	}
}
