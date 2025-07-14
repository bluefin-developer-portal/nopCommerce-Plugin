using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Components;

using Nop.Plugin.Payments.Bluefin.Models;
using Nop.Plugin.Payments.Bluefin.Domain;
using Nop.Services.Configuration;
using Nop.Core;

namespace Nop.Plugin.Payments.Bluefin.Components;

public class PaymentBluefinViewComponent : NopViewComponent
{
    private readonly ISettingService _settingService;
    private readonly IStoreContext _storeContext;

    public PaymentBluefinViewComponent(ISettingService settingService, IStoreContext storeContext)
    {
        _settingService = settingService;
        _storeContext = storeContext;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var bluefinPaymentSettings = await _settingService.LoadSettingAsync<BluefinPaymentSettings>(storeScope);
        
        var model = new PaymentInfoModel
        {
            IframeResponsive = bluefinPaymentSettings.IframeResponsive,
            IframeWidth = bluefinPaymentSettings.IframeWidth,
            IframeHeight = bluefinPaymentSettings.IframeHeight
        };
        
        return View("~/Plugins/Payments.Bluefin/Views/PaymentInfo.cshtml", model);
    }
}