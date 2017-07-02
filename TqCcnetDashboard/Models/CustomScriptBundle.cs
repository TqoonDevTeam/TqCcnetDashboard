using System.Collections.Generic;
using System.Web.Optimization;

namespace TqCcnetDashboard
{
    public class UniqueScriptBundle : ScriptBundle
    {
        private IList<string> ItemList = new List<string>();

        public UniqueScriptBundle(string virtualPath) : base(virtualPath)
        {
        }

        public override Bundle Include(params string[] virtualPaths)
        {
            foreach (var virtualPath in virtualPaths)
            {
                if (!ItemList.Contains(virtualPath))
                {
                    this.ItemList.Add(virtualPath);
                    base.Include(virtualPath);
                }
            }
            return this;
        }

        public override Bundle Include(string virtualPath, params IItemTransform[] transforms)
        {
            if (!ItemList.Contains(virtualPath))
            {
                this.ItemList.Add(virtualPath);
                base.Include(virtualPath, transforms);
            }
            return this;
        }
    }
}