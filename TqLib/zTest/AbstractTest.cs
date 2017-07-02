using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using NUnit.Framework;
using System;
using System.IO;
using System.Reflection;
using System.Text;
using TqLib.Utils;

namespace TqLib.zTest
{
    public abstract class AbstractTest
    {
        protected string BinPath { get; private set; }

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            SetBinPath();
            //SetUpLog4net();
        }

        private void SetUpLog4net()
        {
            var hierarchy = (Hierarchy)LogManager.GetRepository();

            RollingFileAppender rootLogAppender = new RollingFileAppender
            {
                Encoding = Encoding.UTF8,
                File = PathUtil.Combine(BinPath, @"logs\"),
                AppendToFile = true,
                RollingStyle = RollingFileAppender.RollingMode.Date,
                DatePattern = "yyyyMMdd'.log'",
                StaticLogFileName = false,
                Layout = new PatternLayout { ConversionPattern = "%date{yyyy-MM-dd hh:mm:ss} %-5level %message%newline" }
            };
            (rootLogAppender.Layout as PatternLayout).ActivateOptions();
            rootLogAppender.ActivateOptions();

            hierarchy.Root.AddAppender(rootLogAppender);
            hierarchy.Root.Level = Level.All;
            hierarchy.Configured = true;
        }

        private void SetBinPath()
        {
            string codeBase = Assembly.GetExecutingAssembly().CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);
            string path = Uri.UnescapeDataString(uri.Path);
            BinPath = Path.GetDirectoryName(path);
        }
    }

    [TestFixture]
    public class AbstractTestTest { }
}