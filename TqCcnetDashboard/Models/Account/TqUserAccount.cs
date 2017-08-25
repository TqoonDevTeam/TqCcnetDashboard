using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using System.Collections.Generic;
using System.Security.Claims;
using System.Web;

namespace TqCcnetDashboard
{
    public class TqUserAccount
    {
        public static IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.Current.GetOwinContext().Authentication;
            }
        }

        public static void SignIn(myAccount account)
        {
            SignOut();
            var claims = new List<Claim>();
            claims.Add(new Claim(ClaimTypes.Name, account.Id));
            claims.Add(new Claim(ClaimTypes.GivenName, account.Id));

            var identities = new TqUserIdentity(claims, DefaultAuthenticationTypes.ApplicationCookie);
            AuthenticationManager.SignIn(new AuthenticationProperties() { IsPersistent = false }, identities);
            HttpContext.Current.User = new TqUserPrincipal(AuthenticationManager.AuthenticationResponseGrant.Principal);
        }

        public static void SignOut()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
        }
    }
}