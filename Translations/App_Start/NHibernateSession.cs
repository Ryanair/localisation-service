using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Mapping.ByCode;
using System.Reflection;

namespace Translations
{
	public class NHibernateSession
	{
		public static ISession OpenSession()
		{
			string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["TransConnection"].ToString();

			var configuration = new Configuration();
			configuration.SetProperty(NHibernate.Cfg.Environment.ProxyFactoryFactoryClass, typeof(NHibernate.Bytecode.DefaultProxyFactoryFactory).AssemblyQualifiedName);
			configuration.SetProperty(NHibernate.Cfg.Environment.Dialect, typeof(NHibernate.Dialect.MsSql2008Dialect).AssemblyQualifiedName);
			configuration.SetProperty(NHibernate.Cfg.Environment.ConnectionString, connectionString);
			configuration.SetProperty(NHibernate.Cfg.Environment.FormatSql, "true");

			configuration.AddMapping(GetMappings());

			ISessionFactory sessionFactory = configuration.BuildSessionFactory();
			return sessionFactory.OpenSession();
		}

		public static IStatelessSession OpenStatelessSession()
		{
			string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["TransConnection"].ToString();

			var configuration = new Configuration();
			configuration.SetProperty(NHibernate.Cfg.Environment.ProxyFactoryFactoryClass, typeof(NHibernate.Bytecode.DefaultProxyFactoryFactory).AssemblyQualifiedName);
			configuration.SetProperty(NHibernate.Cfg.Environment.Dialect, typeof(NHibernate.Dialect.MsSql2008Dialect).AssemblyQualifiedName);
			configuration.SetProperty(NHibernate.Cfg.Environment.ConnectionString, connectionString);
			configuration.SetProperty(NHibernate.Cfg.Environment.FormatSql, "true");

			configuration.AddMapping(GetMappings());

			ISessionFactory sessionFactory = configuration.BuildSessionFactory();
			return sessionFactory.OpenStatelessSession();
		}


		private static HbmMapping GetMappings()
		{
			var mapper = new ModelMapper();

			mapper.AddMappings(Assembly.GetAssembly(typeof(Translations.Maps.LanguagesMap)).GetExportedTypes());
			mapper.AddMappings(Assembly.GetAssembly(typeof(Translations.Maps.PluralMap)).GetExportedTypes());
			mapper.AddMappings(Assembly.GetAssembly(typeof(Translations.Maps.TagsMap)).GetExportedTypes());
			mapper.AddMappings(Assembly.GetAssembly(typeof(Translations.Maps.TransLangMap)).GetExportedTypes());
			mapper.AddMappings(Assembly.GetAssembly(typeof(Translations.Maps.TranslationsMap)).GetExportedTypes());
			mapper.AddMappings(Assembly.GetAssembly(typeof(Translations.Maps.TransPluralMap)).GetExportedTypes());
			mapper.AddMappings(Assembly.GetAssembly(typeof(Translations.Maps.TransTagMap)).GetExportedTypes());
			mapper.AddMappings(Assembly.GetAssembly(typeof(Translations.Maps.TransArrayMap)).GetExportedTypes());
			mapper.AddMappings(Assembly.GetAssembly(typeof(Translations.Maps.UsersMap)).GetExportedTypes());
			mapper.AddMappings(Assembly.GetAssembly(typeof(Translations.Maps.FileFolderMap)).GetExportedTypes());
			mapper.AddMappings(Assembly.GetAssembly(typeof(Translations.Maps.TransLangCsvMap)).GetExportedTypes());

			mapper.AddMappings(Assembly.GetAssembly(typeof(Translations.Maps.LanguagesLogsMap)).GetExportedTypes());
			//mapper.AddMappings(Assembly.GetAssembly(typeof(Translations.Maps.PluralLogsMap)).GetExportedTypes());
			mapper.AddMappings(Assembly.GetAssembly(typeof(Translations.Maps.TagsLogsMap)).GetExportedTypes());
			mapper.AddMappings(Assembly.GetAssembly(typeof(Translations.Maps.TransLangLogsMap)).GetExportedTypes());
			mapper.AddMappings(Assembly.GetAssembly(typeof(Translations.Maps.TranslationsLogsMap)).GetExportedTypes());
			mapper.AddMappings(Assembly.GetAssembly(typeof(Translations.Maps.TransPluralLogsMap)).GetExportedTypes());
			mapper.AddMappings(Assembly.GetAssembly(typeof(Translations.Maps.TransTagLogsMap)).GetExportedTypes());
			mapper.AddMappings(Assembly.GetAssembly(typeof(Translations.Maps.TransArrayLogsMap)).GetExportedTypes());
			mapper.AddMappings(Assembly.GetAssembly(typeof(Translations.Maps.FileFolderLogsMap)).GetExportedTypes());
			
			return mapper.CompileMappingForAllExplicitlyAddedEntities();
		}
	}
}