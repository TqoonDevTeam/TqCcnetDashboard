using System;
using System.Collections.Generic;
using Exortech.NetReflector;
using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Tasks;
using TqLib.ccnet.Core.Util;

namespace TqLib.ccnet.Core.Tasks
{
    [ReflectorType("NothingTest")]
    public class NothingTestTask : TaskBase
    {
        [ReflectorProperty("id", Required = false)]
        public string Id { get; set; }
        [ReflectorProperty("bindings", typeof(BindingsSerializerFactory), Description = "바인딩 설정")]
        public TqBinding[] Bindings { get; set; }
        protected override bool Execute(IIntegrationResult result)
        {
            return true;
        }
    }
}