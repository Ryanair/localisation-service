using NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Translations.Models;

namespace Translations.Controllers
{
	public class TagsController : Controller
	{
		[Translations.Filters.AuthorizeUser]
		public ActionResult Index()
		{
			using (ISession session = NHibernateSession.OpenSession())
			{
				var tags = session.QueryOver<Tags>().List<Tags>();
				return View(tags);
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
		public ActionResult Create(Tags tags)
		{
			try
			{
				User _user = Helpers.Core.getUser;
				using (ISession session = NHibernateSession.OpenSession())
				{
					using (ITransaction transaction = session.BeginTransaction())
					{
						string contentHash = Helpers.Hash.CreateHash(tags.ToHash);
						tags.HashCurrent = contentHash;
						session.Save(tags);
						#region Logs
						if (Translations.Helpers.Core.LogsActive)
						{
							TagsLogs tl = new TagsLogs();
							tl.HashKey = contentHash;
							tl.UserName = _user.Name;
							tl.Action = "Create";
							tl.IsActive = tags.IsActive;
							tl.TagKey = tags.TagKey;
							tl.TagText = tags.TagText;
							tl.InsertDate = DateTime.Now;
							session.Save(tl);
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
				var _tags = session.Get<Tags>(id);
				return View(_tags);
			}
		}

		[HttpPost]
		[Translations.Filters.AuthorizeUser]
		public ActionResult Edit(string id, Tags _tags)
		{
			try
			{
				User _user = Helpers.Core.getUser;
				using (ISession session = NHibernateSession.OpenSession())
				{
					using (ITransaction transaction = session.BeginTransaction())
					{
						var tagUpdate = session.Get<Tags>(id);
						tagUpdate.IsActive = _tags.IsActive;
						tagUpdate.TagText = _tags.TagText;

						string contentHash = Helpers.Hash.CreateHash(tagUpdate.ToHash);
						if (string.IsNullOrEmpty(tagUpdate.HashCurrent) || !Helpers.Hash.ValidateHash(_tags.ToHash, tagUpdate.HashCurrent))
						{
							tagUpdate.HashCurrent = contentHash;
							session.Save(tagUpdate);
							#region Logs
							if (Translations.Helpers.Core.LogsActive)
							{
								TagsLogs tl = new TagsLogs();
								tl.HashKey = contentHash;
								tl.UserName = _user.Name;
								tl.Action = "Edit";
								tl.IsActive = tagUpdate.IsActive;
								tl.TagKey = tagUpdate.TagKey;
								tl.TagText = tagUpdate.TagText;
								tl.InsertDate = DateTime.Now;
								session.Save(tl);
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
		public ActionResult Delete(int id, FormCollection collection)
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
