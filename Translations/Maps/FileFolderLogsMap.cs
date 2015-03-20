using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Mapping.ByCode.Conformist;
using NHibernate.Mapping.ByCode;
using Translations.Models;
namespace Translations.Maps
{
	public class FileFolderLogsMap : ClassMapping<FileFolderLogs>
	{
		public FileFolderLogsMap()
		{
			Table("File_FolderLogs");
			Schema("dbo");
			Lazy(true);
			Id(x => x.logId, map => map.Generator(Generators.Identity));
			Property(x => x.FolderLangId, map => map.Column("folderLangId"));
			Property(x => x.FolderName, map => map.Column("folderName"));
			Property(x => x.FileName, map => map.Column("fileName"));
			Property(x => x.HashKey, map => map.Column("hashKey"));
			Property(x => x.UserName, map => map.Column("userName"));
			Property(x => x.Action, map => map.Column("action"));
			Property(x => x.TagKey, map => map.Column("tagKey"));
			Property(x => x.LangKey, map => map.Column("langKey"));
			Property(x => x.InsertDate, map => map.Column("insertDate"));
		}
	}
}