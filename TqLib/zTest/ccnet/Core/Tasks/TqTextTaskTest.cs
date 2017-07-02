using Exortech.NetReflector;
using NUnit.Framework;
using ThoughtWorks.CruiseControl.Core;
using TqLib.ccnet.Core.Tasks;

namespace TqLib.zTest.ccnet.Core.Tasks
{
    [TestFixture(Category = "ccnet.core.Tasks")]
    public class TqTextTaskTest
    {
        [Test]
        [Ignore("로컬전용")]
        public void Test()
        {
            var result = new IntegrationResult()
            {
                ProjectName = "TestProject",
                WorkingDirectory = @"D:\TEST",
                ArtifactDirectory = @"D:\TEST"
            };

            var task = new TqTextTask();
            task.Source = source;
            task.SavePath = "TqTestTaskTest.xml";
            task.FileSaveType = TqTextTask.SaveType.Xml;
            task.Run(result);

            var xml = NetReflector.Write(task);
        }

        private string source = @"<?xml version=""1.0""?>

<configuration>
  <configSections>
    <sectionGroup name=""system.web.webPages.razor"" type=""System.Web.WebPages.Razor.Configuration.RazorWebSectionGroup, System.Web.WebPages.Razor, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31BF3856AD364E35"">
      <section name=""host"" type=""System.Web.WebPages.Razor.Configuration.HostSection, System.Web.WebPages.Razor, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31BF3856AD364E35"" requirePermission=""false"" />
      <section name=""pages"" type=""System.Web.WebPages.Razor.Configuration.RazorPagesSection, System.Web.WebPages.Razor, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31BF3856AD364E35"" requirePermission=""false"" />
    </sectionGroup>
  </configSections>

  <system.web.webPages.razor>
    <host factoryType=""System.Web.Mvc.MvcWebRazorHostFactory, System.Web.Mvc, Version=5.2.3.0, Culture=neutral, PublicKeyToken=31BF3856AD364E35"" />
    <pages pageBaseType=""System.Web.Mvc.WebViewPage"">
      <namespaces>
        <add namespace=""System.Web.Mvc"" />
        <add namespace=""System.Web.Mvc.Ajax"" />
        <add namespace=""System.Web.Mvc.Html"" />
        <add namespace=""System.Web.Optimization"" />
        <add namespace=""System.Web.Routing"" />
        <add namespace=""TqCcnetDashboard"" />
      </namespaces>
    </pages>
  </system.web.webPages.razor>

  <appSettings>
    <add key=""webpages:Enabled"" value=""false"" />
  </appSettings>

  <system.webServer>
    <handlers>
      <remove name=""BlockViewHandler"" />
    </handlers>
  </system.webServer>

  <system.web>
    <compilation>
      <assemblies>
        <add assembly=""System.Web.Mvc, Version=5.2.3.0, Culture=neutral, PublicKeyToken=31BF3856AD364E35"" />
      </assemblies>
    </compilation>
  </system.web>
</configuration>";
    }
}