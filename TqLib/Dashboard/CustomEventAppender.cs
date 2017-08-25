using log4net.Appender;
using log4net.Core;
using System;

namespace TqLib.Dashboard
{
    public class CustomEventAppender : AppenderSkeleton
    {
        public event EventHandler<CustomEventAppenderArgs> ProcessChanged;

        protected override void Append(LoggingEvent loggingEvent)
        {
            var msg = base.RenderLoggingEvent(loggingEvent);
            OnProcessChanged(new CustomEventAppenderArgs
            {
                Level = loggingEvent.Level.DisplayName,
                Msg = msg
            });
        }

        private void OnProcessChanged(CustomEventAppenderArgs e)
        {
            ProcessChanged?.Invoke(this, e);
        }
    }

    public class CustomEventAppenderArgs
    {
        public string Level { get; set; } = string.Empty;
        public string Msg { get; set; } = string.Empty;
    }
}