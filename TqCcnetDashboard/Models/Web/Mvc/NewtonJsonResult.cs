using Newtonsoft.Json;
using System;
using System.Web;
using System.Web.Mvc;

namespace TqCcnetDashboard.Web.Mvc
{
    public class NewtonJsonResult : JsonResult
    {
        private static JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };

        public override void ExecuteResult(ControllerContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            if (this.JsonRequestBehavior == JsonRequestBehavior.DenyGet && string.Equals(context.HttpContext.Request.HttpMethod, "GET", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("JsonRequest_GetNotAllowed");
            }
            HttpResponseBase response = context.HttpContext.Response;
            if (string.IsNullOrEmpty(this.ContentType))
            {
                response.ContentType = "application/json";
            }
            else
            {
                response.ContentType = this.ContentType;
            }
            if (this.ContentEncoding != null)
            {
                response.ContentEncoding = this.ContentEncoding;
            }
            if (this.Data != null)
            {
                var text = JsonConvert.SerializeObject(this.Data, Formatting.None, jsonSerializerSettings);
                response.Write(text.Replace("\\\"", ""));
            }
        }
    }
}