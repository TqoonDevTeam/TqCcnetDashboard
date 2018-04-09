using Exortech.NetReflector;
using Microsoft.Web.Administration;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Tasks;
using TqLib.ccnet.Core.Util;
using TqLib.Utils;

namespace TqLib.ccnet.Core.Tasks
{
    [ReflectorType("TqIIS", Description = "IIS 설정")]
    public class TqIISTask : TaskBase
    {
        [ReflectorProperty("siteName", Description = "사이트명")]
        public string SiteName { get; set; }

        [ReflectorProperty("physicalPath", Description = "사이트경로")]
        public string PhysicalPath { get; set; }

        [DataType(DataType.MultilineText)]
        [ReflectorProperty("siteConfig", typeof(SiteConfigSerializerFactory), Description = "Site 설정", Required = false)]
        public Dictionary<string, string> SiteConfig { get; set; }

        [DataType(DataType.MultilineText)]
        [ReflectorProperty("virtualDirectories", typeof(VirtualDirectorySerializerFactory), Description = "가상경로", Required = false)]
        public Dictionary<string, string> VirtualDirectories { get; set; }

        [DataType(DataType.MultilineText)]
        [ReflectorProperty("bindings", typeof(BindingsSerializerFactory), Description = "바인딩 설정")]
        public TqBinding[] Bindings { get; set; }

        [ReflectorProperty("poolName", Description = "ApplicationPool 명")]
        public string PoolName { get; set; }

        [DataType(DataType.MultilineText)]
        [ReflectorProperty("poolConfig", typeof(ApplicationPoolConfigSerializerFactory), Description = "ApplicationPool 설정")]
        public Dictionary<string, string> PoolConfig { get; set; }

        protected override bool Execute(IIntegrationResult result)
        {
            result.BuildProgressInformation.SignalStartRunTask($"Executing TqIIS {SiteName}");
            Set(result);
            return true;
        }

        internal void Set(IIntegrationResult result)
        {
            string physicalPath = GetPhysicalPath(result);
            result.AddTaskResult("SiteName:" + SiteName);
            using (var iis = new ServerManager())
            {
                Site site = iis.Sites[SiteName];
                if (site == null)
                {
                    site = iis.Sites.Add(SiteName, physicalPath, 0);
                    site.Bindings.Clear();
                }
                else
                {
                    return;
                }

                SetSiteOption(site);
                site.Name = SiteName;
                site.ApplicationDefaults.ApplicationPoolName = PoolName;

                ApplicationPool pool = iis.ApplicationPools[PoolName];
                if (pool == null)
                {
                    pool = iis.ApplicationPools.Add(PoolName);
                }
                SetApplicationPoolOption(pool);
                pool.Name = PoolName;

                // set binginds
                site.Bindings.Clear();
                foreach (var binding in Bindings)
                {
                    if (binding.HasSSL)
                    {
                        site.Bindings.Add(binding.GetBindingInfomation(), binding.GetCertificateHash(), binding.GetCertificateStoreName());
                    }
                    else
                    {
                        site.Bindings.Add(binding.GetBindingInfomation(), binding.GetBindingProtocol());
                    }
                }

                // set virtualPath
                var defaultPath = site.Applications[0].VirtualDirectories["/"];
                site.Applications[0].VirtualDirectories.Clear();
                site.Applications[0].VirtualDirectories.Add(defaultPath);
                foreach (var vd in VirtualDirectories)
                {
                    site.Applications[0].VirtualDirectories.Add(vd.Key, vd.Value);
                }

                iis.CommitChanges();
            }
        }

        private void SetSiteOption(Site site)
        {
            foreach (var kv in SiteConfig)
            {
                SetValueBySearchProperty(site, kv.Value, kv.Key.Split('.'));
            }
        }

        private void SetApplicationPoolOption(ApplicationPool pool)
        {
            foreach (var kv in PoolConfig)
            {
                SetValueBySearchProperty(pool, kv.Value, kv.Key.Split('.'));
            }
        }

        private void SetValueBySearchProperty(object obj, object value, string[] propertySearch)
        {
            // indexer 는 지원안함
            if (propertySearch.Length > 1)
            {
                var newPropertySearch = propertySearch.Skip(1).ToArray();
                var prop = obj.GetType().GetProperty(propertySearch[0]);
                var newObj = prop.GetValue(obj);
                SetValueBySearchProperty(newObj, value, newPropertySearch);
            }
            else
            {
                var prop = obj.GetType().GetProperty(propertySearch[0]);
                if (prop.CanWrite)
                {
                    object validValue;
                    if (prop.PropertyType.IsEnum)
                    {
                        validValue = Enum.Parse(prop.PropertyType, value.ToString());
                    }
                    else
                    {
                        validValue = Convert.ChangeType(value, prop.PropertyType);
                    }
                    prop.SetValue(obj, validValue);
                }
            }
        }

        private string GetPhysicalPath(IIntegrationResult result)
        {
            var workingDirectory = result.WorkingDirectory;
            return PathUtil.Combine(workingDirectory, PhysicalPath);
        }
    }
}