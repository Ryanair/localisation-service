using NHibernate;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using Ionic.Zip;
using Translations.Models;
using System.Text;

namespace Translations.Controllers
{
	public class ExportController : Controller
	{
		[Translations.Filters.AuthorizeUser]
		public ActionResult Index()
		{
			return View();
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
		public ActionResult Create(FormCollection collection)
		{
			try
			{
				// TODO: Add insert logic here

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
			return View();
		}

		[HttpPost]
		[Translations.Filters.AuthorizeUser]
		public ActionResult Edit(int id, FormCollection collection)
		{
			try
			{
				// TODO: Add update logic here

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
				// TODO: Add delete logic here

				return RedirectToAction("Index");
			}
			catch
			{
				return View();
			}
		}

		[Translations.Filters.AuthorizeUser]
		public ActionResult Export(string tagkey, string exportType, DateTime? cDate = null)
		{
			string physicalPath = Server.MapPath("~/Exports");
			string path = Path.Combine(physicalPath, string.Format("{0}_{1}", User.Identity.Name, tagkey));

			bool exists = Directory.Exists(path);
			if (!exists)
				Directory.CreateDirectory(path);

			if (exportType.Equals("csv", StringComparison.InvariantCultureIgnoreCase) || exportType.Equals("xls", StringComparison.InvariantCultureIgnoreCase) || exportType.Equals("xlsx", StringComparison.InvariantCultureIgnoreCase))
			{
				if (!cDate.HasValue)
					cDate = DateTime.Now;

				return RedirectToAction("GetCsv", new { cDate = cDate, type = exportType });
			}


			switch (tagkey.ToLowerInvariant())
			{
				case "android":
					return RedirectToAction("GetAndroidXMLFile", new { tagkey = tagkey, path = path });
				case "ios":
					return RedirectToAction("GetIosXMLFile", new { tagkey = tagkey, path = path });
				default:
					return View("Index");
			}
		}

		[Translations.Filters.AuthorizeUser]
		public ActionResult GetAndroidXMLFile(string tagkey, string path)
		{
			using (ISession session = NHibernateSession.OpenSession())
			{
				foreach (var _itemLanguages in Translations.Helpers.Core.ListLanguages)
				{
					Translations.Models.FileFolder _fileFolder = session.CreateSQLQuery("exec GetFileFolderByTagLang :tagKey , :langKey")
					.AddEntity(typeof(Translations.Models.FileFolder))
					.SetParameter("tagKey", tagkey.Trim())
					.SetParameter("langKey", _itemLanguages.Value.Trim())
					.UniqueResult<Translations.Models.FileFolder>();


					List<Translations.Models.Translations> _listTrans = new List<Translations.Models.Translations>();
					_listTrans = session.CreateSQLQuery("exec GetKeysByTag :tagKey, :langKey")
											.AddEntity(typeof(Translations.Models.Translations))
											.SetParameter("tagKey", tagkey)
											.SetParameter("langKey", _itemLanguages.Value)
											.List<Translations.Models.Translations>() as List<Translations.Models.Translations>;

					XmlDocument doc = new XmlDocument();

					//(1) the xml declaration is recommended, but not mandatory
					//XmlDeclaration xmlDeclaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
					XmlElement root = doc.DocumentElement;
					//doc.InsertBefore(xmlDeclaration, root);

					//(2) string.Empty makes cleaner code
					XmlElement element1 = doc.CreateElement(string.Empty, "resources", string.Empty);
					doc.AppendChild(element1);

					foreach (Translations.Models.Translations itemTrans in _listTrans)
					{
						List<Translations.Models.TransLang> _listTransLang = new List<Translations.Models.TransLang>();
						_listTransLang = session.CreateSQLQuery("exec GetActiveTransLag :transKey, :langKey ")
												.AddEntity(typeof(Translations.Models.TransLang))
												.SetParameter("transKey", itemTrans.TransKey)
												.SetParameter("langKey", _itemLanguages.Value)
												.List<Translations.Models.TransLang>() as List<Translations.Models.TransLang>;

						// TEST LANGUAGES
						//List<Translations.Models.TransLang> _listTransLang = new List<Translations.Models.TransLang>();
						//_listTransLang = session.CreateSQLQuery("exec GetTransLationsByDesc :description")
						//						.AddEntity(typeof(Translations.Models.TransLang))
						//						.SetParameter("description", "find your GPS  location")
						//						.List<Translations.Models.TransLang>() as List<Translations.Models.TransLang>;




						foreach (Translations.Models.TransLang itemLang in _listTransLang)
						{
							XmlElement element2 = doc.CreateElement(string.Empty, "string", string.Empty);
							element2.SetAttribute("name", itemTrans.TransKey);
							//parse ' for android
							//XmlText text1 = doc.CreateTextNode(itemLang.TransDescription.Replace("'","\\'"));
							//DEBUG
							XmlText text1 = doc.CreateTextNode(itemLang.TransLangKey.Replace("'","\\'"));
							element1.AppendChild(element2);
							element2.AppendChild(text1);

							List<Translations.Models.TransPlural> _listTransPlural = new List<Translations.Models.TransPlural>();
							_listTransPlural = session.CreateSQLQuery("exec GetPluralsByKey :trans_langKey ")
													.AddEntity(typeof(Translations.Models.TransPlural))
													.SetParameter("trans_langKey", itemLang.TransLangKey.Trim())
													.List<Translations.Models.TransPlural>() as List<Translations.Models.TransPlural>;
							if (_listTransPlural != null && _listTransPlural.Count > 0)
							{
								XmlElement element3 = doc.CreateElement(string.Empty, "plurals", string.Empty);
								element3.SetAttribute("name", itemTrans.TransKey);

								foreach (Translations.Models.TransPlural itemTransPlural in _listTransPlural)
								{
									if (itemTransPlural.IsActive)
									{
										XmlElement element4 = doc.CreateElement(string.Empty, "item", string.Empty);
										element4.SetAttribute("quantity", itemTransPlural.Plural.PluralKey);
										//XmlText text2 = doc.CreateTextNode(itemTransPlural.PluralDescription);
										//DEBUG
										XmlText text2 = doc.CreateTextNode(itemTransPlural.TransLang.TransLangKey);
										element3.AppendChild(element4);
										element4.AppendChild(text2);
									}
								}
								element1.AppendChild(element3);
							}

							List<Translations.Models.TransArray> _listTransArray = new List<Translations.Models.TransArray>();
							_listTransArray = session.CreateSQLQuery("exec GetArraysByKey :trans_langKey ")
													.AddEntity(typeof(Translations.Models.TransArray))
													.SetParameter("trans_langKey", itemLang.TransLangKey.Trim())
													.List<Translations.Models.TransArray>() as List<Translations.Models.TransArray>;

							if (_listTransArray != null && _listTransArray.Count > 0)
							{
								XmlElement element3 = doc.CreateElement(string.Empty, "string-array", string.Empty);
								element3.SetAttribute("name", itemTrans.TransKey);

								foreach (Translations.Models.TransArray itemTransArray in _listTransArray)
								{
									if (itemTransArray.IsActive)
									{
										XmlElement element4 = doc.CreateElement(string.Empty, "item", string.Empty);
										//XmlText text2 = doc.CreateTextNode(itemTransArray.ArrayDescription);
										//DEBUG
										XmlText text2 = doc.CreateTextNode(itemTransArray.TransLang.TransLangKey);
										element3.AppendChild(element4);
										element4.AppendChild(text2);
									}
								}
								element1.AppendChild(element3);
							}
						}
					}

					string pathf = Path.Combine(path, _fileFolder.FolderName);
					bool exists = Directory.Exists(pathf);
					if (!exists)
						Directory.CreateDirectory(pathf);


					//REMOVE Bom
					var settings = new XmlWriterSettings
					{
						Encoding = new UTF8Encoding(false),
						Indent = true
					};

					using (var writer = XmlWriter.Create(pathf + "\\" + string.Format("{0}", _fileFolder.FileName) + ".xml", settings))
					{
						doc.Save(writer);
						//doc.Save(pathf + "\\" + string.Format("{0}", _fileFolder.FileName) + ".xml");
					}


				}

				using (ZipFile zip = new ZipFile())
				{
					zip.AddDirectory(path);
					zip.Comment = "Resources For Android From " + System.DateTime.Now.ToString("G");

					Response.Clear();
					Response.BufferOutput = false;
					string zipName = String.Format("ZipAndroid_{0}.zip", DateTime.Now.ToString("yyyy-MMM-dd-HHmmss"));
					Response.ContentType = "application/zip";
					Response.AddHeader("content-disposition", "attachment; filename=" + zipName);
					zip.Save(Response.OutputStream);
					Response.End();
				}
				return View("Index");
			}
		}

		[Translations.Filters.AuthorizeUser]
		public ActionResult GetIosXMLFile(string tagkey, string path)
		{
			using (ISession session = NHibernateSession.OpenSession())
			{
				foreach (var _itemLanguages in Translations.Helpers.Core.ListLanguages)
				{
					Translations.Models.FileFolder _fileFolder = session.CreateSQLQuery("exec GetFileFolderByTagLang :tagKey , :langKey")
					.AddEntity(typeof(Translations.Models.FileFolder))
					.SetParameter("tagKey", tagkey.Trim())
					.SetParameter("langKey", _itemLanguages.Value.Trim())
					.UniqueResult<Translations.Models.FileFolder>();

					string pathf = Path.Combine(path, _fileFolder.FolderName);
					bool exists = Directory.Exists(pathf);
					if (!exists)
						Directory.CreateDirectory(pathf);

					List<Translations.Models.Translations> _listTrans = new List<Translations.Models.Translations>();
					_listTrans = session.CreateSQLQuery("exec GetKeysByTag :tagKey, :langKey")
											.AddEntity(typeof(Translations.Models.Translations))
											.SetParameter("tagKey", tagkey.Trim())
											.SetParameter("langKey", _itemLanguages.Value)
											.List<Translations.Models.Translations>() as List<Translations.Models.Translations>;

					System.IO.StreamWriter file = new System.IO.StreamWriter(pathf + "\\" + string.Format("{0}", _fileFolder.FileName));

					foreach (Translations.Models.Translations itemTrans in _listTrans)
					{
						List<Translations.Models.TransLang> _listTransLang = new List<Translations.Models.TransLang>();
						_listTransLang = session.CreateSQLQuery("exec GetActiveTransLag :transKey, :langKey ")
												.AddEntity(typeof(Translations.Models.TransLang))
												.SetParameter("transKey", itemTrans.TransKey)
												.SetParameter("langKey", _itemLanguages.Value)
												.List<Translations.Models.TransLang>() as List<Translations.Models.TransLang>;

						foreach (Translations.Models.TransLang itemLang in _listTransLang)
						{
							List<Translations.Models.TransArray> _listTransArray = new List<Translations.Models.TransArray>();
							_listTransArray = session.CreateSQLQuery("exec GetArraysByKey :trans_langKey ")
													.AddEntity(typeof(Translations.Models.TransArray))
													.SetParameter("trans_langKey", itemLang.TransLangKey.Trim())
													.List<Translations.Models.TransArray>() as List<Translations.Models.TransArray>;
							if (_listTransArray != null && _listTransArray.Count > 0)
							{
								int i = 1;
								foreach (Translations.Models.TransArray itemTransArray in _listTransArray)
								{
									if (itemTransArray.IsActive)
									{
										//file.WriteLine(String.Format("\"{0}_{1}\" = \"{2}\";", itemTrans.TransKey, i, itemTransArray.ArrayDescription.Replace("\n", "\\n").Replace("\r", "\\r")));
										//DEBUG
										file.WriteLine(String.Format("\"{0}_{1}\" = \"{2}\";", itemTrans.TransKey, i, itemTransArray.TransLang.TransLangKey.Replace("\n", "\\n").Replace("\r", "\\r")));
										i++;
									}
								}
							}
							else if (itemLang.TransPlural == null || itemLang.TransPlural.Count() == 0)
							{
								//file.WriteLine(String.Format("\"{0}\" = \"{1}\";", itemTrans.TransKey, itemLang.TransDescription.Replace("\n", "\\n").Replace("\r", "\\r")));
								//DEBUG
								file.WriteLine(String.Format("\"{0}\" = \"{1}\";", itemTrans.TransKey, itemLang.TransLangKey.Replace("\n", "\\n").Replace("\r", "\\r")));
							}
						}

						List<Translations.Models.Translations> _listTransPlural = new List<Translations.Models.Translations>();
						_listTransPlural = session.CreateSQLQuery("exec GetPluralsByTag :tagKey, :langKey  ")
												.AddEntity(typeof(Translations.Models.Translations))
											.SetParameter("tagKey", tagkey.Trim())
											.SetParameter("langKey", _itemLanguages.Value.Trim())
											.List<Translations.Models.Translations>() as List<Translations.Models.Translations>;

						if (_listTransPlural != null && _listTransPlural.Count > 0)
						{
							XmlDocument doc = new XmlDocument();

							//(1) the xml declaration is recommended, but not mandatory
							XmlDeclaration xmlDeclaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
							XmlElement root = doc.DocumentElement;
							doc.InsertBefore(xmlDeclaration, root);

							//(2) string.Empty makes cleaner code
							XmlElement element1 = doc.CreateElement(string.Empty, "plist", string.Empty);
							doc.AppendChild(element1);

							XmlElement element2 = doc.CreateElement(string.Empty, "dict", string.Empty);
							element1.AppendChild(element2);

							foreach (Translations.Models.Translations trans in _listTransPlural)
							{

								// <key>%@ is %lu (his/her/their) name</key>
								XmlElement element3 = doc.CreateElement(string.Empty, "key", string.Empty);
								XmlText text2 = doc.CreateTextNode(trans.TransKey);
								element3.AppendChild(text2);
								element2.AppendChild(element3);

								XmlElement element4 = doc.CreateElement(string.Empty, "dict", string.Empty);
								element2.AppendChild(element4);

								XmlElement element5 = doc.CreateElement(string.Empty, "key", string.Empty);
								XmlText tex3 = doc.CreateTextNode("NSStringLocalizedFormatKey");
								element5.AppendChild(tex3);
								element4.AppendChild(element5);

								XmlElement element6 = doc.CreateElement(string.Empty, "string", string.Empty);
								XmlText tex4 = doc.CreateTextNode("%#@unit@");
								element6.AppendChild(tex4);
								element4.AppendChild(element6);

								XmlElement element7 = doc.CreateElement(string.Empty, "key", string.Empty);
								XmlText tex5 = doc.CreateTextNode("unit");
								element7.AppendChild(tex5);
								element4.AppendChild(element7);

								XmlElement element8 = doc.CreateElement(string.Empty, "dict", string.Empty);
								element4.AppendChild(element8);

								XmlElement element9 = doc.CreateElement(string.Empty, "key", string.Empty);
								XmlText tex6 = doc.CreateTextNode("NSStringFormatSpecTypeKey");
								element9.AppendChild(tex6);
								element8.AppendChild(element9);

								XmlElement element10 = doc.CreateElement(string.Empty, "string", string.Empty);
								XmlText tex7 = doc.CreateTextNode("NSStringPluralRuleType");
								element10.AppendChild(tex7);
								element8.AppendChild(element10);

								XmlElement element11 = doc.CreateElement(string.Empty, "key", string.Empty);
								XmlText tex8 = doc.CreateTextNode("NSStringFormatValueTypeKey");
								element11.AppendChild(tex8);
								element8.AppendChild(element11);

								XmlElement element12 = doc.CreateElement(string.Empty, "string", string.Empty);
								XmlText tex9 = doc.CreateTextNode("lu");
								element12.AppendChild(tex9);
								element8.AppendChild(element12);

								List<Translations.Models.TransPlural> _listPlurals = new List<Translations.Models.TransPlural>();
								_listPlurals = session.CreateSQLQuery("exec GetPluralsByTransKey :transKey  ")
														.AddEntity(typeof(Translations.Models.TransPlural))
													.SetParameter("transKey", trans.TransKey)
													.List<Translations.Models.TransPlural>() as List<Translations.Models.TransPlural>;

								foreach (Translations.Models.TransPlural itemTransPlural in _listPlurals)
								{
									XmlElement element13 = doc.CreateElement(string.Empty, "key", string.Empty);
									XmlText tex10 = doc.CreateTextNode(itemTransPlural.Plural.PluralKey);
									element13.AppendChild(tex10);
									element8.AppendChild(element13);

									XmlElement element14 = doc.CreateElement(string.Empty, "string", string.Empty);
									//XmlText tex11 = doc.CreateTextNode(itemTransPlural.PluralDescription);
									//DEBUG
									XmlText tex11 = doc.CreateTextNode(itemTransPlural.TransLang.TransLangKey);
									element14.AppendChild(tex11);
									element8.AppendChild(element14);
								}
							}
							doc.Save(pathf + "\\" + string.Format("{0}", _fileFolder.FileName) + "dict");
						}
					}
					file.Close();
				}

				using (ZipFile zip = new ZipFile())
				{
					zip.AddDirectory(path);
					zip.Comment = "Resources For IOS From " + System.DateTime.Now.ToString("G");

					Response.Clear();
					Response.BufferOutput = false;
					string zipName = String.Format("ZipIos_{0}.zip", DateTime.Now.ToString("yyyy-MMM-dd-HHmmss"));
					Response.ContentType = "application/zip";
					Response.AddHeader("content-disposition", "attachment; filename=" + zipName);
					zip.Save(Response.OutputStream);
					Response.End();
				}
				return View("Index");
			}
		}

		[Translations.Filters.AuthorizeUser]
		public ActionResult GetCsv(DateTime cDate, string type)
		{
			using (ISession session = NHibernateSession.OpenSession())
			{
				IList<TransLangCsv> _trans = session.CreateSQLQuery("exec GetTransLationsByDate :date")
												.AddEntity(typeof(Translations.Models.TransLangCsv))
											.SetParameter("date", cDate.ToString("yyyy-MM-dd"))
											.List<Translations.Models.TransLangCsv>() as List<Translations.Models.TransLangCsv>;


				//get translations with key equal description
				//IList<TransLangCsv> _trans = session.CreateSQLQuery("exec GetTransLationsWhereKeyEqualsDescription")
				//				.AddEntity(typeof(Translations.Models.TransLangCsv))
				//			.List<Translations.Models.TransLangCsv>() as List<Translations.Models.TransLangCsv>;




				//IList<TransLangCsv> _trans = session.QueryOver<TransLangCsv>().List<TransLangCsv>();

				foreach (TransLangCsv item in _trans)
				{
					item.Few = item.TransPlural.ToList().Find(x => x.Plural.PluralKey.Equals("few", StringComparison.InvariantCultureIgnoreCase)) != null ? item.TransPlural.ToList().Find(x => x.Plural.PluralKey.Equals("few", StringComparison.InvariantCultureIgnoreCase)).PluralDescription : "";
					item.Many = item.TransPlural.ToList().Find(x => x.Plural.PluralKey.Equals("many", StringComparison.InvariantCultureIgnoreCase)) != null ? item.TransPlural.ToList().Find(x => x.Plural.PluralKey.Equals("many", StringComparison.InvariantCultureIgnoreCase)).PluralDescription : "";
					item.One = item.TransPlural.ToList().Find(x => x.Plural.PluralKey.Equals("one", StringComparison.InvariantCultureIgnoreCase)) != null ? item.TransPlural.ToList().Find(x => x.Plural.PluralKey.Equals("one", StringComparison.InvariantCultureIgnoreCase)).PluralDescription : "";
					item.Other = item.TransPlural.ToList().Find(x => x.Plural.PluralKey.Equals("other", StringComparison.InvariantCultureIgnoreCase)) != null ? item.TransPlural.ToList().Find(x => x.Plural.PluralKey.Equals("other", StringComparison.InvariantCultureIgnoreCase)).PluralDescription : "";
					item.Two = item.TransPlural.ToList().Find(x => x.Plural.PluralKey.Equals("two", StringComparison.InvariantCultureIgnoreCase)) != null ? item.TransPlural.ToList().Find(x => x.Plural.PluralKey.Equals("two", StringComparison.InvariantCultureIgnoreCase)).PluralDescription : "";
					item.Zero = item.TransPlural.ToList().Find(x => x.Plural.PluralKey.Equals("zero", StringComparison.InvariantCultureIgnoreCase)) != null ? item.TransPlural.ToList().Find(x => x.Plural.PluralKey.Equals("zero", StringComparison.InvariantCultureIgnoreCase)).PluralDescription : "";
				}

				if (_trans == null || _trans.Count == 0)
					return View("Index");

				//if (type.Equals("xlsx", StringComparison.InvariantCultureIgnoreCase))
				//{

				//	using (XLWorkbook wb = new XLWorkbook())
				//	{
				//		DataTable dt = Translations.Helpers.Core.ConvertToDataTable(_trans);
				//		wb.Worksheets.Add(dt, "Translations");
				//		Response.Clear();
				//		Response.Buffer = true;
				//		Response.Charset = "";
				//		Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
				//		Response.AddHeader("content-disposition", "attachment;filename=Translations.xlsx");
				//		using (MemoryStream MyMemoryStream = new MemoryStream())
				//		{
				//			wb.SaveAs(MyMemoryStream);
				//			MyMemoryStream.WriteTo(Response.OutputStream);
				//			Response.Flush();
				//			Response.End();
				//		}
				//	}
				//}
				if (type.Equals("csv", StringComparison.InvariantCultureIgnoreCase))
				{
					Translations.Helpers.CsvExport<TransLangCsv> csv = new Translations.Helpers.CsvExport<TransLangCsv>(_trans.ToList());
					Response.ClearContent();
					Response.Buffer = true;
					Response.AddHeader("content-disposition", "attachment; filename=Translations.csv");
					Response.ContentType = "application/ms-excel";
					Response.Charset = "";
					Response.Output.Write(csv.Export());
					Response.Flush();
					Response.End();
				}
				//else
				//{
				//	GridView gv = new GridView();
				//	gv.DataSource = _trans;
				//	gv.DataBind();
				//	Response.ClearContent();
				//	Response.Buffer = true;
				//	Response.AddHeader("content-disposition", "attachment; filename=Translations.xls");
				//	Response.ContentType = "application/ms-excel";
				//	Response.Charset = "";
				//	StringWriter sw = new StringWriter();
				//	HtmlTextWriter htw = new HtmlTextWriter(sw);
				//	gv.RenderControl(htw);
				//	Response.Output.Write(sw.ToString());
				//	Response.Flush();
				//	Response.End();
				//}

				return View("Index");
			}
		}

		[HttpPost]
		[Translations.Filters.AuthorizeUser]
		public ActionResult Import(HttpPostedFileBase file)
		{
			string fileLocation = string.Empty;
			if (Request.Files["file"].ContentLength > 0 && file != null && file.ContentLength > 0)
			{
				string fileExtension = System.IO.Path.GetExtension(Request.Files["file"].FileName);

				if (fileExtension.Equals(".xls", StringComparison.InvariantCultureIgnoreCase) || fileExtension.Equals(".xlsx", StringComparison.InvariantCultureIgnoreCase) || fileExtension.Equals(".csv", StringComparison.InvariantCultureIgnoreCase))
				{
					fileLocation = Server.MapPath("~/Content/") + Request.Files["file"].FileName;
					if (System.IO.File.Exists(fileLocation))
					{
						System.IO.File.Delete(fileLocation);
					}
					Request.Files["file"].SaveAs(fileLocation);

					//call import CSV
					ImportFromExcel("Translations", fileLocation, fileExtension);

				}
				else if (fileExtension.Equals(".xml", StringComparison.InvariantCultureIgnoreCase))
				{
					//call import XML (android schema)
					ImportFromXml(file);
				}
			}
			return View("Index", null);
		}


		public void ImportFromExcel(string name, string fileLocation, string type)
		{
			List<TransLangCsv> translations = new List<TransLangCsv>();
			User _user = Helpers.Core.getUser;

			#region ExcelFormat
			if (type.Equals(".xlsx", StringComparison.InvariantCultureIgnoreCase))
			{
				//string sheetName = name;
				//var excelFile = new ExcelQueryFactory(fileLocation);
				//excelFile.DatabaseEngine = DatabaseEngine.Ace;
				//translations = (from a in excelFile.Worksheet<TransLangCsv>(0).Where(x => !string.IsNullOrEmpty(x.TransLangKey)) select a).ToList();
			}
			else if (type.Equals(".csv", StringComparison.InvariantCultureIgnoreCase))
			{
				var translationsLines = System.IO.File.ReadAllLines(fileLocation).Skip(1);
				foreach (string line in translationsLines)
				{
					List<string> obj = Helpers.Core.SplitCSV(line);

					if (obj.Count >= 10)
					{
						translations.Add(new TransLangCsv()
						{
							TransLangKey = obj[0],
							Language = obj[1],
							TransDescription = obj[2],
							Few = obj[3],
							Many = obj[4],
							One = obj[5],
							Other = obj[6],
							Two = obj[7],
							Zero = obj[8],
							CreationDate = Convert.ToDateTime(obj[9])
						});
					}
				}
			}
			else if (type.Equals(".xls", StringComparison.InvariantCultureIgnoreCase))
			{
				//string newPath = Server.MapPath("~/Content/") + "temp.xlsx";

				//if (System.IO.File.Exists(newPath))
				//{
				//	System.IO.File.Delete(newPath);
				//}

				//Microsoft.Office.Interop.Excel.Application excelApp = new Microsoft.Office.Interop.Excel.Application();
				//excelApp.Visible = false;
				//Microsoft.Office.Interop.Excel.Workbook eWorkbook = excelApp.Workbooks.Open(fileLocation, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
				//eWorkbook.SaveAs(newPath, Microsoft.Office.Interop.Excel.XlFileFormat.xlOpenXMLWorkbook, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Microsoft.Office.Interop.Excel.XlSaveAsAccessMode.xlNoChange, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
				//eWorkbook.Close(false, Type.Missing, Type.Missing);

				//string sheetName = name;
				//var excelFile = new ExcelQueryFactory(newPath);
				//excelFile.DatabaseEngine = DatabaseEngine.Ace;
				//translations = (from a in excelFile.Worksheet<TransLangCsv>(0).Where(x => !string.IsNullOrEmpty(x.TransLangKey)) select a).ToList();
			}
			#endregion

			int xpto = translations.Count;

			using (IStatelessSession session = NHibernateSession.OpenStatelessSession())
			{
				using (ITransaction transaction = session.BeginTransaction())
				{
					foreach (TransLangCsv item in translations)
					{
						Models.TransLang transl = session.Get<Models.TransLang>(item.TransLangKey);

						if (transl != null)
						{
							//Edit Translation
							transl.TransDescription = item.TransDescription;
							#region Plurals
							if (!string.IsNullOrEmpty(item.One))
							{
								string key = string.Format("{0}_{1}", transl.TransLangKey.Trim(), "one");
								Models.TransPlural transP = session.Get<Models.TransPlural>(key);
								if (transP == null)
								{

									TransPlural pl = new TransPlural();
									pl.IsActive = true;
									pl.PluralDescription = item.One;
									pl.Plural = session.Get<Models.Plural>("one");
									pl.TransLang = transl;
									pl.TransPluralKey = key;
									string itemPluralHash = Helpers.Hash.CreateHash(pl.ToHash);
									pl.HashCurrent = itemPluralHash;
									session.Insert(pl);

									#region Logs Trans Plural
									if (Translations.Helpers.Core.LogsActive)
									{
										TransPluralLogs tpl = new TransPluralLogs();
										tpl.HashKey = itemPluralHash;
										tpl.UserName = _user.Name;
										tpl.Action = "Create";
										tpl.TransLangKey = pl.TransLang.TransLangKey;
										tpl.TransPluralKey = pl.TransPluralKey;
										tpl.PluralDescription = pl.PluralDescription;
										tpl.PluralKey = pl.Plural.PluralKey;
										tpl.IsActive = pl.IsActive;
										tpl.InsertDate = DateTime.Now;
										session.Insert(tpl);
									}
									#endregion
								}
								else
								{
									//Updade
									transP.IsActive = true;
									transP.PluralDescription = item.One;

									string itemPluralHash = Helpers.Hash.CreateHash(transP.ToHash);
									if (string.IsNullOrEmpty(transP.HashCurrent) || !Helpers.Hash.ValidateHash(transP.ToHash, transP.HashCurrent))
									{
										transP.HashCurrent = itemPluralHash;
										session.Update(transP);
										#region Logs Plural
										if (Translations.Helpers.Core.LogsActive)
										{

											TransPluralLogs tpl = new TransPluralLogs();
											tpl.HashKey = itemPluralHash;
											tpl.UserName = _user.Name;
											tpl.TransLangKey = transP.TransLang.TransLangKey;
											tpl.Action = "Edit";
											tpl.TransPluralKey = transP.TransPluralKey;
											tpl.PluralKey = transP.Plural.PluralKey;
											tpl.PluralDescription = transP.PluralDescription;
											tpl.IsActive = transP.IsActive;
											tpl.InsertDate = DateTime.Now;
											session.Insert(tpl);
										}
										#endregion
									}
								}
							}
							if (!string.IsNullOrEmpty(item.Many))
							{
								string key = string.Format("{0}_{1}", transl.TransLangKey.Trim(), "many");
								Models.TransPlural transP = session.Get<Models.TransPlural>(key);
								if (transP == null)
								{
									TransPlural pl = new TransPlural();
									pl.IsActive = true;
									pl.PluralDescription = item.Many;
									pl.Plural = session.Get<Models.Plural>("many");
									pl.TransLang = transl;
									pl.TransPluralKey = key;
									string itemPluralHash = Helpers.Hash.CreateHash(pl.ToHash);
									pl.HashCurrent = itemPluralHash;
									session.Insert(pl);

									#region Logs Trans Plural
									if (Translations.Helpers.Core.LogsActive)
									{
										TransPluralLogs tpl = new TransPluralLogs();
										tpl.HashKey = itemPluralHash;
										tpl.UserName = _user.Name;
										tpl.Action = "Create";
										tpl.TransLangKey = pl.TransLang.TransLangKey;
										tpl.TransPluralKey = pl.TransPluralKey;
										tpl.PluralDescription = pl.PluralDescription;
										tpl.PluralKey = pl.Plural.PluralKey;
										tpl.IsActive = pl.IsActive;
										tpl.InsertDate = DateTime.Now;
										session.Insert(tpl);
									}
									#endregion

								}
								else
								{
									//Updade
									transP.IsActive = true;
									transP.PluralDescription = item.Many;

									string itemPluralHash = Helpers.Hash.CreateHash(transP.ToHash);
									if (string.IsNullOrEmpty(transP.HashCurrent) || !Helpers.Hash.ValidateHash(transP.ToHash, transP.HashCurrent))
									{
										transP.HashCurrent = itemPluralHash;
										session.Update(transP);

										#region Logs Plural
										if (Translations.Helpers.Core.LogsActive)
										{

											TransPluralLogs tpl = new TransPluralLogs();
											tpl.HashKey = itemPluralHash;
											tpl.UserName = _user.Name;
											tpl.TransLangKey = transP.TransLang.TransLangKey;
											tpl.Action = "Edit";
											tpl.TransPluralKey = transP.TransPluralKey;
											tpl.PluralKey = transP.Plural.PluralKey;
											tpl.PluralDescription = transP.PluralDescription;
											tpl.IsActive = transP.IsActive;
											tpl.InsertDate = DateTime.Now;
											session.Insert(tpl);
										}
										#endregion
									}
								}
							}
							if (!string.IsNullOrEmpty(item.Few))
							{
								string key = string.Format("{0}_{1}", transl.TransLangKey.Trim(), "few");
								Models.TransPlural transP = session.Get<Models.TransPlural>(key);
								if (transP == null)
								{
									TransPlural pl = new TransPlural();
									pl.IsActive = true;
									pl.PluralDescription = item.Few;
									pl.Plural = session.Get<Models.Plural>("few");
									pl.TransLang = transl;
									pl.TransPluralKey = key;
									string itemPluralHash = Helpers.Hash.CreateHash(pl.ToHash);
									pl.HashCurrent = itemPluralHash;
									session.Insert(pl);

									#region Logs Trans Plural
									if (Translations.Helpers.Core.LogsActive)
									{
										TransPluralLogs tpl = new TransPluralLogs();
										tpl.HashKey = itemPluralHash;
										tpl.UserName = _user.Name;
										tpl.Action = "Create";
										tpl.TransLangKey = pl.TransLang.TransLangKey;
										tpl.TransPluralKey = pl.TransPluralKey;
										tpl.PluralDescription = pl.PluralDescription;
										tpl.PluralKey = pl.Plural.PluralKey;
										tpl.IsActive = pl.IsActive;
										tpl.InsertDate = DateTime.Now;
										session.Insert(tpl);
									}
									#endregion
								}
								else
								{
									//Updade
									transP.IsActive = true;
									transP.PluralDescription = item.Few;

									string itemPluralHash = Helpers.Hash.CreateHash(transP.ToHash);
									if (string.IsNullOrEmpty(transP.HashCurrent) || !Helpers.Hash.ValidateHash(transP.ToHash, transP.HashCurrent))
									{
										transP.HashCurrent = itemPluralHash;
										session.Update(transP);

										#region Logs Plural
										if (Translations.Helpers.Core.LogsActive)
										{

											TransPluralLogs tpl = new TransPluralLogs();
											tpl.HashKey = itemPluralHash;
											tpl.UserName = _user.Name;
											tpl.TransLangKey = transP.TransLang.TransLangKey;
											tpl.Action = "Edit";
											tpl.TransPluralKey = transP.TransPluralKey;
											tpl.PluralKey = transP.Plural.PluralKey;
											tpl.PluralDescription = transP.PluralDescription;
											tpl.IsActive = transP.IsActive;
											tpl.InsertDate = DateTime.Now;
											session.Insert(tpl);
										}
										#endregion
									}
								}
							}
							if (!string.IsNullOrEmpty(item.Other))
							{
								string key = string.Format("{0}_{1}", transl.TransLangKey.Trim(), "other");
								Models.TransPlural transP = session.Get<Models.TransPlural>(key);
								if (transP == null)
								{
									TransPlural pl = new TransPlural();
									pl.IsActive = true;
									pl.PluralDescription = item.Other;
									pl.Plural = session.Get<Models.Plural>("other");
									pl.TransLang = transl;
									pl.TransPluralKey = key;
									string itemPluralHash = Helpers.Hash.CreateHash(pl.ToHash);
									pl.HashCurrent = itemPluralHash;
									session.Insert(pl);

									#region Logs Trans Plural
									if (Translations.Helpers.Core.LogsActive)
									{
										TransPluralLogs tpl = new TransPluralLogs();
										tpl.HashKey = itemPluralHash;
										tpl.UserName = _user.Name;
										tpl.Action = "Create";
										tpl.TransLangKey = pl.TransLang.TransLangKey;
										tpl.TransPluralKey = pl.TransPluralKey;
										tpl.PluralDescription = pl.PluralDescription;
										tpl.PluralKey = pl.Plural.PluralKey;
										tpl.IsActive = pl.IsActive;
										tpl.InsertDate = DateTime.Now;
										session.Insert(tpl);
									}
									#endregion
								}
								else
								{
									//Updade
									transP.IsActive = true;
									transP.PluralDescription = item.Other;

									string itemPluralHash = Helpers.Hash.CreateHash(transP.ToHash);
									if (string.IsNullOrEmpty(transP.HashCurrent) || !Helpers.Hash.ValidateHash(transP.ToHash, transP.HashCurrent))
									{
										transP.HashCurrent = itemPluralHash;
										session.Update(transP);

										#region Logs Plural
										if (Translations.Helpers.Core.LogsActive)
										{

											TransPluralLogs tpl = new TransPluralLogs();
											tpl.HashKey = itemPluralHash;
											tpl.UserName = _user.Name;
											tpl.TransLangKey = transP.TransLang.TransLangKey;
											tpl.Action = "Edit";
											tpl.TransPluralKey = transP.TransPluralKey;
											tpl.PluralKey = transP.Plural.PluralKey;
											tpl.PluralDescription = transP.PluralDescription;
											tpl.IsActive = transP.IsActive;
											tpl.InsertDate = DateTime.Now;
											session.Insert(tpl);
										}
										#endregion
									}
								}
							}
							if (!string.IsNullOrEmpty(item.Two))
							{
								string key = string.Format("{0}_{1}", transl.TransLangKey.Trim(), "two");
								Models.TransPlural transP = session.Get<Models.TransPlural>(key);
								if (transP == null)
								{
									TransPlural pl = new TransPlural();
									pl.IsActive = true;
									pl.PluralDescription = item.Two;
									pl.Plural = session.Get<Models.Plural>("two");
									pl.TransLang = transl;
									pl.TransPluralKey = key;
									string itemPluralHash = Helpers.Hash.CreateHash(pl.ToHash);
									pl.HashCurrent = itemPluralHash;
									session.Insert(pl);

									#region Logs Trans Plural
									if (Translations.Helpers.Core.LogsActive)
									{
										TransPluralLogs tpl = new TransPluralLogs();
										tpl.HashKey = itemPluralHash;
										tpl.UserName = _user.Name;
										tpl.Action = "Create";
										tpl.TransLangKey = pl.TransLang.TransLangKey;
										tpl.TransPluralKey = pl.TransPluralKey;
										tpl.PluralDescription = pl.PluralDescription;
										tpl.PluralKey = pl.Plural.PluralKey;
										tpl.IsActive = pl.IsActive;
										tpl.InsertDate = DateTime.Now;
										session.Insert(tpl);
									}
									#endregion
								}
								else
								{
									//Updade
									transP.IsActive = true;
									transP.PluralDescription = item.Two;

									string itemPluralHash = Helpers.Hash.CreateHash(transP.ToHash);
									if (string.IsNullOrEmpty(transP.HashCurrent) || !Helpers.Hash.ValidateHash(transP.ToHash, transP.HashCurrent))
									{
										transP.HashCurrent = itemPluralHash;
										session.Update(transP); ;

										#region Logs Plural
										if (Translations.Helpers.Core.LogsActive)
										{

											TransPluralLogs tpl = new TransPluralLogs();
											tpl.HashKey = itemPluralHash;
											tpl.UserName = _user.Name;
											tpl.TransLangKey = transP.TransLang.TransLangKey;
											tpl.Action = "Edit";
											tpl.TransPluralKey = transP.TransPluralKey;
											tpl.PluralKey = transP.Plural.PluralKey;
											tpl.PluralDescription = transP.PluralDescription;
											tpl.IsActive = transP.IsActive;
											tpl.InsertDate = DateTime.Now;
											session.Insert(tpl);
										}
										#endregion
									}
								}
							}
							if (!string.IsNullOrEmpty(item.Zero))
							{
								string key = string.Format("{0}_{1}", transl.TransLangKey.Trim(), "zero");
								Models.TransPlural transP = session.Get<Models.TransPlural>(key);
								if (transP == null)
								{
									TransPlural pl = new TransPlural();
									pl.IsActive = true;
									pl.PluralDescription = item.Zero;
									pl.Plural = session.Get<Models.Plural>("zero");
									pl.TransLang = transl;
									pl.TransPluralKey = key;
									string itemPluralHash = Helpers.Hash.CreateHash(pl.ToHash);
									pl.HashCurrent = itemPluralHash;
									session.Insert(pl);

									#region Logs Trans Plural
									if (Translations.Helpers.Core.LogsActive)
									{
										TransPluralLogs tpl = new TransPluralLogs();
										tpl.HashKey = itemPluralHash;
										tpl.UserName = _user.Name;
										tpl.Action = "Create";
										tpl.TransLangKey = pl.TransLang.TransLangKey;
										tpl.TransPluralKey = pl.TransPluralKey;
										tpl.PluralDescription = pl.PluralDescription;
										tpl.PluralKey = pl.Plural.PluralKey;
										tpl.IsActive = pl.IsActive;
										tpl.InsertDate = DateTime.Now;
										session.Insert(tpl);
									}
									#endregion
								}
								else
								{
									//Updade
									transP.IsActive = true;
									transP.PluralDescription = item.Zero;

									string itemPluralHash = Helpers.Hash.CreateHash(transP.ToHash);
									if (string.IsNullOrEmpty(transP.HashCurrent) || !Helpers.Hash.ValidateHash(transP.ToHash, transP.HashCurrent))
									{
										transP.HashCurrent = itemPluralHash;
										session.Update(transP);

										#region Logs Plural
										if (Translations.Helpers.Core.LogsActive)
										{

											TransPluralLogs tpl = new TransPluralLogs();
											tpl.HashKey = itemPluralHash;
											tpl.UserName = _user.Name;
											tpl.TransLangKey = transP.TransLang.TransLangKey;
											tpl.Action = "Edit";
											tpl.TransPluralKey = transP.TransPluralKey;
											tpl.PluralKey = transP.Plural.PluralKey;
											tpl.PluralDescription = transP.PluralDescription;
											tpl.IsActive = transP.IsActive;
											tpl.InsertDate = DateTime.Now;
											session.Insert(tpl);
										}
										#endregion
									}
								}
							}
							#endregion


							string itemTransLangHash = Helpers.Hash.CreateHash(transl.ToHash);
							if (string.IsNullOrEmpty(transl.HashCurrent) || !Helpers.Hash.ValidateHash(transl.ToHash, transl.HashCurrent))
							{
								transl.HashCurrent = itemTransLangHash;
								session.Update(transl);
								#region Logs Trans Lang
								if (Translations.Helpers.Core.LogsActive)
								{
									TransLangLogs tll = new TransLangLogs();
									tll.HashKey = itemTransLangHash;
									tll.UserName = _user.Name;
									tll.Action = "Edit";
									tll.TransLangKey = transl.TransLangKey;
									tll.LangKey = transl.Languages.LangKey.Trim();
									tll.TransDescription = transl.TransDescription;
									tll.TransKey = transl.Translations.TransKey.Trim();
									tll.IsActive = transl.IsActive;
									tll.InsertDate = DateTime.Now;
									session.Insert(tll);
								}
								#endregion
							}



							
						}
						else
						{
							//Create Translation
						}
					}
					transaction.Commit();
				}
			}


		}

		public void ImportFromXml(HttpPostedFileBase file)
		{
			if (file.ContentType == "text/xml")
			{
				var document = new XmlDocument();
				document.Load(file.InputStream);
				using (IStatelessSession session = NHibernateSession.OpenStatelessSession())
				{
					using (ITransaction transaction = session.BeginTransaction())
					{
						var translations = document.SelectNodes("//string");
						//Region Translations
						#region trans
						foreach (XmlNode item in translations)
						{
							if (item.Attributes != null && item.Attributes.Count > 0 && item.Attributes["name"] != null && !string.IsNullOrEmpty(item.Attributes["name"].Value))
							{
								Models.Translations trans = session.Get<Models.Translations>(item.Attributes["name"].Value);
								if (trans == null)
								{
									trans = new Models.Translations() { TransKey = item.Attributes["name"].Value, CreationDate = DateTime.Now };
									session.Insert(trans);
								}

								foreach (var tag in Translations.Helpers.Core.ListTags)
								{
									Models.TransTag transT = session.Get<Models.TransTag>(string.Format("{0}_{1}", trans.TransKey, tag.Value));
									if (transT == null)
									{
										transT = new TransTag();
										transT.Translations = trans;
										transT.Tags = new Tags() { TagKey = tag.Value };
										transT.TransTagKey = string.Format("{0}_{1}", trans.TransKey, tag.Value);
										transT.IsActive = true;
										session.Insert(transT);
									}
								}

								Models.Languages lang = session.Get<Models.Languages>("en");
								if (lang == null)
								{
									lang = new Languages();
									lang.LangKey = "en";
									lang.LangText = "English";
									lang.IsActive = true;
									session.Insert(lang);
								}
								Models.TransLang transl = session.Get<Models.TransLang>(string.Format("{0}_{1}", trans.TransKey, lang.LangKey));
								if (transl == null)
								{
									transl = new TransLang();
									transl.TransLangKey = string.Format("{0}_{1}", trans.TransKey.Trim(), lang.LangKey.Trim());
									transl.Languages = new Languages() { LangKey = lang.LangKey, LangText = lang.LangText, IsActive = lang.IsActive };
									transl.Translations = new Translations.Models.Translations() { TransKey = trans.TransKey };
									transl.TransDescription = item.InnerText;
									transl.IsActive = true;
									session.Insert(transl);
								}
								else
								{
									transl.TransDescription = item.InnerText;
									session.Update(transl);
								}
							}
						}
						#endregion

						var plurals = document.SelectNodes("//plurals");
						//region Plurals
						#region plurals
						foreach (XmlNode itemPlural in plurals)
						{
							if (itemPlural.Attributes != null && itemPlural.Attributes.Count > 0 && itemPlural.Attributes["name"] != null && !string.IsNullOrEmpty(itemPlural.Attributes["name"].Value))
							{
								Models.Translations trans = session.Get<Models.Translations>(itemPlural.Attributes["name"].Value);

								if (trans == null)
								{
									trans = new Models.Translations() { TransKey = itemPlural.Attributes["name"].Value, CreationDate = DateTime.Now };
									session.Insert(trans);

								}
								foreach (var tag in Translations.Helpers.Core.ListTags)
								{
									Models.TransTag transT = session.Get<Models.TransTag>(string.Format("{0}_{1}", trans.TransKey, tag.Value));
									if (transT == null)
									{
										transT = new TransTag();
										transT.Translations = trans;
										transT.Tags = new Tags() { TagKey = tag.Value };
										transT.TransTagKey = string.Format("{0}_{1}", trans.TransKey, tag.Value);
										transT.IsActive = true;
										session.Insert(transT);
									}
									Models.Languages lang = session.Get<Models.Languages>("en");
									if (lang == null)
									{
										lang = new Languages();
										lang.LangKey = "en";
										lang.LangText = "English";
										lang.IsActive = true;
										session.Insert(lang);
									}
									Models.TransLang transl = session.Get<Models.TransLang>(string.Format("{0}_{1}", trans.TransKey, lang.LangKey));
									if (transl == null)
									{
										transl = new TransLang();
										transl.TransLangKey = string.Format("{0}_{1}", trans.TransKey, lang.LangKey);
										transl.Languages = new Languages() { LangKey = lang.LangKey, LangText = lang.LangText, IsActive = lang.IsActive };
										transl.Translations = new Translations.Models.Translations() { TransKey = trans.TransKey };
										transl.TransDescription = itemPlural.Attributes["name"].Value;
										transl.IsActive = true;
										session.Insert(transl);
									}
									else
									{
										transl.TransDescription = itemPlural.Attributes["name"].Value;
										session.Update(transl);
									}


									if (itemPlural.ChildNodes != null && itemPlural.ChildNodes.Count > 0)
									{
										foreach (XmlNode itemP in itemPlural.ChildNodes)
										{
											if (itemP.Attributes != null && itemP.Attributes.Count > 0 && itemP.Attributes["quantity"] != null && !string.IsNullOrEmpty(itemP.Attributes["quantity"].Value))
											{
												Models.TransPlural transP = session.Get<Models.TransPlural>(string.Format("{0}_{1}", transl.TransLangKey, itemP.Attributes["quantity"].Value));
												if (transP == null)
												{
													transP = new TransPlural();
													transP.TransPluralKey = string.Format("{0}_{1}", transl.TransLangKey.Trim(), itemP.Attributes["quantity"].Value.Trim());
													transP.TransLang = transl;
													transP.PluralDescription = itemP.InnerText;
													Models.Plural plural = session.Get<Models.Plural>(itemP.Attributes["quantity"].Value);
													if (plural == null)
													{
														plural = new Plural() { IsActive = true, PluralKey = itemP.Attributes["quantity"].Value, PluralText = itemP.Attributes["quantity"].Value };
														session.Insert(plural);
													}
													transP.Plural = plural;
													transP.IsActive = true;
													session.Insert(transP);
												}
												else
												{
													transP.PluralDescription = itemP.InnerText;
													session.Update(transP);
												}
											}
										}
									}
								}
							}
						}
						#endregion

						var stringArray = document.SelectNodes("//string-array");
						//region String Array
						#region stringArray
						foreach (XmlNode sArray in stringArray)
						{
							if (sArray.Attributes != null && sArray.Attributes.Count > 0 && sArray.Attributes["name"] != null && !string.IsNullOrEmpty(sArray.Attributes["name"].Value))
							{
								Models.Translations trans = session.Get<Models.Translations>(sArray.Attributes["name"].Value);

								if (trans == null)
								{
									trans = new Models.Translations() { TransKey = sArray.Attributes["name"].Value, CreationDate = DateTime.Now };
									session.Insert(trans);

								}
								foreach (var tag in Translations.Helpers.Core.ListTags)
								{
									Models.TransTag transT = session.Get<Models.TransTag>(string.Format("{0}_{1}", trans.TransKey, tag.Value));
									if (transT == null)
									{
										transT = new TransTag();
										transT.Translations = trans;
										transT.Tags = new Tags() { TagKey = tag.Value };
										transT.TransTagKey = string.Format("{0}_{1}", trans.TransKey, tag.Value);
										transT.IsActive = true;
										session.Insert(transT);
									}
									Models.Languages lang = session.Get<Models.Languages>("en");
									if (lang == null)
									{
										lang = new Languages();
										lang.LangKey = "en";
										lang.LangText = "English";
										lang.IsActive = true;
										session.Insert(lang);
									}
									Models.TransLang transl = session.Get<Models.TransLang>(string.Format("{0}_{1}", trans.TransKey, lang.LangKey));
									if (transl == null)
									{
										transl = new TransLang();
										transl.TransLangKey = string.Format("{0}_{1}", trans.TransKey.Trim(), lang.LangKey.Trim());
										transl.Languages = new Languages() { LangKey = lang.LangKey, LangText = lang.LangText, IsActive = lang.IsActive };
										transl.Translations = new Translations.Models.Translations() { TransKey = trans.TransKey };
										transl.TransDescription = sArray.Attributes["name"].Value;
										transl.IsActive = true;
										session.Insert(transl);
									}
									else
									{
										transl.TransDescription = sArray.Attributes["name"].Value;
										session.Update(transl);
									}


									if (sArray.ChildNodes != null && sArray.ChildNodes.Count > 0)
									{
										foreach (XmlNode itemS in sArray.ChildNodes)
										{
											TransArray _trans = session.CreateSQLQuery("exec GetTransArray :trans_langKey, :arrayDescription")
																.AddEntity(typeof(TransArray))
																.SetParameter("trans_langKey", transl.TransLangKey.Trim())
																.SetParameter("arrayDescription", itemS.InnerText.Trim()).UniqueResult<TransArray>();

											if (_trans == null)
											{
												_trans = new TransArray();
												_trans.TransLang = transl;
												_trans.ArrayDescription = itemS.InnerText.Trim();
												_trans.IsActive = true;
												session.Insert(_trans);
											}
											else
											{
												_trans.ArrayDescription = itemS.InnerText.Trim();
												session.Update(_trans);
											}
										}
									}
								}
							}
						}
						#endregion

						var arrays = document.SelectNodes("//array");
						//region Arrays
						#region Arrays
						foreach (XmlNode sArray in arrays)
						{
							if (sArray.Attributes != null && sArray.Attributes.Count > 0 && sArray.Attributes["name"] != null && !string.IsNullOrEmpty(sArray.Attributes["name"].Value))
							{
								Models.Translations trans = session.Get<Models.Translations>(sArray.Attributes["name"].Value);

								if (trans == null)
								{
									trans = new Models.Translations() { TransKey = sArray.Attributes["name"].Value, CreationDate = DateTime.Now };
									session.Insert(trans);

								}
								foreach (var tag in Translations.Helpers.Core.ListTags)
								{
									Models.TransTag transT = session.Get<Models.TransTag>(string.Format("{0}_{1}", trans.TransKey, tag.Value));
									if (transT == null)
									{
										transT = new TransTag();
										transT.Translations = trans;
										transT.Tags = new Tags() { TagKey = tag.Value };
										transT.TransTagKey = string.Format("{0}_{1}", trans.TransKey, tag.Value);
										transT.IsActive = true;
										session.Insert(transT);
									}
									Models.Languages lang = session.Get<Models.Languages>("en");
									if (lang == null)
									{
										lang = new Languages();
										lang.LangKey = "en";
										lang.LangText = "English";
										lang.IsActive = true;
										session.Insert(lang);
									}
									Models.TransLang transl = session.Get<Models.TransLang>(string.Format("{0}_{1}", trans.TransKey, lang.LangKey));
									if (transl == null)
									{
										transl = new TransLang();
										transl.TransLangKey = string.Format("{0}_{1}", trans.TransKey.Trim(), lang.LangKey.Trim());
										transl.Languages = new Languages() { LangKey = lang.LangKey, LangText = lang.LangText, IsActive = lang.IsActive };
										transl.Translations = new Translations.Models.Translations() { TransKey = trans.TransKey };
										transl.TransDescription = sArray.Attributes["name"].Value;
										transl.IsActive = true;
										session.Insert(transl);
									}
									else
									{
										transl.TransDescription = sArray.Attributes["name"].Value;
										session.Update(transl);
									}


									if (sArray.ChildNodes != null && sArray.ChildNodes.Count > 0)
									{
										foreach (XmlNode itemS in sArray.ChildNodes)
										{
											TransArray _trans = session.CreateSQLQuery("exec GetTransArray :trans_langKey, :arrayDescription")
																.AddEntity(typeof(TransArray))
																.SetParameter("trans_langKey", transl.TransLangKey.Trim())
																.SetParameter("arrayDescription", itemS.InnerText.Trim()).UniqueResult<TransArray>();

											if (_trans == null)
											{
												_trans = new TransArray();
												_trans.TransLang = transl;
												_trans.ArrayDescription = itemS.InnerText.Trim();
												_trans.IsActive = true;
												session.Insert(_trans);
											}
											else
											{
												_trans.ArrayDescription = itemS.InnerText.Trim();
												session.Update(_trans);
											}
										}
									}
								}
							}
						}
						#endregion

						transaction.Commit();
					}
				}
			}
		}


	}
}
