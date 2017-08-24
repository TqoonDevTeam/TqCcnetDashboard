using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Filters;
using System.Web.Mvc;
using TqLib.Exceptions;

namespace TqCcnetDashboard.Web.Mvc
{
    public class TqoonDevTeamExceptionApiFilterAttribute : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext context)
        {
            TqLogger.System.Error(context.Exception);

            if (context.Exception is ITqoonDevTeamException)
            {
                var ex = context.Exception as ITqoonDevTeamException;
                var error = new HttpError();
                error.Message = ex.Title;
                error.MessageDetail = ex.Message;
                var res = context.Request.CreateErrorResponse((HttpStatusCode)ex.ErrorCode, error);
                context.Response = res;
                return;
            }
            base.OnException(context);
        }
    }

    public class TqoonDevTeamExceptionFilterAttribute : HandleErrorAttribute
    {
        public override void OnException(ExceptionContext context)
        {
            TqLogger.System.Error(context.Exception);

            if (context.Exception is ITqoonDevTeamException)
            {
                var ex = context.Exception as ITqoonDevTeamException;
                var error = new HttpError();
                error.Message = ex.Title;
                error.MessageDetail = ex.Message;
                context.ExceptionHandled = true;
                context.HttpContext.Response.Clear();
                context.HttpContext.Response.TrySkipIisCustomErrors = true;
                context.HttpContext.Response.StatusCode = ex.ErrorCode;
                context.Result = new JsonResult() { Data = error };
                return;
            }
            else if (HasOctokitException(context.Exception))
            {
                var ex = context.Exception;
                var error = new HttpError(ex, true);
                context.ExceptionHandled = true;
                context.HttpContext.Response.Clear();
                context.HttpContext.Response.TrySkipIisCustomErrors = true;
                context.HttpContext.Response.StatusCode = 500;
                context.Result = new JsonResult() { Data = error };
                return;
            }
            base.OnException(context);
        }

        private bool HasOctokitException(Exception ex)
        {
            if (ex == null) return false;
            if (ex.GetType().FullName.StartsWith("Octokit.")) return true;
            if (ex.InnerException != null)
            {
                if (ex.InnerException.GetType().FullName.StartsWith("Octokit.")) return true;
            }
            return false;
        }
    }
}