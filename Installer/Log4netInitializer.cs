using log4net.Config;
using System;
using System.IO;

namespace Installer
{
    public class Log4netInitializer
    {
        public void Initialize()
        {
            using (MemoryStream ms = new MemoryStream())
            using (StreamWriter sw = new StreamWriter(ms))
            {
                sw.Write(GetConfig());
                sw.Flush();
                ms.Position = 0;
                XmlConfigurator.Configure(ms);
            }
        }

        private string GetConfig()
        {
            var logPath = Path.Combine(Environment.CurrentDirectory, "%date{yyyyMMdd}.log");
            return $@"<?xml version=""1.0"" encoding=""utf-8"" ?>
<log4net>
    <appender name=""defaultLogAppender"" type=""TqLib.Dashboard.CustomEventAppender"">
        <encoding value=""utf-8""/>
        <layout type=""log4net.Layout.PatternLayout"">
            <conversionPattern value=""%-5level %message"" />
        </layout>
    </appender>
    <root>
        <level value=""ALL""/>
        <appender-ref ref=""defaultLogAppender""/>
    </root>
</log4net>
";
        }
    }
}