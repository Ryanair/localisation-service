using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Mapping.ByCode.Conformist;
using NHibernate.Mapping.ByCode;
using Translations.Models;


namespace Translations.Maps
{
	public class TransPluralMap : ClassMapping<TransPlural>
	{

		public TransPluralMap()
		{
			Table("Trans_Plural");
			Schema("dbo");
			Lazy(true);
			Id(x => x.TransPluralKey, map => { map.Column("trans_pluralKey"); map.Generator(Generators.Assigned); });
			Property(x => x.PluralDescription, map => map.NotNullable(true));
			Property(x => x.IsActive, map => map.NotNullable(true));
			Property(x => x.HashCurrent, map => map.Column("hashCurrent"));
			ManyToOne(x => x.TransLang, map =>
			{
				map.Column("trans_langKey");
				//map.PropertyRef("TransLangkey");
				map.Cascade(Cascade.None);
			});

			ManyToOne(x => x.Plural, map => { map.Column("pluralKey"); map.Cascade(Cascade.None); });
		}
	}
}
