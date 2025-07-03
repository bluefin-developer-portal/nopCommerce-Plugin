using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Components;

using Nop.Plugin.Payments.Bluefin.Models;


namespace Nop.Plugin.Payments.Bluefin.Components;

public class PaymentBluefinViewComponent : NopViewComponent
{
    public IViewComponentResult Invoke()
    {
        var model = new PaymentInfoModel();
        return View("~/Plugins/Payments.Bluefin/Views/PaymentInfo.cshtml", model);
    }
}