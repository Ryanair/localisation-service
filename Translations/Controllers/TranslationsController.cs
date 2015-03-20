using NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Translations.Models;

namespace Translations.Controllers
{
	public class TranslationsController : Controller
	{
		[Translations.Filters.AuthorizeUser]
		public ActionResult Index(string lang = null)
		{
			using (ISession session = NHibernateSession.OpenSession())
			{
				List<TransLang> _trans = session.CreateSQLQuery("exec GetTranslationsByLang :lang ")
					.AddEntity(typeof(TransLang))
					.SetParameter("lang", !string.IsNullOrEmpty(lang) ? lang.Trim() : "en")
					.List<TransLang>() as List<TransLang>;

				if (Request.IsAjaxRequest())
				{
					return PartialView("_TransList", _trans);
				}

				return View(_trans);
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
		public ActionResult Create(Translations.Models.Translations translations, TransLang[] transLang, Tags[] tags, TransPlural[][] transPlural, Languages[] language, TransArray[][] transArray)
		{
			try
			{
				User _user = Helpers.Core.getUser;
				// Create Translations and Tags
				#region Translation and tag
				using (ISession session = NHibernateSession.OpenSession())
				{
					using (ITransaction transaction = session.BeginTransaction())
					{
						string transHash = Helpers.Hash.CreateHash(translations.ToHash);
						translations.HashCurrent = transHash;
						translations.CreationDate = DateTime.Now;
						session.Save(translations);

						#region Logs Translation
						if (Translations.Helpers.Core.LogsActive)
						{
							TranslationsLogs tl = new TranslationsLogs();
							tl.HashKey = transHash;
							tl.UserName = _user.Name;
							tl.Action = "Create";
							tl.TransKey = translations.TransKey;
							tl.InsertDate = DateTime.Now;
							session.Save(tl);
						}
						#endregion

						// See if Tags null create content for all tags otherwise create only for selected Tags
						#region Content By Tag
						if (tags == null)
						{
							foreach (var item in Translations.Helpers.Core.ListTags)
							{
								TransTag transT = new TransTag();
								transT.Translations = translations;
								transT.Tags = new Tags() { TagKey = item.Value };
								transT.TransTagKey = string.Format("{0}_{1}", translations.TransKey.Trim(), item.Value.Trim());
								transT.IsActive = true;
								string transTagHash = Helpers.Hash.CreateHash(transT.ToHash);
								transT.HashCurrent = transTagHash;
								session.Save(transT);

								#region Logs Trans Tags
								if (Translations.Helpers.Core.LogsActive)
								{
									TransTagLogs ttl = new TransTagLogs();
									ttl.HashKey = transTagHash;
									ttl.UserName = _user.Name;
									ttl.Action = "Create";
									ttl.TransKey = transT.Translations.TransKey;
									ttl.TagKey = transT.Tags.TagKey;
									ttl.TransTagKey = transT.TransTagKey;
									ttl.IsActive = transT.IsActive;
									ttl.InsertDate = DateTime.Now;
									session.Save(ttl);
								}
								#endregion
							}
						}
						else
						{
							foreach (Tags tag in tags)
							{
								TransTag transT = new TransTag();
								transT.Translations = translations;
								transT.Tags = tag;
								transT.TransTagKey = string.Format("{0}_{1}", translations.TransKey.Trim(), tag.TagKey.Trim());
								transT.IsActive = true;
								string transTagHash = Helpers.Hash.CreateHash(transT.ToHash);
								transT.HashCurrent = transTagHash;
								session.Save(transT);

								#region Logs Trans Tags
								if (Translations.Helpers.Core.LogsActive)
								{
									TransTagLogs ttl = new TransTagLogs();
									ttl.HashKey = transTagHash;
									ttl.UserName = _user.Name;
									ttl.Action = "Create";
									ttl.TransKey = transT.Translations.TransKey;
									ttl.TagKey = transT.Tags.TagKey;
									ttl.TransTagKey = transT.TransTagKey;
									ttl.IsActive = transT.IsActive;
									ttl.InsertDate = DateTime.Now;
									session.Save(ttl);
								}
								#endregion
							}
						}
						#endregion

						transaction.Commit();
					}
				}
				#endregion

				//Create all information about Translations and do it by language
				#region Translation By Language
				using (ISession session = NHibernateSession.OpenSession())
				{
					using (ITransaction transaction = session.BeginTransaction())
					{
						var langIdx = 0;
						foreach (Languages lang in language)
						{
							TransLang transl = new TransLang();
							transl.TransLangKey = string.Format("{0}_{1}", translations.TransKey.Trim(), lang.LangKey.Trim());
							transl.Languages = new Languages() { LangKey = lang.LangKey.Trim(), LangText = lang.LangText.Trim(), IsActive = lang.IsActive };
							transl.Translations = new Translations.Models.Translations() { TransKey = translations.TransKey.Trim() };
							transl.TransDescription = transLang[langIdx].TransDescription.Trim();
							transl.IsActive = true;
							string translHash = Helpers.Hash.CreateHash(transl.ToHash);
							transl.HashCurrent = translHash;
							session.Save(transl);

							#region Logs Trans Lang
							if (Translations.Helpers.Core.LogsActive)
							{
								TransLangLogs tll = new TransLangLogs();
								tll.HashKey = translHash;
								tll.UserName = _user.Name;
								tll.Action = "Create";
								tll.TransLangKey = transl.TransLangKey;
								tll.LangKey = transl.Languages.LangKey.Trim();
								tll.TransDescription = transl.TransDescription;
								tll.TransKey = transl.Translations.TransKey.Trim();
								tll.IsActive = transl.IsActive;
								tll.InsertDate = DateTime.Now;
								session.Save(tll);
							}
							#endregion

							//Get all plurals with description not empty to create plural
							if (transPlural != null && transPlural[langIdx].Any(x => !string.IsNullOrEmpty(x.PluralDescription)))
							{
								foreach (TransPlural tpItem in transPlural[langIdx].Where(x => !string.IsNullOrEmpty(x.PluralDescription)))
								{
									TransPlural transP = new TransPlural();
									transP.TransPluralKey = string.Format("{0}_{1}", transl.TransLangKey.Trim(), tpItem.Plural.PluralKey.Trim());
									transP.TransLang = transl;
									transP.PluralDescription = tpItem.PluralDescription.Trim();
									transP.Plural = tpItem.Plural;
									transP.IsActive = true;
									string pluralHash = Helpers.Hash.CreateHash(transP.ToHash);
									transP.HashCurrent = pluralHash;
									session.Save(transP);

									#region Logs Trans Plural
									if (Translations.Helpers.Core.LogsActive)
									{
										TransPluralLogs tpl = new TransPluralLogs();
										tpl.HashKey = pluralHash;
										tpl.UserName = _user.Name;
										tpl.Action = "Create";
										tpl.TransLangKey = transP.TransLang.TransLangKey;
										tpl.TransPluralKey = transP.TransPluralKey;
										tpl.PluralDescription = transP.PluralDescription;
										tpl.PluralKey = transP.Plural.PluralKey;
										tpl.IsActive = transP.IsActive;
										tpl.InsertDate = DateTime.Now;
										session.Save(tpl);
									}
									#endregion

								}
							}

							//create all Translations Array that description not empty or null
							foreach (TransArray taItem in transArray[langIdx])
							{
								if (!string.IsNullOrEmpty(taItem.ArrayDescription))
								{
									TransArray transA = new TransArray();
									transA.TransLang = transl;
									transA.ArrayDescription = taItem.ArrayDescription.Trim();
									transA.IsActive = true;
									string transHash = Helpers.Hash.CreateHash(transA.ToHash);
									transA.HashCurrent = transHash;
									session.Save(transA);

									#region Logs Translations Array
									if (Translations.Helpers.Core.LogsActive)
									{
										TransArrayLogs tal = new TransArrayLogs();
										tal.HashKey = transA.HashCurrent;
										tal.UserName = _user.Name;
										tal.Action = "Create";
										tal.TransLangKey = transA.TransLang.TransLangKey;
										tal.TransArrayId = transA.TransArrayId;
										tal.ArrayDescription = transA.ArrayDescription;
										tal.IsActive = transA.IsActive;
										tal.InsertDate = DateTime.Now;
										session.Save(tal);
									}
									#endregion
								}
							}

							langIdx++;
						}
						transaction.Commit();
					}
				}
				#endregion

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
				var _trans = session.Get<Translations.Models.Translations>(id);

				//Initialize Object because in views session already lost.
				List<TransTag> _transTagList = _trans.TransTag.ToList();
				List<TransLang> _transLangList = _trans.TransLang.ToList();
				foreach (var item in _trans.TransLang)
				{
					List<TransPlural> _transPliralList = item.TransPlural.ToList();
				}

				foreach (var item in _trans.TransLang)
				{
					List<TransArray> _transArray = item.TransArray.ToList();
				}

				return View(_trans);
			}
		}

		[HttpPost]
		[Translations.Filters.AuthorizeUser]
		public ActionResult Edit(string id, Translations.Models.Translations _trans, TransLang[] transLang, Tags[] tags, TransPlural[][] transPlural, Languages[] language, TransArray[][] transArray)
		{
			try
			{
				User _user = Helpers.Core.getUser;
				using (ISession session = NHibernateSession.OpenSession())
				{
					var transUpdate = session.Get<Translations.Models.Translations>(id);
					//transUpdate.CreationDate = DateTime.Now;
					using (ITransaction transaction = session.BeginTransaction())
					{
						#region Update/Insert Tags
						foreach (var item in Translations.Helpers.Core.ListTags)
						{
							//validate if tags already exist to edit or create
							bool exist = tags != null && tags.Count() > 0 ? tags.Any(x => x.TagKey.Trim().Equals(item.Value.Trim(), StringComparison.InvariantCultureIgnoreCase)) : false;
							TransTag transTagUpdate = null;
							TransTagLogs ttl = new TransTagLogs();
							transTagUpdate = session.Get<Translations.Models.TransTag>(string.Format("{0}_{1}", transUpdate.TransKey.Trim(), item.Value.Trim()));

							if (!exist && transTagUpdate != null)
							{
								transTagUpdate.IsActive = false;
								ttl.Action = "Edit";
							}

							else if (transTagUpdate == null)
							{
								transTagUpdate = new TransTag();
								transTagUpdate.Translations = transUpdate;
								transTagUpdate.Tags = new Tags() { TagKey = item.Value.Trim() };
								transTagUpdate.TransTagKey = string.Format("{0}_{1}", transUpdate.TransKey.Trim(), item.Value.Trim());
								transTagUpdate.IsActive = true;
								ttl.Action = "Create";
							}
							else
							{
								transTagUpdate.IsActive = true;
								ttl.Action = "Edit";
							}

							string tagttlUpdade = Helpers.Hash.CreateHash(transTagUpdate.ToHash);
							if (string.IsNullOrEmpty(transTagUpdate.HashCurrent) || !Helpers.Hash.ValidateHash(transTagUpdate.ToHash, transTagUpdate.HashCurrent))
							{
								transTagUpdate.HashCurrent = tagttlUpdade;
								session.Save(transTagUpdate);
								#region Logs Trans Tag
								if (Translations.Helpers.Core.LogsActive)
								{
									ttl.HashKey = tagttlUpdade;
									ttl.UserName = _user.Name;
									ttl.TransKey = transTagUpdate.Translations.TransKey;
									ttl.TagKey = transTagUpdate.Tags.TagKey;
									ttl.TransTagKey = transTagUpdate.TransTagKey;
									ttl.IsActive = transTagUpdate.IsActive;
									ttl.InsertDate = DateTime.Now;
									session.Save(ttl);
								}
								#endregion
							}
						}
						#endregion

						//Get All Active Languages
						var _activeTransLang = session.CreateSQLQuery("exec GetActiveLanguages :tKey")
						   .AddEntity(typeof(Translations.Models.TransLang))
						   .SetParameter("tKey", transUpdate.TransKey.Trim()).List<Translations.Models.TransLang>();

						//set languages not new or edit as inactive.
						foreach (TransLang itemTransLang in _activeTransLang)
						{
							if (!language.Any(x => x.LangKey.Trim() == itemTransLang.Languages.LangKey.Trim()))
							{
								itemTransLang.IsActive = false;
								string itemTransLangHash = Helpers.Hash.CreateHash(itemTransLang.ToHash);
								if (string.IsNullOrEmpty(itemTransLang.HashCurrent) || !Helpers.Hash.ValidateHash(itemTransLang.ToHash, itemTransLang.HashCurrent))
								{
									itemTransLang.HashCurrent = itemTransLangHash;
									session.Save(itemTransLang);
									#region Logs Trans Lang
									if (Translations.Helpers.Core.LogsActive)
									{
										TransLangLogs tll = new TransLangLogs();
										tll.HashKey = itemTransLangHash;
										tll.UserName = _user.Name;
										tll.Action = "Edit";
										tll.TransLangKey = itemTransLang.TransLangKey;
										tll.LangKey = itemTransLang.Languages.LangKey.Trim();
										tll.TransDescription = itemTransLang.TransDescription;
										tll.TransKey = itemTransLang.Translations.TransKey.Trim();
										tll.IsActive = itemTransLang.IsActive;
										tll.InsertDate = DateTime.Now;
										session.Save(tll);
									}
									#endregion
								}
							}
						}
						//create translation by languages and plurals arrays per translation
						var langIdx = 0;
						if (language != null)
						{
							foreach (Languages lang in language)
							{
								TransLang translUpdade = session.Get<Translations.Models.TransLang>(string.Format("{0}_{1}", _trans.TransKey.Trim(), lang.LangKey.Trim()));
								TransLangLogs tll = new TransLangLogs();

								if (translUpdade != null)
								{
									translUpdade.TransDescription = !string.IsNullOrEmpty(transLang[langIdx].TransDescription.Trim()) ? transLang[langIdx].TransDescription.Trim() : "";
									translUpdade.IsActive = !string.IsNullOrEmpty(transLang[langIdx].TransDescription) ? true : false;
									tll.Action = "Edit";
								}
								else
								{
									translUpdade = new TransLang();
									translUpdade.TransLangKey = string.Format("{0}_{1}", _trans.TransKey.Trim(), lang.LangKey.Trim());
									translUpdade.Languages = new Languages() { LangKey = lang.LangKey.Trim(), LangText = lang.LangText.Trim(), IsActive = lang.IsActive };
									translUpdade.Translations = new Translations.Models.Translations() { TransKey = _trans.TransKey.Trim() };
									translUpdade.IsActive = true;
									translUpdade.TransDescription = transLang[langIdx].TransDescription.Trim();
									tll.Action = "Create";

								}

								string translUpdadeHash = Helpers.Hash.CreateHash(translUpdade.ToHash);
								if (string.IsNullOrEmpty(translUpdade.HashCurrent) || !Helpers.Hash.ValidateHash(translUpdade.ToHash, translUpdade.HashCurrent))
								{
									translUpdade.HashCurrent = translUpdadeHash;
									session.Save(translUpdade);

									#region Logs Trans Lang
									if (Translations.Helpers.Core.LogsActive)
									{
										tll.HashKey = translUpdadeHash;
										tll.UserName = _user.Name;
										tll.TransLangKey = translUpdade.TransLangKey;
										tll.LangKey = translUpdade.Languages.LangKey.Trim();
										tll.TransDescription = translUpdade.TransDescription;
										tll.TransKey = translUpdade.Translations.TransKey.Trim();
										tll.IsActive = translUpdade.IsActive;
										tll.InsertDate = DateTime.Now;
										session.Save(tll);
									}
									#endregion
								}

								//Get All Active Plurals
								var _activePlurals = session.CreateSQLQuery("exec GetActivePlurals :tlKey")
								   .AddEntity(typeof(Translations.Models.TransPlural))
								   .SetParameter("tlKey", translUpdade.TransLangKey.Trim()).List<Translations.Models.TransPlural>();

								//set Plurals not new or edit as inactive.
								foreach (TransPlural itemPlural in _activePlurals)
								{
									if (transPlural == null || transPlural[langIdx].Length == 0 || !transPlural[langIdx].Any(x => x.Plural != null && !string.IsNullOrEmpty(x.Plural.PluralKey.Trim()) && x.Plural.PluralKey.Trim().Equals(itemPlural.Plural.PluralKey.Trim())))
									{
										itemPlural.IsActive = false;
										itemPlural.PluralDescription = string.Empty;

										string itemPluralHash = Helpers.Hash.CreateHash(itemPlural.ToHash);
										if (string.IsNullOrEmpty(itemPlural.HashCurrent) || !Helpers.Hash.ValidateHash(itemPlural.ToHash, itemPlural.HashCurrent))
										{
											itemPlural.HashCurrent = itemPluralHash;
											session.Save(itemPlural);
											#region Logs Plural
											if (Translations.Helpers.Core.LogsActive)
											{
												TransPluralLogs tpl = new TransPluralLogs();
												tpl.HashKey = itemPluralHash;
												tpl.UserName = _user.Name;
												tpl.TransLangKey = itemPlural.TransLang.TransLangKey;
												tpl.Action = "Edit";
												tpl.TransPluralKey = itemPlural.TransPluralKey;
												tpl.PluralKey = itemPlural.Plural.PluralKey;
												tpl.PluralDescription = itemPlural.PluralDescription;
												tpl.IsActive = itemPlural.IsActive;
												tpl.InsertDate = DateTime.Now;
												session.Save(tpl);
											}
											#endregion
										}
									}
								}
								//create or edit pluals active
								if (transPlural != null && transPlural[langIdx].Any(x => !string.IsNullOrEmpty(x.PluralDescription)))
								{
									foreach (TransPlural tpItem in transPlural[langIdx].Where(x => !string.IsNullOrEmpty(x.PluralDescription)))
									{
										TransPlural transPUpdate = new TransPlural();
										transPUpdate = session.Get<Translations.Models.TransPlural>(string.Format("{0}_{1}", translUpdade.TransLangKey.Trim(), tpItem.Plural.PluralKey.Trim()));

										if (transPUpdate != null)
										{
											transPUpdate.PluralDescription = tpItem.PluralDescription.Trim();
											transPUpdate.IsActive = true;

											string itemPluralHash = Helpers.Hash.CreateHash(transPUpdate.ToHash);
											if (string.IsNullOrEmpty(transPUpdate.HashCurrent) || !Helpers.Hash.ValidateHash(transPUpdate.ToHash, transPUpdate.HashCurrent))
											{
												transPUpdate.HashCurrent = itemPluralHash;
												session.Save(transPUpdate);

												#region Logs Trans Plural
												if (Translations.Helpers.Core.LogsActive)
												{
													TransPluralLogs tpl = new TransPluralLogs();
													tpl.HashKey = itemPluralHash;
													tpl.UserName = _user.Name;
													tpl.TransLangKey = transPUpdate.TransLang.TransLangKey;
													tpl.Action = "Edit";
													tpl.TransPluralKey = transPUpdate.TransPluralKey;
													tpl.PluralKey = transPUpdate.Plural.PluralKey;
													tpl.PluralDescription = transPUpdate.PluralDescription;
													tpl.IsActive = transPUpdate.IsActive;
													tpl.InsertDate = DateTime.Now;
													session.Save(tpl);
												}
												#endregion
											}

										}
										else
										{
											TransPlural transPluralNew = new TransPlural();
											transPluralNew.TransPluralKey = string.Format("{0}_{1}", translUpdade.TransLangKey.Trim(), tpItem.Plural.PluralKey.Trim());
											transPluralNew.TransLang = translUpdade;
											transPluralNew.PluralDescription = tpItem.PluralDescription.Trim();
											transPluralNew.Plural = tpItem.Plural;
											transPluralNew.IsActive = true;

											string itemPluralHash = Helpers.Hash.CreateHash(transPluralNew.ToHash);
											if (string.IsNullOrEmpty(transPluralNew.HashCurrent) || !Helpers.Hash.ValidateHash(transPluralNew.ToHash, transPluralNew.HashCurrent))
											{
												transPluralNew.HashCurrent = itemPluralHash;
												session.Save(transPluralNew);

												#region Log Create Plural
												if (Translations.Helpers.Core.LogsActive)
												{
													TransPluralLogs tpl = new TransPluralLogs();
													tpl.HashKey = itemPluralHash;
													tpl.UserName = _user.Name;
													tpl.TransLangKey = transPluralNew.TransLang.TransLangKey;
													tpl.Action = "Create";
													tpl.TransPluralKey = transPluralNew.TransPluralKey;
													tpl.PluralKey = transPluralNew.Plural.PluralKey;
													tpl.PluralDescription = transPluralNew.PluralDescription;
													tpl.IsActive = transPluralNew.IsActive;
													tpl.InsertDate = DateTime.Now;
													session.Save(tpl);
												}
												#endregion
											}

										}
									}
								}
								if (langIdx < transArray[langIdx].Length)
								{
									foreach (TransArray taItem in transArray[langIdx])
									{
										TransArray transAUpdate = new TransArray();
										transAUpdate = session.Get<Translations.Models.TransArray>(taItem.TransArrayId);
										if (transAUpdate != null)
										{
											transAUpdate.ArrayDescription = !string.IsNullOrEmpty(taItem.ArrayDescription) && !string.IsNullOrEmpty(taItem.ArrayDescription.Trim()) ? taItem.ArrayDescription.Trim() : string.Empty;
											transAUpdate.IsActive = !string.IsNullOrEmpty(taItem.ArrayDescription) && !string.IsNullOrEmpty(taItem.ArrayDescription.Trim()) ? true : false;

											string transArrayHash = Helpers.Hash.CreateHash(transAUpdate.ToHash);
											if (string.IsNullOrEmpty(transAUpdate.HashCurrent) || !Helpers.Hash.ValidateHash(transAUpdate.ToHash, transAUpdate.HashCurrent))
											{
												transAUpdate.HashCurrent = transArrayHash;
												session.Save(transAUpdate);
												#region Log Edit Array
												if (Translations.Helpers.Core.LogsActive)
												{
													TransArrayLogs tal = new TransArrayLogs();
													tal.HashKey = transArrayHash;
													tal.UserName = _user.Name;
													tal.TransLangKey = transAUpdate.TransLang.TransLangKey;
													tal.Action = "Edit";
													tal.TransArrayId = transAUpdate.TransArrayId;
													tal.ArrayDescription = transAUpdate.ArrayDescription;
													tal.IsActive = transAUpdate.IsActive;
													tal.InsertDate = DateTime.Now;
													session.Save(tal);
												}
												#endregion
											}
										}
										else
										{
											if (!string.IsNullOrEmpty(taItem.ArrayDescription))
											{
												TransArray transA = new TransArray();
												transA.TransLang = translUpdade;
												transA.ArrayDescription = taItem.ArrayDescription.Trim();
												transA.IsActive = true;

												string transArrayHash = Helpers.Hash.CreateHash(transA.ToHash);
												if (string.IsNullOrEmpty(transA.HashCurrent) || !Helpers.Hash.ValidateHash(transA.ToHash, transA.HashCurrent))
												{
													transA.HashCurrent = transArrayHash;
													session.Save(transA);
													#region Create Array Log
													if (Translations.Helpers.Core.LogsActive)
													{
														TransArrayLogs tal = new TransArrayLogs();
														tal.HashKey = transArrayHash;
														tal.UserName = _user.Name;
														tal.TransLangKey = transA.TransLang.TransLangKey;
														tal.Action = "Create";
														tal.TransArrayId = transA.TransArrayId;
														tal.ArrayDescription = transA.ArrayDescription;
														tal.IsActive = transA.IsActive;
														tal.InsertDate = DateTime.Now;
														session.Save(tal);
													}
													#endregion
												}
											}
										}
									}
								}

								langIdx++;
							}
						}
						session.Save(transUpdate);
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

		[HttpPost]
		[Translations.Filters.AuthorizeUser]
		public ActionResult validateTrans(string key)
		{
			try
			{
				Translations.Models.Translations _trans = null;

				using (ISession session = NHibernateSession.OpenSession())
				{
					_trans = session.CreateSQLQuery("exec ValidateTranslation :key")
					   .AddEntity(typeof(Translations.Models.Translations))
					   .SetParameter("key", key).UniqueResult<Translations.Models.Translations>();
				}

				if (_trans != null)
				{
					return Json(new { status = "edit", html = Url.Action("Edit", "Translations", new { id = key }) });
				}
				else
				{
					return Json(new { status = "create", html = "" });
				}
			}
			catch
			{
				return Json(new { status = "error", html = "teste error" });
			}
		}



		[Translations.Filters.AuthorizeUser]
		public ActionResult Activate(string transLangKey)
		{
			//using (ISession session = NHibernateSession.OpenSession())
			//{
			//	var _trans = session.Get<Translations.Models.TransLang>(transLangKey);



				return RedirectToAction("Index", "Translations");
			//}
		}


		[Translations.Filters.AuthorizeUser]
		public ActionResult Inactivate(string transLangKey)
		{
			using (ISession session = NHibernateSession.OpenSession())
			{
				using (ITransaction transaction = session.BeginTransaction())
				{
					var _trans = session.Get<Translations.Models.TransLang>(transLangKey);
					if (_trans != null)
					{
						User _user = Helpers.Core.getUser;
						//Inactivate all actives Plurals
						#region Inactive Plurals
						List<Translations.Models.TransPlural> _listTransPlural = new List<Translations.Models.TransPlural>();
						_listTransPlural = session.CreateSQLQuery("exec GetPluralsByKey :trans_langKey ")
												.AddEntity(typeof(Translations.Models.TransPlural))
												.SetParameter("trans_langKey", _trans.TransLangKey.Trim())
												.List<Translations.Models.TransPlural>() as List<Translations.Models.TransPlural>;
						if (_listTransPlural != null && _listTransPlural.Count > 0)
						{
							foreach (TransPlural item in _listTransPlural)
							{
								if (item.IsActive)
								{
									item.IsActive = false;
									string transPluralHash = Helpers.Hash.CreateHash(item.ToHash);
									if (string.IsNullOrEmpty(item.HashCurrent) || !Helpers.Hash.ValidateHash(item.ToHash, item.HashCurrent))
									{
										item.HashCurrent = transPluralHash;
										session.Save(item);
										#region Create Trans Plural Log
										if (Translations.Helpers.Core.LogsActive)
										{
											TransPluralLogs tpl = new TransPluralLogs();
											tpl.HashKey = transPluralHash;
											tpl.UserName = _user.Name;
											tpl.Action = "Edit";
											tpl.TransLangKey = item.TransLang.TransLangKey;
											tpl.TransPluralKey = item.TransPluralKey;
											tpl.PluralDescription = item.PluralDescription;
											tpl.PluralKey = item.Plural.PluralKey;
											tpl.IsActive = item.IsActive;
											tpl.InsertDate = DateTime.Now;
											session.Save(tpl);
										}
										#endregion
									}
								}
							}
						}
						#endregion
						//Inactivate all actives Trans Array
						#region Inactive Arrays
						List<Translations.Models.TransArray> _listTransArray = new List<Translations.Models.TransArray>();
						_listTransArray = session.CreateSQLQuery("exec GetArraysByKey :trans_langKey ")
															.AddEntity(typeof(Translations.Models.TransArray))
															.SetParameter("trans_langKey", _trans.TransLangKey.Trim())
															.List<Translations.Models.TransArray>() as List<Translations.Models.TransArray>;
						if (_listTransArray != null && _listTransArray.Count > 0)
						{
							foreach (TransArray item in _listTransArray)
							{
								if (item.IsActive)
								{
									item.IsActive = false;
									string transArrayHash = Helpers.Hash.CreateHash(item.ToHash);
									if (string.IsNullOrEmpty(item.HashCurrent) || !Helpers.Hash.ValidateHash(item.ToHash, item.HashCurrent))
									{
										item.HashCurrent = transArrayHash;
										session.Save(item);
										#region Create Array Log
										if (Translations.Helpers.Core.LogsActive)
										{
											TransArrayLogs tal = new TransArrayLogs();
											tal.HashKey = transArrayHash;
											tal.UserName = _user.Name;
											tal.TransLangKey = item.TransLang.TransLangKey;
											tal.Action = "Edit";
											tal.TransArrayId = item.TransArrayId;
											tal.ArrayDescription = item.ArrayDescription;
											tal.IsActive = item.IsActive;
											tal.InsertDate = DateTime.Now;
											session.Save(tal);
										}
										#endregion
									}
								}
							}
						}
						#endregion
						//inactive all active Trans Tags
						#region Inactive Trans Tags
						List<Translations.Models.TransTag> _listTransTags = new List<Translations.Models.TransTag>();
						_listTransTags = session.CreateSQLQuery("exec GetTransTagsByKey :trans_Key ")
															.AddEntity(typeof(Translations.Models.TransTag))
															.SetParameter("trans_Key", _trans.Translations.TransKey)
															.List<Translations.Models.TransTag>() as List<Translations.Models.TransTag>;
						if (_listTransTags != null && _listTransTags.Count > 0)
						{
							foreach (TransTag item in _listTransTags)
							{
								if (item.IsActive)
								{
									item.IsActive = false;

									string transTagHash = Helpers.Hash.CreateHash(item.ToHash);
									if (string.IsNullOrEmpty(item.HashCurrent) || !Helpers.Hash.ValidateHash(item.ToHash, item.HashCurrent))
									{
										item.HashCurrent = transTagHash;
										session.Save(item);
										#region Create Trans Plural Log
										if (Translations.Helpers.Core.LogsActive)
										{
											TransTagLogs ttl = new TransTagLogs();
											ttl.HashKey = transTagHash;
											ttl.UserName = _user.Name;
											ttl.Action = "Edit";
											ttl.TransKey = item.Translations.TransKey;
											ttl.TagKey = item.Tags.TagKey;
											ttl.TransTagKey = item.TransTagKey;
											ttl.IsActive = item.IsActive;
											ttl.InsertDate = DateTime.Now;
											session.Save(ttl);
										}
										#endregion
									}

								}
							}
						}
						#endregion
						//inactive Translation
						#region Inactive Translation
						_trans.IsActive = false;
						string itemTransLangHash = Helpers.Hash.CreateHash(_trans.ToHash);
						if (string.IsNullOrEmpty(_trans.HashCurrent) || !Helpers.Hash.ValidateHash(_trans.ToHash, _trans.HashCurrent))
						{
							_trans.HashCurrent = itemTransLangHash;
							session.Save(_trans);
							#region Logs Trans Lang
							if (Translations.Helpers.Core.LogsActive)
							{
								TransLangLogs tll = new TransLangLogs();
								tll.HashKey = itemTransLangHash;
								tll.UserName = _user.Name;
								tll.Action = "Edit";
								tll.TransLangKey = _trans.TransLangKey;
								tll.LangKey = _trans.Languages.LangKey.Trim();
								tll.TransDescription = _trans.TransDescription;
								tll.TransKey = _trans.Translations.TransKey.Trim();
								tll.IsActive = _trans.IsActive;
								tll.InsertDate = DateTime.Now;
								session.Save(tll);
							}
							#endregion
						}
						#endregion

						transaction.Commit();
					}
				}
				return RedirectToAction("Index", "Translations");
			}
		}




	}
}
