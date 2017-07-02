using Spring.Context.Support;
using Spring.Core.IO;
using Spring.Data.Generic;
using Spring.Objects.Factory.Config;
using Spring.Objects.Factory.Support;
using Spring.Objects.Factory.Xml;
using System;

namespace TqLib.ccnet.Core.Util
{
    internal static class SpringContextHelper
    {
        internal const string _RootContext = "spring.tqLib.root";
        private static object lock_init = new object();
        private static IObjectDefinitionFactory fac = new DefaultObjectDefinitionFactory();

        static SpringContextHelper()
        {
            if (!ContextRegistry.IsContextRegistered(_RootContext))
            {
                lock (lock_init)
                {
                    if (!ContextRegistry.IsContextRegistered(_RootContext))
                    {
                        GenericApplicationContext ctx = new GenericApplicationContext();
                        ctx.Name = _RootContext;
                        XmlObjectDefinitionReader reader = new XmlObjectDefinitionReader(ctx);
                        reader.LoadObjectDefinitions(new StringResource("<?xml version=\"1.0\" encoding=\"utf-8\" ?><objects></objects>"));
                        ContextRegistry.RegisterContext(ctx);
                    }
                }
            }
        }

        private static AbstractApplicationContext GetAbstractApplicationContext()
        {
            return ContextRegistry.GetContext(_RootContext) as AbstractApplicationContext;
        }

        public static AdoTemplate GetAdoTemplate(string provider, string connectionString)
        {
            string conn = connectionString.Replace(" ", string.Empty);
            string providerId = $"genDbProvider_{conn}";
            string adoTemplateId = $"genAdoTemplate_{conn}";
            var ctx = GetAbstractApplicationContext();

            if (!ctx.IsObjectNameInUse(providerId))
            {
                lock (lock_init)
                {
                    if (!ctx.IsObjectNameInUse(providerId))
                    {
                        var def = fac.CreateObjectDefinition("Spring.Data.Common.DbProviderFactoryObject, Spring.Data", null, AppDomain.CurrentDomain);
                        def.PropertyValues.Add("Provider", provider);
                        def.PropertyValues.Add("ConnectionString", connectionString);
                        ctx.RegisterObjectDefinition(providerId, def);
                    }
                }
            }

            if (!ctx.IsObjectNameInUse(adoTemplateId))
            {
                lock (lock_init)
                {
                    if (!ctx.IsObjectNameInUse(adoTemplateId))
                    {
                        var def = fac.CreateObjectDefinition("Spring.Data.Generic.AdoTemplate, Spring.Data", null, AppDomain.CurrentDomain);
                        def.PropertyValues.Add("DbProvider", new RuntimeObjectReference(providerId));
                        def.PropertyValues.Add("DataReaderWrapperType", "Spring.Data.Support.NullMappingDataReader, Spring.Data");
                        def.PropertyValues.Add("CommandTimeout", 60);
                        ctx.RegisterObjectDefinition(adoTemplateId, def);
                    }
                }
            }

            return ctx.GetObject(adoTemplateId) as AdoTemplate;
        }
    }
}