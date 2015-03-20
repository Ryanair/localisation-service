using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Mapping.ByCode.Conformist;
using NHibernate.Mapping.ByCode;
using Translations.Models;


namespace Translations.Maps
{
	public class TransTagMap : ClassMapping<TransTag>
	{

		public TransTagMap()
		{
			Table("Trans_Tag");
			Schema("dbo");
			Lazy(true);
			Id(x => x.TransTagKey, map => { map.Column("trans_tagKey"); map.Generator(Generators.Assigned); });
			Property(x => x.IsActive, map => map.NotNullable(true));
			Property(x => x.HashCurrent, map => map.Column("hashCurrent"));
			ManyToOne(x => x.Tags, map => { map.Column("tagKey"); map.Cascade(Cascade.None); });
			ManyToOne(x => x.Translations, map => { map.Column("transKey"); map.Cascade(Cascade.None); });
		}
	}
}
