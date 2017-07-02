using System;

namespace TqLib.Exceptions
{
    public interface ITqoonDevTeamException
    {
        string Title { get; set; }
        string Message { get; }
        int ErrorCode { get; }
    }

    public class WebAlertException : Exception, ITqoonDevTeamException
    {
        public string Title { get; set; } = string.Empty;
        public int ErrorCode { get { return 555; } }

        public WebAlertException(string message, string title = null) : base(message)
        {
            if (title != null) Title = title;
        }
    }

    public class FormDataErrorException : Exception, ITqoonDevTeamException
    {
        public string Title { get; set; } = string.Empty;
        public int ErrorCode { get { return 556; } }

        public FormDataErrorException(string message, string title = null) : base(message)
        {
            if (title != null) Title = title;
        }
    }
}