using System.IO;
using System.Net.Mime;
using System.Text;
using System.Web.Mvc;

namespace TqCcnetDashboard.Web.Mvc
{
    public class ViewDownloadFilterAttribute : ActionFilterAttribute
    {
        public string Query { get; set; }

        public ViewDownloadFilterAttribute(string query = "toFile")
        {
            Query = query;
        }

        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            if (HasKey(filterContext))
            {
                // .aspx, .ascx = 미지원

                // .cshtml
                using (var sw = new StringWriter())
                {
                    var viewResult = filterContext.Result as ViewResult;
                    var viewContext = new ViewContext(filterContext, viewResult.View, viewResult.ViewData, viewResult.TempData, sw);
                    viewResult.View.Render(viewContext, sw);
                    foreach (var engine in viewResult.ViewEngineCollection)
                    {
                        engine.ReleaseView(filterContext, viewResult.View);
                    }
                    var html = sw.GetStringBuilder().ToString();
                    var bytes = Encoding.UTF8.GetBytes(html);

                    filterContext.HttpContext.Response.ClearContent();
                    filterContext.HttpContext.Response.BinaryWrite(bytes);
                    filterContext.HttpContext.Response.ContentType = MediaTypeNames.Application.Octet;
                    filterContext.HttpContext.Response.AddHeader("Content-Disposition", "attachment; filename=" + "wizard.html");
                }
            }
            else
            {
                base.OnResultExecuted(filterContext);
            }
        }

        private bool HasKey(ControllerContext filterContext)
        {
            return filterContext.HttpContext.Request.QueryString[Query] != null;
        }
    }
}