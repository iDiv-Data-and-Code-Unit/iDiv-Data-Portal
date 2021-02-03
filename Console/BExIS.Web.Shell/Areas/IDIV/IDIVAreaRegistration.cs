using System.Web.Mvc;
using Vaiona.Utils.Cfg;

namespace BExIS.Web.Shell.Areas.IDIV
{
    public class IDIVAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "IDIV";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "IDIV_default",
                "IDIV/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
