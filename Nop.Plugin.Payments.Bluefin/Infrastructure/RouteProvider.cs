using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc.Routing;
using Nop.Web.Infrastructure;


namespace Nop.Plugin.Payments.Bluefin.Infrastructure
{
    public class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {
            /*
            endpointRouteBuilder.MapControllerRoute(
                name: "Plugin.Payments.Bluefin.ViewOrder",
                pattern: "/Admin/PaymentBluefin/ViewOrder/{order_temp_id}/{id}",
                defaults: new { controller = "PaymentBluefin", action = "ViewOrder", area = AreaNames.ADMIN }
            );
            */
        }

        public int Priority => 0; // route order priority
    }
}