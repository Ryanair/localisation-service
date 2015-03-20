using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Mapping.ByCode.Conformist;
using NHibernate.Mapping.ByCode;
using Translations.Models;


namespace Translations.Maps
{
	public class TransArrayLogsMap : ClassMapping<TransArrayLogs>
	{
		public TransArrayLogsMap()
		{
			Schema("dbo");
			Lazy(true);
			Id(x => x.logId, map => map.Generator(Generators.Identity));
			Property(x => x.TransArrayId, map => map.Column("transArrayId"));
			Property(x => x.TransLangKey, map => map.Column("trans_langKey"));
			Property(x => x.ArrayDescription, map => map.Column("arrayDescription"));
			Property(x => x.IsActive, map => map.Column("isActive"));
			Property(x => x.HashKey, map => map.Column("hashKey"));
			Property(x => x.UserName, map => map.Column("userName"));
			Property(x => x.Action, map => map.Column("action"));
			Property(x => x.InsertDate, map => map.Column("insertDate"));
		}
	}
}
