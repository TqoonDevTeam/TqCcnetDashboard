using NUnit.Framework;
using TqLib.Utils;

namespace TqLib.zTest
{
    public class PathTest : AbstractTest
    {
        [Test]
        public void Combine()
        {
            string workingDirectory, path;

            workingDirectory = @"D:\TEST\1";
            path = PathUtil.Combine(workingDirectory, "first");
            Assert.AreEqual(@"D:\TEST\1\first", path);

            workingDirectory = @"D:\TEST\1\";
            path = PathUtil.Combine(workingDirectory, "first");
            Assert.AreEqual(@"D:\TEST\1\first", path);

            workingDirectory = @"D:\TEST\1\";
            path = PathUtil.Combine(workingDirectory, @"\first");
            Assert.AreEqual(@"D:\TEST\1\first", path);
        }
    }
}