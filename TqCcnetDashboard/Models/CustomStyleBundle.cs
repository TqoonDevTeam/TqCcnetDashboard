using BundleTransformer.Core.Builders;
using BundleTransformer.Core.Orderers;
using BundleTransformer.Core.Transformers;
using System.Collections.Generic;
using System.Web.Optimization;

namespace TqCcnetDashboard
{
    public class UniqueCustomStyleBundle : Bundle
    {
        private IList<string> ItemList = new List<string>();
        private static NullBuilder nullBuilder = new NullBuilder();
        private static StyleTransformer styleTransformer = new StyleTransformer();
        private static NullOrderer nullOrderer = new NullOrderer();

        public UniqueCustomStyleBundle(string virtualPath) : base(virtualPath)
        {
            this.Builder = nullBuilder;
            this.Transforms.Add(styleTransformer);
            this.Transforms.Add(new CssMinify());
            this.Orderer = nullOrderer;
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