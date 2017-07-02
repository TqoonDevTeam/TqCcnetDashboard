using System.Web.Optimization;

namespace TqCcnetDashboard
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new StyleBundle("~/bundles/commoncss").Include(
                "~/Content/ui-grid.min.css",
                "~/Content/font-awesome.min.css",
                "~/Content/bootstrap.min.css",
                "~/Content/ui-bootstrap-csp.css",
                "~/Content/sb-admin/sb-admin.css",
                "~/Content/common/*.css"
                ));
        }
    }
}