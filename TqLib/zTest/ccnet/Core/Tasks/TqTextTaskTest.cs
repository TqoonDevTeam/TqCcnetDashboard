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
        //[Ignore("로컬전용")]
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
            task.SaveEncoding = "SHIFT-JIS";
            task.Run(result);

            var xml = NetReflector.Write(task);
        }

        private string source = @"###クライアント証明書ファイルパス（他の証明書をインストールしている場合重複しないこと）###
paygentB2Bmodule.client_file_path=05138a0f6d2062d9";
    }
}