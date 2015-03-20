using NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Translations.Models;

namespace Translations.Controllers
{
	public class LanguagesController : Controller
	{
		[Translations.Filters.AuthorizeUser]
		public ActionResult Index()
		{
			using (ISession session = NHibernateSession.OpenSession())
			{
				var languages = session.QueryOver<Languages>().List<Languages>();
				return View(languages);
			}
		}

		[Translations.Filters.AuthorizeUser]
		public ActionResult Details(int id)
		{
			return View();
		}

		[Translations.Filters.AuthorizeUser]
		public ActionResult Create()
		{
			return View();
		}

		[HttpPost]
		[Translations.Filters.AuthorizeUser]
		public ActionResult Create(Languages language)
		{
			try
			{
				User _user = Helpers.Core.getUser;
				using (ISession session = NHibernateSession.OpenSession())
				{
					using (ITransaction transaction = session.BeginTransaction())
					{
						string contentHash = Helpers.Hash.CreateHash(language.ToHash);
						language.HashCurrent = contentHash;
						session.Save(language);
						#region Log
						if (Translations.Helpers.Core.LogsActive)
						{
							LanguagesLogs ll = new LanguagesLogs();
							ll.HashKey = contentHash;
							ll.UserName = _user.Name;
							ll.Action = "Create";
							ll.IsActive = language.IsActive;
							ll.LangKey = language.LangKey;
							ll.LangText = language.LangText;
							ll.InsertDate = DateTime.Now;
							session.Save(ll);
						}
						#endregion
						transaction.Commit();
					}
				}
				return RedirectToAction("Index");
			}
			catch
			{
				return View();
			}
		}

		[Translations.Filters.AuthorizeUser]
		public ActionResult Edit(string id)
		{
			using (ISession session = NHibernateSession.OpenSession())
			{
				var _lang = session.Get<Languages>(id);
				return View(_lang);
			}
		}

		[HttpPost]
		[Translations.Filters.AuthorizeUser]
		public ActionResult Edit(string id, Languages _lang)
		{
			try
			{
				User _user = Helpers.Core.getUser;
				using (ISession session = NHibernateSession.OpenSession())
				{
					using (ITransaction transaction = session.BeginTransaction())
					{
						var langUpdate = session.Get<Languages>(id);
						langUpdate.IsActive = _lang.IsActive;
						langUpdate.LangText = _lang.LangText;

						string contentHash = Helpers.Hash.CreateHash(langUpdate.ToHash);
						if (string.IsNullOrEmpty(langUpdate.HashCurrent) || !Helpers.Hash.ValidateHash(langUpdate.ToHash, langUpdate.HashCurrent))
						{
							langUpdate.HashCurrent = contentHash;
							session.Save(langUpdate);
							#region Log
							if (Translations.Helpers.Core.LogsActive)
							{
								LanguagesLogs ll = new LanguagesLogs();
								ll.HashKey = contentHash;
								ll.UserName = _user.Name;
								ll.Action = "Edit";
								ll.IsActive = langUpdate.IsActive;
								ll.LangKey = langUpdate.LangKey;
								ll.LangText = langUpdate.LangText;
								ll.InsertDate = DateTime.Now;
								session.Save(ll);
							}
							#endregion
							transaction.Commit();
						}
					}
				}
				return RedirectToAction("Index");
			}
			catch
			{
				return View();
			}
		}

		[Translations.Filters.AuthorizeUser]
		public ActionResult Delete(int id)
		{
			return View();
		}

		[HttpPost]
		[Translations.Filters.AuthorizeUser]
		public ActionResult Delete(int id, Languages collection)
		{
			try
			{
				return RedirectToAction("Index");
			}
			catch
			{
				return View();
			}
		}
	}
}
