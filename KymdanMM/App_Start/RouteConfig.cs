using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace KymdanMM
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "AddMaterialProposal",
                url: "them-de-xuat",
                defaults: new { controller = "Home", action = "AddOrUpdateMaterialProposal" }
            );

            routes.MapRoute(
                name: "UpdateMaterialProposal",
                url: "chinh-sua-de-xuat/{id}",
                defaults: new { controller = "Home", action = "AddOrUpdateMaterialProposal", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}