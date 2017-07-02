using System.Linq;
using System.Web.Mvc;

namespace TqCcnetDashboard.Web.Mvc
{
    public class AllowCrossDomainFilterAttribute : ActionFilterAttribute
    {
        public string[] Rules { get; set; }

        public AllowCrossDomainFilterAttribute(params string[] rules)
        {
            Rules = rules;
            if (Rules == null || Rules.Length == 0)
            {
                Rules = new[] { "*" };
            }
        }

        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            if (Rules.Contains("*"))
            {
                filterContext.RequestContext.HttpContext.Response.Headers["Access-Control-Allow-Origin"] = "*";
            }
            else if (Rules.Contains(filterContext.RequestContext.HttpContext.Request.UrlReferrer.DnsSafeHost))
            {
                filterContext.RequestContext.HttpContext.Response.Headers["Access-Control-Allow-Origin"] = $"{filterContext.HttpContext.Request.UrlReferrer.Scheme}://{filterContext.RequestContext.HttpContext.Request.UrlReferrer.DnsSafeHost}";
            }
            base.OnResultExecuted(filterContext);
        }
    }
}