using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using DotNetOpenAuth.AspNet;
using Microsoft.Web.WebPages.OAuth;
using WebMatrix.WebData;
using Translations.Filters;
using Translations.Models;
using System.Web.Routing;
using Translations.Helpers;
using NHibernate;

namespace Translations.Controllers
{
	public class AccountController : Controller
	{
		#region Initialize
		public Translations.Models.IFormsAuthenticationService FormsService { get; set; }
		public Translations.Models.IMembershipService MembershipService { get; set; }
		protected override void Initialize(RequestContext requestContext)
		{
			if (FormsService == null) { FormsService = new Translations.Models.FormsAuthenticationService(); }
			if (MembershipService == null) { MembershipService = new Translations.Models.AccountMembershipService(); }

			base.Initialize(requestContext);
		}

		#endregion

		// GET: /User/
		public ActionResult Index()
		{
			return View();
		}

		[HttpGet]
		public ActionResult Login()
		{
			return View();
		}

		[HttpPost]
		public ActionResult Login(Models.User user)
		{
			//remove Fields that just necessary to Register since we use same model to register/login
			ModelState.Remove("ConfirmPassword");
			ModelState.Remove("Name");
			ModelState.Remove("Surname");
			
			if (ModelState.IsValid)
			{
				TranslationsMembershipProvider userProvider = MembershipService.Provider as TranslationsMembershipProvider;
				User _user = userProvider.GetUser(user.Username);

				if (_user != null && Hash.ValidateHash(user.Password, _user.Password))
				{
					FormsAuthentication.SetAuthCookie(user.Username, user.RememberMe);
					Session[System.Configuration.ConfigurationManager.AppSettings["userSessionKey"].ToString()] = _user;
					return RedirectToAction("Index", "Home");
				}
				else
				{
					ModelState.AddModelError("", "Login data is incorrect!");
				}
			}
			return View(user);
		}
		
		public ActionResult Logout()
		{
			FormsService.SignOut();
			Session.Remove(System.Configuration.ConfigurationManager.AppSettings["userSessionKey"].ToString());
			return RedirectToAction("Index", "Home");
		}

		[HttpGet]
		public ActionResult Register()
		{
			return View();
		}

		[HttpPost]
		public ActionResult Register(Models.User user)
		{
			if (ModelState.IsValid)
			{
				using (ISession session = NHibernateSession.OpenSession())
				{
					using (ITransaction transaction = session.BeginTransaction())
					{
						Users _user = new Users();
						_user.Username = user.Username;
						_user.Password = Hash.CreateHash(user.Password);
						_user.Surname = user.Surname;
						_user.Name = user.Name;
						session.Save(_user);
						transaction.Commit();

						Session[System.Configuration.ConfigurationManager.AppSettings["userSessionKey"].ToString()] = _user;
					}
				}



				FormsAuthentication.SetAuthCookie(user.Username, user.RememberMe);
				TranslationsMembershipProvider userProvider = MembershipService.Provider as TranslationsMembershipProvider;
				//User _user = userProvider.GetUser(user.Username);

				//if (_user.IsAdmin)
				//{
				return RedirectToAction("Index", "Home");
				//}
			}
			else
			{
				ModelState.AddModelError("", "Login data is incorrect!");
			}

			return View(user);
		}

	}
}
