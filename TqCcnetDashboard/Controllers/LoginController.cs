using System.Web.Mvc;
using TqCcnetDashboard.Config;
using TqLib.Exceptions;

namespace TqCcnetDashboard.Controllers
{
    public class LoginController : AbstractController
    {
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult SignIn(string id, string pw, string ReturnUrl = "/")
        {
            var account = GetAccount();
            if (account.Id == id && account.Pw == pw)
            {
                TqUserAccount.SignIn(account);
                return Json(ReturnUrl);
            }
            else
            {
                throw new WebAlertException("로그인 실패");
            }
        }

        private myAccount GetAccount()
        {
            var account = ConfigManager.Get("account", "admin:admin");
            var split = account.Split(new[] { ':' }, 2);
            return new myAccount
            {
                Id = split[0],
                Pw = split[1]
            };
        }
    }
}