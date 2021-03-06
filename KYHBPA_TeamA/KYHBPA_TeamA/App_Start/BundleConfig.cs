﻿using System.Web;
using System.Web.Optimization;

namespace KYHBPA_TeamA
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js").Include(
                        "~/Scripts/EventCard.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            bundles.Add(new ScriptBundle("~/bundles/BoardOfDirectorsReadMore").Include(
                        "~/Scripts/BoardOfDirectorsReadMore.js"));

            bundles.Add(new ScriptBundle("~/Content/fontawesome").Include(
                    "~/Content/fontawesome.js",
                    "~/Content/solid.js",
                    "~/Content/brands.min.js",
                    "~/Content/fontawesome-pro-brands/index.js",
                    "~/Content/regular.js",
                    "~/Content/light.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/summernote", "https://cdnjs.cloudflare.com/ajax/libs/summernote/0.8.9/summernote-lite.js").Include(
                    "~/Scripts/summernote.min.js",
                    "~/Scripts/script-custom-editor.js"
                ));

            bundles.Add(new StyleBundle("~/bundles/summernotestyle", "https://cdnjs.cloudflare.com/ajax/libs/summernote/0.8.9/summernote-lite.css").Include(
                    "~/Content/Summernote/summernote.css", new CssRewriteUrlTransform()
                ));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at https://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/respond.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/Site.css"
                      ));
        }
    }
}
