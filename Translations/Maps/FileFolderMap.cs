using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Mapping.ByCode.Conformist;
using NHibernate.Mapping.ByCode;
using Translations.Models;
namespace Translations.Maps
{
	public class FileFolderMap : ClassMapping<FileFolder>
	{
		public FileFolderMap()
		{
			Table("File_Folder");
			Schema("dbo");
			Lazy(true);
			Id(x => x.FolderLangId, map => map.Generator(Generators.Identity));
			Property(x => x.FolderName, map => map.NotNullable(true));
			Property(x => x.FileName, map => map.NotNullable(true));
			ManyToOne(x => x.Tags, map => { map.Column("tagKey"); map.Cascade(Cascade.None); });
			ManyToOne(x => x.Languages, map => { map.Column("langKey"); map.Cascade(Cascade.None); });
		}
	}
}