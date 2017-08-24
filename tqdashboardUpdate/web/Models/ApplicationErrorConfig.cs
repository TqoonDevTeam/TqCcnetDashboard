using System;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Web;

namespace TqCcnetDashboard
{
    public static class ApplicationErrorConfig
    {
        private static readonly log4net.ILog logger404 = log4net.LogManager.GetLogger("RollingLog404");
        private static readonly log4net.ILog logger500 = log4net.LogManager.GetLogger("RollingLogr500");

        public static void RegisterErrorHandler(HttpApplication app)
        {
            if (CheckClientScriptError(app)) return;

            try { LoggingException(app); } catch { }
        }

        /// <summary>
        /// 클라이언트에서 처리하는 예외를 체크한다.
        /// </summary>
        private static bool CheckClientScriptError(HttpApplication app)
        {
            return false;
        }

        private static void LoggingException(HttpApplication app)
        {
            var dbLoggerItem = new LoggerItem();
            dbLoggerItem.url = app.Request.Url.PathAndQuery;
            dbLoggerItem.userHostAddress = app.Request.UserHostAddress ?? string.Empty;
            dbLoggerItem.userAgent = app.Request.UserAgent ?? string.Empty;
            dbLoggerItem.errorMsg = GetExceptionString(app);
            dbLoggerItem.os = app.Request.Browser.Platform ?? string.Empty;
            dbLoggerItem.browser = app.Request.Browser.Type ?? string.Empty;
            dbLoggerItem.userLanguage = app.Request.UserLanguages != null || app.Request.UserLanguages.Length > 0 ? app.Request.UserLanguages[0].ToString() : string.Empty;
            dbLoggerItem.dns = app.Request.Url.DnsSafeHost;

            if (app.Context.Error is HttpException
                && (
                ((HttpException)app.Context.Error).GetHttpCode() == 404 ||
                ((HttpException)app.Context.Error).GetHttpCode() == 400
                ))
            {
                Logging404(dbLoggerItem);
            }
            else
            {
                Logging500(dbLoggerItem);
            }
        }

        private static void Logging500(LoggerItem item)
        {
            logger500.Error(item.ToString());
        }

        private static void Logging404(LoggerItem item)
        {
            logger404.Error(item.ToString());
        }

        private static string GetExceptionString(HttpApplication app)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("# Request Headers");
            sb.AppendLine(NameValueCollectionToString(app.Request.Headers));
            sb.AppendLine("# Request Parameters - ServerVariables");
            sb.AppendLine(NameValueCollectionToString(app.Request.ServerVariables));
            sb.AppendLine("# Request Parameters - QueryString");
            sb.AppendLine(NameValueCollectionToString(app.Request.QueryString));
            sb.AppendLine("# Request Parameters - Form");
            sb.AppendLine(NameValueCollectionToString(app.Request.Form));
            sb.AppendLine("# Request Parameters - Payload");
            sb.AppendLine(NameValueCollectionToString(app.Request.Form));
            sb.AppendLine("# Exceptions");
            sb.AppendLine(ExceptionToString(app.Context.Error));
            return sb.ToString();
        }

        private static string NameValueCollectionToString(NameValueCollection col)
        {
            StringBuilder sb = new StringBuilder();
            string keyVal;
            foreach (var key in col.AllKeys)
            {
                keyVal = col[key];
                sb.AppendLine($"{key}={(keyVal == null ? "null" : HttpUtility.UrlDecode(keyVal))}");
            }
            return sb.ToString();
        }

        private static string PayloadToString(HttpApplication app)
        {
            // IsPayload Request
            if (app.Request.ContentType.ToLower().Contains("json"))
            {
                app.Request.InputStream.Seek(0, SeekOrigin.Begin);
                using (var sr = new StreamReader(app.Request.InputStream, Encoding.UTF8, false, 1024, leaveOpen: true))
                {
                    string data = sr.ReadToEnd();
                    return data;
                }
            }
            else
            {
                return string.Empty;
            }
        }

        private static string ExceptionToString(Exception ex)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("----------------------------------------");
            sb.AppendLine("## Message ##");
            sb.AppendLine(ex.Message ?? string.Empty);
            sb.AppendLine("## StackTrace ##");
            sb.AppendLine(ex.StackTrace ?? string.Empty);
            sb.AppendLine("----------------------------------------");
            if (ex.InnerException != null) sb.AppendLine(ExceptionToString(ex.InnerException));
            return sb.ToString();
        }

        private class LoggerItem
        {
            public string errorMsg { get; set; }
            public string url { get; set; }
            public string os { get; set; }
            public string browser { get; set; }
            public string userHostAddress { get; set; }
            public string userAgent { get; set; }
            public string userLanguage { get; set; }
            public string dns { get; set; }

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                sb.Append($"{url} {userHostAddress} {os} {browser} {userAgent} {userLanguage} {dns}");
                sb.AppendLine("[MESSAGE]");
                sb.AppendLine(errorMsg);
                return sb.ToString();
            }
        }
    }
}