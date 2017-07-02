using Microsoft.Owin.Security;
using System;
using System.Security.Claims;
using System.Web;

namespace TqCcnetDashboard
{
    public class CurrentUser
    {
        private static IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.Current.GetOwinContext().Authentication;
            }
        }

        public static bool IsLogin
        {
            get { return HttpContext.Current != null && HttpContext.Current.User.Identity.IsAuthenticated; }
        }

        public static int Id
        {
            get
            {
                if (IsLogin)
                {
                    return Convert.ToInt32(AuthenticationManager.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                }
                else
                {
                    return 0;
                }
            }
        }

        public static string UserId
        {
            get
            {
                if (IsLogin)
                {
                    return HttpContext.Current.User.Identity.Name;
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public static string UserName
        {
            get
            {
                if (IsLogin)
                {
                    return AuthenticationManager.User.FindFirst(ClaimTypes.GivenName)?.Value;
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public static int CurrentUserGroupId
        {
            get
            {
                if (IsLogin)
                {
                    return Convert.ToInt32(AuthenticationManager.User.FindFirst("dev.tqoon.com:CurrentUserGroupId")?.Value ?? "0");
                }
                else
                {
                    return 0;
                }
            }
            set
            {
                if (IsLogin)
                {
                    var Identity = new ClaimsIdentity(AuthenticationManager.User.Identity);
                    var claim = Identity.FindFirst("dev.tqoon.com:CurrentUserGroupId");
                    if (claim != null)
                    {
                        Identity.RemoveClaim(claim);
                    }
                    Identity.AddClaim(new Claim("dev.tqoon.com:CurrentUserGroupId", value.ToString()));
                    AuthenticationManager.AuthenticationResponseGrant = new AuthenticationResponseGrant(new ClaimsPrincipal(Identity), new AuthenticationProperties { IsPersistent = true });
                }
            }
        }

        public static string WorkingDirectory
        {
            get { return @"~/Repo/" + CurrentUserGroupId; }
        }

        public static string WorkingDirectoryMapPath
        {
            get
            {
                return HttpContext.Current.Server.MapPath(WorkingDirectory);
            }
        }
    }

    //public static string DefaultUserName
    //{
    //    get
    //    {
    //        if (HttpContext.Current != null && HttpContext.Current.User.Identity.IsAuthenticated)
    //        {
    //            return AuthenticationManager.User.FindFirst("dev.tqoon.com:DefaultUserName")?.Value;
    //        }
    //        else
    //        {
    //            return string.Empty;
    //        }
    //    }
    //}

    //public static string AvatarUrl
    //{
    //    get
    //    {
    //        if (HttpContext.Current != null && HttpContext.Current.User.Identity.IsAuthenticated)
    //        {
    //            return AuthenticationManager.User.FindFirst("dev.tqoon.com:AvatarUrl")?.Value;
    //        }
    //        else
    //        {
    //            return string.Empty;
    //        }
    //    }
    //}

    //public static string ConvertToJsonString()
    //{
    //    Dictionary<string, string> dic = new Dictionary<string, string>();
    //    var props = typeof(CurrentUser).GetProperties(BindingFlags.Static | BindingFlags.Public);
    //    foreach (var prop in props)
    //    {
    //        dic[prop.Name] = (prop.GetValue(null) ?? string.Empty).ToString();
    //    }
    //    return JsonConvert.SerializeObject(dic);
    //}
}