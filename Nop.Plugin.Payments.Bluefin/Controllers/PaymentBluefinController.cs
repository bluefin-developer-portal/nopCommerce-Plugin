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
using Nop.Services.Security;

using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;

using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;


// [HttpsRequirement]
// [AutoValidateAntiforgeryToken]
// [ValidateIpAddress]
// [AuthorizeAdmin]
// [ValidateVendor]
public class PaymentBluefinController : BasePaymentController
{
    // private readonly ILogger _logger;
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
        // _logger = logger;
        _localizationService = localizationService;
        _notificationService = notificationService;
        _settingService = settingService;
        _orderProcessingService = orderProcessingService;
        _storeContext = storeContext;
        _bluefinPaymentSettings = bluefinPaymentSettings;
        _genericAttributeService = genericAttributeService;
        _bluefinTokenRepositoryService = bluefinTokenRepositoryService;
        _workContext = workContext;
        _gateway = new BluefinGateway(logger, _bluefinPaymentSettings);
    }

    [Area(AreaNames.ADMIN)]
    [CheckPermission(StandardPermission.Configuration.MANAGE_PAYMENT_METHODS)]
    public async Task<IActionResult> MyIndex()
    {
        // var data = await _dbContext.BluefinEntries.ToListAsync();
        return View("~/Plugins/Payments.Bluefin/Views/MyIndex.cshtml"); // , data);
    }

    [Area(AreaNames.ADMIN)]
    [CheckPermission(StandardPermission.Configuration.MANAGE_PAYMENT_METHODS)]
    public async Task<IActionResult> Configure()
    {
        var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var bluefinPaymentSettings = await _settingService.LoadSettingAsync<BluefinPaymentSettings>(storeScope);

        var model = new ConfigurationModel
        {
            ActiveStoreScopeConfiguration = storeScope,
            EnableLogging = bluefinPaymentSettings.EnableLogging,
            UseSandbox = bluefinPaymentSettings.UseSandbox,
            Use3DS = bluefinPaymentSettings.Use3DS,
            UseAuthorizeOnly = bluefinPaymentSettings.UseAuthorizeOnly,
            ApiKeyId = bluefinPaymentSettings.ApiKeyId,
            ApiKeySecret = bluefinPaymentSettings.ApiKeySecret,
            AccountId = bluefinPaymentSettings.AccountId,
            IFrameConfigId = bluefinPaymentSettings.IFrameConfigId,
            ThreeDTransType = bluefinPaymentSettings.ThreeDTransType,
            DeliveryTimeFrame = bluefinPaymentSettings.DeliveryTimeFrame,
            ThreeDSecureChallengeIndicator = bluefinPaymentSettings.ThreeDSecureChallengeIndicator,
            ReorderIndicator = bluefinPaymentSettings.ReorderIndicator,
            ShippingIndicator = bluefinPaymentSettings.ShippingIndicator,
            IframeResponsive = bluefinPaymentSettings.IframeResponsive,
            IframeWidth = bluefinPaymentSettings.IframeWidth,
            IframeHeight = bluefinPaymentSettings.IframeHeight,
            EnableCard = bluefinPaymentSettings.EnableCard,
            EnableACH = bluefinPaymentSettings.EnableACH,
            EnableGooglePay = bluefinPaymentSettings.EnableGooglePay,
            EnableClickToPay = bluefinPaymentSettings.EnableClickToPay
        };

        await _gateway.LogDebug("DEBUG: storeScope " + storeScope.ToString(), "");

        if (storeScope > 0)
        {
            model.ApiKeyId_OverrideForStore = await _settingService.SettingExistsAsync(bluefinPaymentSettings, settings => settings.ApiKeyId, storeScope);
            model.ApiKeySecret_OverrideForStore = await _settingService.SettingExistsAsync(bluefinPaymentSettings, settings => settings.ApiKeySecret, storeScope);
            model.EnableLogging_OverrideForStore = await _settingService.SettingExistsAsync(bluefinPaymentSettings, settings => settings.EnableLogging, storeScope);
            model.UseSandbox_OverrideForStore = await _settingService.SettingExistsAsync(bluefinPaymentSettings, settings => settings.UseSandbox, storeScope);
            model.Use3DS_OverrideForStore = await _settingService.SettingExistsAsync(bluefinPaymentSettings, settings => settings.Use3DS, storeScope);
            model.UseAuthorizeOnly_OverrideForStore = await _settingService.SettingExistsAsync(bluefinPaymentSettings, settings => settings.UseAuthorizeOnly, storeScope);
            model.AccountId_OverrideForStore = await _settingService.SettingExistsAsync(bluefinPaymentSettings, settings => settings.AccountId, storeScope);
            model.IFrameConfigId_OverrideForStore = await _settingService.SettingExistsAsync(bluefinPaymentSettings, settings => settings.IFrameConfigId, storeScope);
            model.ThreeDTransType_OverrideForStore = await _settingService.SettingExistsAsync(bluefinPaymentSettings, settings => settings.ThreeDTransType, storeScope);
            model.DeliveryTimeFrame_OverrideForStore = await _settingService.SettingExistsAsync(bluefinPaymentSettings, settings => settings.DeliveryTimeFrame, storeScope);
            model.ThreeDSecureChallengeIndicator_OverrideForStore = await _settingService.SettingExistsAsync(bluefinPaymentSettings, settings => settings.ThreeDSecureChallengeIndicator, storeScope);
            model.ReorderIndicator_OverrideForStore = await _settingService.SettingExistsAsync(bluefinPaymentSettings, settings => settings.ReorderIndicator, storeScope);
            model.ShippingIndicator_OverrideForStore = await _settingService.SettingExistsAsync(bluefinPaymentSettings, settings => settings.ShippingIndicator, storeScope);
            model.IframeResponsive_OverrideForStore = await _settingService.SettingExistsAsync(bluefinPaymentSettings, settings => settings.IframeResponsive, storeScope);
            model.IframeWidth_OverrideForStore = await _settingService.SettingExistsAsync(bluefinPaymentSettings, settings => settings.IframeWidth, storeScope);
            model.IframeHeight_OverrideForStore = await _settingService.SettingExistsAsync(bluefinPaymentSettings, settings => settings.IframeHeight, storeScope);
            model.EnableCard_OverrideForStore = await _settingService.SettingExistsAsync(bluefinPaymentSettings, settings => settings.EnableCard, storeScope);
            model.EnableACH_OverrideForStore = await _settingService.SettingExistsAsync(bluefinPaymentSettings, settings => settings.EnableACH, storeScope);
            model.EnableGooglePay_OverrideForStore = await _settingService.SettingExistsAsync(bluefinPaymentSettings, settings => settings.EnableGooglePay, storeScope);
            model.EnableClickToPay_OverrideForStore = await _settingService.SettingExistsAsync(bluefinPaymentSettings, settings => settings.EnableClickToPay, storeScope);
        }

        // Load and display settings
        return View("~/Plugins/Payments.Bluefin/Views/Configure.cshtml", model);
    }

    [HttpPost]
    [Area(AreaNames.ADMIN)]
    [CheckPermission(StandardPermission.Configuration.MANAGE_PAYMENT_METHODS)]
    public async Task<IActionResult> Configure(ConfigurationModel model)
    {

        var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        
        var bluefinPaymentSettings = await _settingService.LoadSettingAsync<BluefinPaymentSettings>(storeScope);


        bluefinPaymentSettings.EnableLogging = model.EnableLogging;
        bluefinPaymentSettings.UseSandbox = model.UseSandbox;
        bluefinPaymentSettings.Use3DS = model.Use3DS;
        bluefinPaymentSettings.UseAuthorizeOnly = model.UseAuthorizeOnly;
        bluefinPaymentSettings.ApiKeyId = model.ApiKeyId;
        bluefinPaymentSettings.ApiKeySecret = model.ApiKeySecret;
        bluefinPaymentSettings.AccountId = model.AccountId;
        bluefinPaymentSettings.IFrameConfigId = model.IFrameConfigId;
        bluefinPaymentSettings.ThreeDTransType = model.ThreeDTransType;
        bluefinPaymentSettings.DeliveryTimeFrame = model.DeliveryTimeFrame;
        bluefinPaymentSettings.ThreeDSecureChallengeIndicator = model.ThreeDSecureChallengeIndicator;
        bluefinPaymentSettings.ReorderIndicator = model.ReorderIndicator;
        bluefinPaymentSettings.ShippingIndicator = model.ShippingIndicator;
        bluefinPaymentSettings.IframeResponsive = model.IframeResponsive;
        bluefinPaymentSettings.IframeWidth = model.IframeWidth;
        bluefinPaymentSettings.IframeHeight = model.IframeHeight;
        bluefinPaymentSettings.EnableCard = model.EnableCard;
        bluefinPaymentSettings.EnableACH = model.EnableACH;
        bluefinPaymentSettings.EnableGooglePay = model.EnableGooglePay;
        bluefinPaymentSettings.EnableClickToPay = model.EnableClickToPay;

        await _settingService.SaveSettingOverridablePerStoreAsync(bluefinPaymentSettings, settings => settings.ApiKeyId, model.ApiKeyId_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(bluefinPaymentSettings, settings => settings.ApiKeySecret, model.ApiKeySecret_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(bluefinPaymentSettings, settings => settings.EnableLogging, model.EnableLogging_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(bluefinPaymentSettings, settings => settings.UseSandbox, model.UseSandbox_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(bluefinPaymentSettings, settings => settings.Use3DS, model.Use3DS_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(bluefinPaymentSettings, settings => settings.UseAuthorizeOnly, model.UseAuthorizeOnly_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(bluefinPaymentSettings, settings => settings.AccountId, model.AccountId_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(bluefinPaymentSettings, settings => settings.IFrameConfigId, model.IFrameConfigId_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(bluefinPaymentSettings, settings => settings.ThreeDTransType, model.ThreeDTransType_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(bluefinPaymentSettings, settings => settings.DeliveryTimeFrame, model.DeliveryTimeFrame_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(bluefinPaymentSettings, settings => settings.ThreeDSecureChallengeIndicator, model.ThreeDSecureChallengeIndicator_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(bluefinPaymentSettings, settings => settings.ReorderIndicator, model.ReorderIndicator_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(bluefinPaymentSettings, settings => settings.ShippingIndicator, model.ShippingIndicator_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(bluefinPaymentSettings, settings => settings.IframeResponsive, model.IframeResponsive_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(bluefinPaymentSettings, settings => settings.IframeWidth, model.IframeWidth_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(bluefinPaymentSettings, settings => settings.IframeHeight, model.IframeHeight_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(bluefinPaymentSettings, settings => settings.EnableCard, model.EnableCard_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(bluefinPaymentSettings, settings => settings.EnableACH, model.EnableACH_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(bluefinPaymentSettings, settings => settings.EnableGooglePay, model.EnableGooglePay_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(bluefinPaymentSettings, settings => settings.EnableClickToPay, model.EnableClickToPay_OverrideForStore, storeScope, false);

        // await _settingService.ClearCacheAsync();
        await _settingService.SaveSettingAsync(bluefinPaymentSettings, storeScope);

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
    
}