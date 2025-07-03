using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json;

using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Logging;

using Nop.Plugin.Payments.Bluefin;
using Nop.Plugin.Payments.Bluefin.Domain;
using Nop.Plugin.Payments.Bluefin.Models;
using Nop.Plugin.Payments.Bluefin.Services;

using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Payments;

using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;



public class PaymentBluefinController : BasePaymentController
{

    private readonly ILogger _logger;
    private readonly ISettingService _settingService;
    private readonly ILocalizationService _localizationService;
    private readonly INotificationService _notificationService;
    private readonly IOrderProcessingService _orderProcessingService;
    private readonly IGenericAttributeService _genericAttributeService;

    private readonly BluefinPaymentSettings _bluefinPaymentSettings;
    private readonly IWorkContext _workContext;
    private readonly IStoreContext _storeContext;

    private readonly BluefinGateway _gateway;

    private readonly BluefinTokenRepositoryService _bluefinTokenRepositoryService;

    public PaymentBluefinController(ILogger logger,
        ISettingService settingService,
        ILocalizationService localizationService,
        INotificationService notificationService,
        IOrderProcessingService orderProcessingService,
        IGenericAttributeService genericAttributeService,
        BluefinPaymentSettings bluefinPaymentSettings,
        BluefinTokenRepositoryService bluefinTokenRepositoryService,
        IWorkContext workContext,
        IStoreContext storeContext)
    {
        _logger = logger;
        _localizationService = localizationService;
        _notificationService = notificationService;
        _settingService = settingService;
        _orderProcessingService = orderProcessingService;
        _storeContext = storeContext;
        _bluefinPaymentSettings = bluefinPaymentSettings;
        _genericAttributeService = genericAttributeService;
        _bluefinTokenRepositoryService = bluefinTokenRepositoryService;
        _workContext = workContext;
        _gateway = new BluefinGateway(_logger, _bluefinPaymentSettings);
    }

    [Area(AreaNames.ADMIN)]
    public async Task<IActionResult> Configure()
    {
        var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var bluefinPaymentSettings = await _settingService.LoadSettingAsync<BluefinPaymentSettings>(storeScope);

        var model = new ConfigurationModel
        {
            EnableLogging = bluefinPaymentSettings.EnableLogging,
            UseSandbox = bluefinPaymentSettings.UseSandbox,
            Use3DS = bluefinPaymentSettings.Use3DS,
            UseAuthorizeOnly = bluefinPaymentSettings.UseAuthorizeOnly,
            ApiKeyId = bluefinPaymentSettings.ApiKeyId,
            ApiKeySecret = bluefinPaymentSettings.ApiKeySecret,
            AccountId = bluefinPaymentSettings.AccountId,
            IFrameConfigId = bluefinPaymentSettings.IFrameConfigId
        };

        // Load and display settings
        return View("~/Plugins/Payments.Bluefin/Views/Configure.cshtml", model);
    }

    [HttpPost]
    // [AuthorizeAdmin]
    [Area(AreaNames.ADMIN)]
    public async Task<IActionResult> Configure(ConfigurationModel model)
    {

        var settings = new BluefinPaymentSettings
        {
            EnableLogging = model.EnableLogging,
            UseSandbox = model.UseSandbox,
            Use3DS = model.Use3DS,
            UseAuthorizeOnly = model.UseAuthorizeOnly,
            ApiKeyId = model.ApiKeyId,
            ApiKeySecret = model.ApiKeySecret,
            AccountId = model.AccountId,
            IFrameConfigId = model.IFrameConfigId
        };

        await _settingService.SaveSettingAsync(settings);

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

        return await Configure();
    }

    [HttpPost]
    public async Task<IActionResult> InitIframe(BluefinCustomer customer)
    {
        var nop_customer = await _workContext.GetCurrentCustomerAsync();
        var nop_store = await _storeContext.GetCurrentStoreAsync();

        var IbfTokenReferences = await _bluefinTokenRepositoryService.GetBluefinTokensByCustomerIdAsync(customer.CustomerId);
        var bfTokenReferences = new List<string>(IbfTokenReferences.Select(x => x.BfTokenReference));

        dynamic res = await _gateway.IframeInit(customer, bfTokenReferences);

        await _genericAttributeService.SaveAttributeAsync(nop_customer, "bfTransactionId", res.transactionId, nop_store.Id);


        return Json(res);
    }

    [HttpPost]
    public async Task<IActionResult> SetBluefinToken(string bfTokenReference)
    {

        var nop_customer = await _workContext.GetCurrentCustomerAsync();
        var nop_store = await _storeContext.GetCurrentStoreAsync();

        await _genericAttributeService.SaveAttributeAsync(nop_customer, "bfTokenReference", bfTokenReference, nop_store.Id);


        return Json(new { ok = true });
    }

    [HttpPost]
    public async Task<IActionResult> StoreBluefinToken(int customerId, string bfTokenReference)
    {

        await _bluefinTokenRepositoryService.InsertAsync(
            new BluefinTokenEntry
            {
                CustomerId = customerId,
                BfTokenReference = bfTokenReference
            }
        );


        return Json(new { ok = true });
    }


    public IActionResult PaymentInfo()
    {
        // var model = new PaymentInfoModel();
        return View("~/Plugins/Payments.Bluefin/Views/PaymentInfo.cshtml"); // , model);
    }
}