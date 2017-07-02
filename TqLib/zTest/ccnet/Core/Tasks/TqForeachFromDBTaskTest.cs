using Exortech.NetReflector;
using NUnit.Framework;
using ThoughtWorks.CruiseControl.Core;
using TqLib.ccnet.Core.Tasks;

namespace TqLib.zTest.ccnet.Core.Tasks
{
    [TestFixture(Category = "ccnet.core.Tasks")]
    public class TqForeachFromDBTaskTest
    {
        [Test]
        public void ConfigTest()
        {
            var obj = NetReflector.Read(xml) as TqForeachFromDBTask;
        }

        [Test]
        [Ignore("로컬전용")]
        public void RunTest()
        {
            var result = new IntegrationResult()
            {
                ProjectName = "TestProject",
                WorkingDirectory = @"D:\TEST",
                ArtifactDirectory = @"D:\TEST"
            };

            var obj = NetReflector.Read(xml) as TqForeachFromDBTask;
            obj.Run(result);
            var xxml = NetReflector.Write(obj);
        }

        private string xml = @"
<TqForeachFromDB>
    <provider>System.Data.SqlClient</provider>
    <connectionString>SERVER=192.168.1.45 ;DATABASE=adprintNewDB;USER ID=web; PASSWORD=xlzns!@#0</connectionString>
    <query>SELECT TOP 3 * FROM Joiner ORDER BY 1 DESC</query>
    <tasks>
        <NothingTest>
            <id>1234567890 @[db:siteUrl] @[db:id]</id>
            <bindings>*:80:local.@[db:siteUrl]
*:80:local2.@[db:siteUrl]
</bindings>
        </NothingTest>
    </tasks>
</TqForeachFromDB>
";
    }

    //[ReflectorType("directValue2")]
    //public class DirectDynamicValue2 : IDynamicValue
    //{
    //    private string propertyName;

    //    private string parameterName;

    //    private string defaultValue;

    //    [ReflectorProperty("default", Required = false)]
    //    public string DefaultValue
    //    {
    //        get
    //        {
    //            return this.defaultValue;
    //        }
    //        set
    //        {
    //            this.defaultValue = value;
    //        }
    //    }

    //    [ReflectorProperty("parameter")]
    //    public string ParameterName
    //    {
    //        get
    //        {
    //            return this.parameterName;
    //        }
    //        set
    //        {
    //            this.parameterName = value;
    //        }
    //    }

    //    [ReflectorProperty("property")]
    //    public string PropertyName
    //    {
    //        get
    //        {
    //            return this.propertyName;
    //        }
    //        set
    //        {
    //            this.propertyName = value;
    //        }
    //    }

    //    public DirectDynamicValue2()
    //    {
    //    }

    //    public DirectDynamicValue2(string parameter, string property)
    //    {
    //        this.propertyName = property;
    //        this.parameterName = parameter;
    //    }

    //    public virtual void ApplyTo(object value, Dictionary<string, string> parameters, IEnumerable<ParameterBase> parameterDefinitions)
    //    {
    //        try
    //        {
    //            DynamicValueUtility.PropertyValue propertyValue = DynamicValueUtility.FindProperty(value, this.propertyName);
    //            if (propertyValue != null)
    //            {
    //                string item = this.defaultValue;
    //                if (parameters.ContainsKey(this.parameterName))
    //                {
    //                    item = parameters[this.parameterName];
    //                }
    //                propertyValue.ChangeProperty(DynamicValueUtility.ConvertValue(this.parameterName, item, parameterDefinitions));
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //            throw ex;
    //        }
    //    }
    //}
}