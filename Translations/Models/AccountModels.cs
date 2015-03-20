using NHibernate;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Globalization;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Translations.Helpers;

namespace Translations.Models
{
	public class User : Users
	{
		[Required]
		[Display(Name = "User name")]
		public string Username { get; set; }

		[Required]
		[DataType(DataType.Password)]
		[Display(Name = "Password")]
		public string Password { get; set; }

		[NotMapped]
		[Compare("Password")]
		public string ConfirmPassword { get; set; }

		[Display(Name = "Remember on this computer")]
		public bool RememberMe { get; set; }
	}


	public class TranslationsMembershipProvider : MembershipProvider
	{
		public override string ApplicationName
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		public override bool ValidateUser(string username, string password)
		{

			using (ISession session = NHibernateSession.OpenSession())
			{
				User user = GetUser(username);
				return user != null && Hash.ValidateHash(password, user.Password);
			}
		}

		public override bool ChangePassword(string username, string oldPassword, string newPassword)
		{
			throw new NotImplementedException();
		}

		public override bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion, string newPasswordAnswer)
		{
			throw new NotImplementedException();
		}

		public override MembershipUser CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status)
		{
			throw new NotImplementedException();
		}

		public override bool DeleteUser(string username, bool deleteAllRelatedData)
		{
			throw new NotImplementedException();
		}

		public override bool EnablePasswordReset
		{
			get { throw new NotImplementedException(); }
		}

		public override bool EnablePasswordRetrieval
		{
			get { throw new NotImplementedException(); }
		}

		public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
		{
			throw new NotImplementedException();
		}

		public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
		{
			throw new NotImplementedException();
		}

		public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
		{
			throw new NotImplementedException();
		}

		public override int GetNumberOfUsersOnline()
		{
			throw new NotImplementedException();
		}

		public override string GetPassword(string username, string answer)
		{
			throw new NotImplementedException();
		}

		public override MembershipUser GetUser(string username, bool userIsOnline)
		{
			throw new NotImplementedException();
		}

		public User GetUser(string username)
		{
			using (ISession session = NHibernateSession.OpenSession())
			{
				Users _user = session.CreateSQLQuery("exec GetUser :name")
					.AddEntity(typeof(Users))
					.SetParameter("name", username).UniqueResult<Users>();

				return new User
				{
					Name = _user.Name,
					Password = _user.Password,
					Surname = _user.Surname
				};
			}
		}

		public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
		{
			throw new NotImplementedException();
		}

		public override string GetUserNameByEmail(string email)
		{
			throw new NotImplementedException();
		}

		public override int MaxInvalidPasswordAttempts
		{
			get { throw new NotImplementedException(); }
		}

		public override int MinRequiredNonAlphanumericCharacters
		{
			get { throw new NotImplementedException(); }
		}

		public override int MinRequiredPasswordLength
		{
			get { throw new NotImplementedException(); }
		}

		public override int PasswordAttemptWindow
		{
			get { throw new NotImplementedException(); }
		}

		public override MembershipPasswordFormat PasswordFormat
		{
			get { throw new NotImplementedException(); }
		}

		public override string PasswordStrengthRegularExpression
		{
			get { throw new NotImplementedException(); }
		}

		public override bool RequiresQuestionAndAnswer
		{
			get { throw new NotImplementedException(); }
		}

		public override bool RequiresUniqueEmail
		{
			get { throw new NotImplementedException(); }
		}

		public override string ResetPassword(string username, string answer)
		{
			throw new NotImplementedException();
		}

		public override bool UnlockUser(string userName)
		{
			throw new NotImplementedException();
		}

		public override void UpdateUser(MembershipUser user)
		{
			throw new NotImplementedException();
		}
	}

	#region Services

	public interface IMembershipService
	{
		MembershipProvider Provider { get; }
		bool ValidateUser(string userName, string password);
		bool ChangePassword(string userName, string oldPassword, string newPassword);
	}

	public class AccountMembershipService : IMembershipService
	{
		private readonly TranslationsMembershipProvider _provider;

		public AccountMembershipService()
			: this(null)
		{
		}

		public AccountMembershipService(TranslationsMembershipProvider provider)
		{
			_provider = provider ?? new TranslationsMembershipProvider();
		}

		public MembershipProvider Provider
		{
			get { return _provider; }
		}

		public bool ValidateUser(string userName, string password)
		{
			if (string.IsNullOrEmpty(userName)) throw new ArgumentException("Value cannot be null or empty.", "userName");
			if (string.IsNullOrEmpty(password)) throw new ArgumentException("Value cannot be null or empty.", "password");

			return _provider.ValidateUser(userName, password);
		}

		public bool ChangePassword(string userName, string oldPassword, string newPassword)
		{
			if (string.IsNullOrEmpty(userName)) throw new ArgumentException("Value cannot be null or empty.", "userName");
			if (string.IsNullOrEmpty(oldPassword)) throw new ArgumentException("Value cannot be null or empty.", "oldPassword");
			if (string.IsNullOrEmpty(newPassword)) throw new ArgumentException("Value cannot be null or empty.", "newPassword");

			return _provider.ChangePassword(userName, oldPassword, newPassword);
		}

	}

	public interface IFormsAuthenticationService
	{
		void SignIn(string userName, bool createPersistentCookie);
		void SignOut();
	}

	public class FormsAuthenticationService : IFormsAuthenticationService
	{
		public void SignIn(string userName, bool createPersistentCookie)
		{
			if (string.IsNullOrEmpty(userName)) throw new ArgumentException("Value cannot be null or empty.", "userName");

			if (HttpContext.Current == null)
				FormsAuthentication.SetAuthCookie(userName, createPersistentCookie);
			else
			{
				var cookie = FormsAuthentication.GetAuthCookie(userName, createPersistentCookie);
				cookie.HttpOnly = true;
				cookie.Path = HttpContext.Current.Request.ApplicationPath;
				cookie.Secure = false;
				// the browser will ignore the cookie if there are fewer than two dots
				// see cookie spec - http://curl.haxx.se/rfc/cookie_spec.html
				if (HttpContext.Current.Request.Url.Host.Split('.').Length > 2)
				{
					// by default the domain will be the host, so www.site.com will get site.com
					// this may be a problem if we have clientA.site.com and clientB.site.com
					// the following line will force the full domain name
					cookie.Domain = HttpContext.Current.Request.Url.Host;
				}
				HttpContext.Current.Response.Cookies.Add(cookie);
			}
		}

		public void SignOut()
		{
			FormsAuthentication.SignOut();

			HttpCookie authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, "");
			HttpCookie appCookie = new HttpCookie("Translations", "");

			authCookie.HttpOnly = true;
			authCookie.Path = HttpContext.Current.Request.ApplicationPath;
			authCookie.Secure = false;

			// the browser will ignore the cookie if there are fewer than two dots
			// see cookie spec - http://curl.haxx.se/rfc/cookie_spec.html
			if (HttpContext.Current.Request.Url.Host.Split('.').Length > 2)
			{
				// by default the domain will be the host, so www.site.com will get site.com
				// this may be a problem if we have clientA.site.com and clientB.site.com
				// the following line will force the full domain name
				authCookie.Domain = HttpContext.Current.Request.Url.Host;
			}
			authCookie.Expires = DateTime.Now.AddDays(-1);
			appCookie.Expires = DateTime.Now.AddDays(-1);

			HttpContext.Current.Response.Cookies.Add(authCookie);
			HttpContext.Current.Response.Cookies.Add(appCookie);
		}
	}

	#endregion

}
