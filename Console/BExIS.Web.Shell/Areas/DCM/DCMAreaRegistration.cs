﻿using System.Web.Mvc;

namespace BExIS.Web.Shell.Areas.DCM
{
    public class DCMAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "DCM";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "DCM_default",
                "DCM/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}