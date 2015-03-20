using NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Translations.Models;

namespace Translations.Controllers
{
	public class FileFolderController : Controller
	{

		[Translations.Filters.AuthorizeUser]
		public ActionResult Index()
		{
			using (ISession session = NHibernateSession.OpenSession())
			{
				var fileFolder = session.QueryOver<FileFolder>().List<FileFolder>();
				return View(fileFolder);
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
		public ActionResult Create(FileFolder fileFolder, Languages lang, Tags tag)
		{
			try
			{
				User _user = Helpers.Core.getUser;
				using (ISession session = NHibernateSession.OpenSession())
				{
					using (ITransaction transaction = session.BeginTransaction())
					{
						//validate if folder already exist for tag and lang
						Translations.Models.FileFolder _fileFolder = session.CreateSQLQuery("exec GetFileFolderByTagLang :tagKey , :langKey")
						.AddEntity(typeof(Translations.Models.FileFolder))
						.SetParameter("tagKey", tag.TagKey.Trim())
						.SetParameter("langKey", lang.LangKey.Trim())
						.UniqueResult<Translations.Models.FileFolder>();

						if (_fileFolder == null)
						{
							fileFolder.Languages = new Languages() { LangKey = lang.LangKey };
							fileFolder.Tags = new Tags() { TagKey = tag.TagKey };

							string contentHash = Helpers.Hash.CreateHash(fileFolder.ToHash);
							fileFolder.HashCurrent = contentHash;
							session.Save(fileFolder);

							#region Log
							if (Translations.Helpers.Core.LogsActive)
							{
								FileFolderLogs fl = new FileFolderLogs();
								fl.HashKey = contentHash;
								fl.UserName = _user.Name;
								fl.Action = "Create";
								fl.FolderName = fileFolder.FolderName;
								fl.FolderLangId = fileFolder.FolderLangId;
								fl.FileName = fileFolder.FileName;
								fl.LangKey = fileFolder.Languages.LangKey;
								fl.TagKey = fileFolder.Tags.TagKey;
								fl.InsertDate = DateTime.Now;
								session.Save(fl);
							}
							#endregion

							transaction.Commit();
						}
						else
						{
							return RedirectToAction("Edit", new { id = _fileFolder.FolderLangId });
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
		public ActionResult Edit(int id)
		{
			using (ISession session = NHibernateSession.OpenSession())
			{
				var _ff = session.Get<FileFolder>(id);
				return View(_ff);
			}
		}

		[HttpPost]
		[Translations.Filters.AuthorizeUser]
		public ActionResult Edit(int id, FileFolder _ff, Languages lang, Tags tag)
		{
			try
			{
				User _user = Helpers.Core.getUser;
				using (ISession session = NHibernateSession.OpenSession())
				{
					using (ITransaction transaction = session.BeginTransaction())
					{
						var ffUpdate = session.Get<FileFolder>(id);
						ffUpdate.FolderName = _ff.FolderName.Trim();
						ffUpdate.FileName = _ff.FileName.Trim();
						ffUpdate.Languages = new Languages() { LangKey = lang.LangKey };
						ffUpdate.Tags = new Tags() { TagKey = tag.TagKey };

						string contentHash = Helpers.Hash.CreateHash(ffUpdate.ToHash);
						if (string.IsNullOrEmpty(ffUpdate.HashCurrent) || !Helpers.Hash.ValidateHash(ffUpdate.ToHash, ffUpdate.HashCurrent))
						{
							ffUpdate.HashCurrent = contentHash;
							session.Save(ffUpdate);
							#region Log
							if (Translations.Helpers.Core.LogsActive)
							{
								FileFolderLogs fl = new FileFolderLogs();
								fl.HashKey = contentHash;
								fl.UserName = _user.Name;
								fl.Action = "Edit";
								fl.FileName = ffUpdate.FileName;
								fl.FolderLangId = ffUpdate.FolderLangId;
								fl.FolderName = ffUpdate.FolderName;
								fl.LangKey = ffUpdate.Languages.LangKey;
								fl.TagKey = ffUpdate.Tags.TagKey;
								fl.InsertDate = DateTime.Now;
								session.Save(fl);
							}
							#endregion
						}
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
