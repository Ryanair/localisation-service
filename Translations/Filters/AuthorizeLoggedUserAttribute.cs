using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Translations.Models;

namespace Translations.Filters
{
	public class AuthorizeLoggedUserAttribute : AuthorizeAttribute
	{
		// Custom property
		public string AccessLevel { get; set; }

		protected override bool AuthorizeCore(HttpContextBase httpContext)
		{
			User _user = httpContext.Session[System.Configuration.ConfigurationManager.AppSettings["userSessionKey"].ToString()] as User;
			return _user != null;
		}
	}
}