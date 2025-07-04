using Nop.Services.Payments;
using Nop.Services.Plugins;
using Nop.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

using Newtonsoft.Json;

using Nop.Core.Domain.Common;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Logging;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;

using Nop.Services.Attributes;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;

using Nop.Plugin.Payments.Bluefin.Components;
using Nop.Plugin.Payments.Bluefin.Domain;
using Nop.Plugin.Payments.Bluefin.Services;


namespace Nop.Plugin.Payments.Bluefin;

/// <summary>
/// Rename this file and change to the correct type
/// </summary>
public class BluefinPaymentProcessor : BasePlugin, IPaymentMethod
{
    #region Fields
    // private readonly ILogger _logger;

    private readonly IGenericAttributeService _genericAttributeService;
    private readonly IWorkContext _workContext;
    private readonly IStoreContext _storeContext;
    
    private readonly ILocalizationService _localizationService;

    private readonly ISettingService _settingService;
    private readonly INotificationService _notificationService;
    
    private readonly IWebHelper _webHelper;

    private readonly BluefinPaymentSettings _bluefinPaymentSettings;

    private readonly BluefinGateway _gateway;

    #endregion

    #region Ctor

    public BluefinPaymentProcessor(ILogger logger,
        IGenericAttributeService genericAttributeService,
        ILocalizationService localizationService,
        ISettingService settingService,
        INotificationService notificationService,
        IWebHelper webHelper,
        IWorkContext workContext,
        IStoreContext storeContext,
        BluefinPaymentSettings bluefinPaymentSettings
        )
    {
        _storeContext = storeContext;
        _genericAttributeService = genericAttributeService;
        _workContext = workContext;
        _notificationService = notificationService;
        _localizationService = localizationService;
        _settingService = settingService;
        _webHelper = webHelper;
        _bluefinPaymentSettings = bluefinPaymentSettings;
        _gateway = new BluefinGateway(logger, _bluefinPaymentSettings);
    }

    #endregion


    public override async Task InstallAsync()
    {
        await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
        {
            ["Plugins.Payments.Bluefin.Fields.ApiKeyId"] = "API Key Identifier",
            ["Plugins.Payments.Bluefin.Fields.ApiKeyId.Hint"] = "Bluefin API Key Identifier",
            ["Plugins.Payments.Bluefin.Fields.ApiKeyId.Required"] = "API Key Identifier is Required",
            ["Plugins.Payments.Bluefin.Fields.ApiKeySecret"] = "API Key Secret",
            ["Plugins.Payments.Bluefin.Fields.ApiKeySecret.Hint"] = "Bluefin API Key Secret",
            ["Plugins.Payments.Bluefin.Fields.ApiKeySecret.Required"] = "API Key Secret is Required",
            ["Plugins.Payments.Bluefin.Fields.IFrameConfigId"] = "iFrame Configuration Identifier",
            ["Plugins.Payments.Bluefin.Fields.IFrameConfigId.Hint"] = "iFrame Configuration used by the Checkout Component. Preconfigure payment methods and their settings",
            ["Plugins.Payments.Bluefin.Fields.IFrameConfigId.Required"] = "IFrame Configuration Identifier is Required",
            ["Plugins.Payments.Bluefin.Fields.AccountId"] = "Account Identifer",
            ["Plugins.Payments.Bluefin.Fields.AccountId.Required"] = "Account Identifer is required",
            ["Plugins.Payments.Bluefin.Fields.UseSandbox"] = "Use sandbox environment",
            ["Plugins.Payments.Bluefin.Fields.UseSandbox.Hint"] = "By enabling this property, they are using Blufin PayConex certification enviroment",
            ["Plugins.Payments.Bluefin.PaymentMethodDescription"] = "Pay With Bluefin",
            ["Plugins.Payments.Bluefin.Fields.UseAuthorizeOnly"] = "Authorize only (capture manually in the admin)",
            ["Plugins.Payments.Bluefin.Fields.UseAuthorizeOnly.Hint"] = "After Order Confirm, Admin -> Order -> Select the order to capture and click on Capture",
            ["Plugins.Payments.Bluefin.Fields.Use3DS"] = "Use 3D Secure",
            ["Plugins.Payments.Bluefin.Fields.Use3DS.Hint"] = "Use 3D Secure for the Checkout Component, Card Payment Method. If this setting is enabled, the threeDSecureInitSettings below must be configured according to your needs. Note that this setting is required if cardSettings.threeDSecure is defined as \"required\".",
            ["Plugins.Payments.Bluefin.Fields.EnableLogging"] = "Enable Logging",
            ["Plugins.Payments.Bluefin.Fields.EnableLogging.Hint"] = "Enable Logging for debugging purposes. This setting is primarily used in development",

            ["Plugins.Payments.Bluefin.Fields.ThreeDTransType"] = "3DS Transaction Type",
            ["Plugins.Payments.Bluefin.Fields.ThreeDTransType.Hint"] = "Each option provides context about the nature of the transaction, helping to ensure accurate processing and risk assessment.",
            ["Plugins.Payments.Bluefin.Fields.DeliveryTimeFrame"] = "Delivery Time Frame",
            ["Plugins.Payments.Bluefin.Fields.DeliveryTimeFrame.Hint"] = "As the setting name suggests, this is the time for the goods to be delivered. The descriptions are pretty much self-explanatory given the options.",
            ["Plugins.Payments.Bluefin.Fields.ThreeDSecureChallengeIndicator"] = "3D Secure Challenge Indicator",
            ["Plugins.Payments.Bluefin.Fields.ThreeDSecureChallengeIndicator.Hint"] = "Indicates whether a challenge is preferred, mandated, or requested for the transaction.",
            ["Plugins.Payments.Bluefin.Fields.ReorderIndicator"] = "Reorder Indicator",
            ["Plugins.Payments.Bluefin.Fields.ReorderIndicator.Hint"] = "This setting indicates whether the order is new or was ordered before.",
            ["Plugins.Payments.Bluefin.Fields.ShippingIndicator"] = "Shipping Indicator",
            ["Plugins.Payments.Bluefin.Fields.ShippingIndicator.Hint"] = "Specifies the type of Shipping",

        });

        await base.InstallAsync();
    }

    public override async Task UninstallAsync()
    {

        // Settings
        await _settingService.DeleteSettingAsync<BluefinPaymentSettings>();

        // Locales
        await _localizationService.DeleteLocaleResourcesAsync("Plugins.Payments.Bluefin");


        await base.UninstallAsync();
    }

    public Task<IList<string>> ValidatePaymentFormAsync(IFormCollection form)
    {

        var warnings = new List<string>();

        if (form.TryGetValue("IFrameCompleted", out var IFrameCompleted) && StringValues.IsNullOrEmpty(IFrameCompleted))
        {
            warnings.Add("Please, complete the Bluefin Checkout");
        }

        return Task.FromResult<IList<string>>(warnings);
    }

    public override string GetConfigurationPageUrl()
    {
        return $"{_webHelper.GetStoreLocation()}Admin/PaymentBluefin/Configure";
    }

    public bool SupportCapture => true;
    public bool SupportVoid => false;
    public bool SupportRefund => true;

    public bool SupportPartiallyRefund => true;


    public RecurringPaymentType RecurringPaymentType => RecurringPaymentType.NotSupported;
    public PaymentMethodType PaymentMethodType => PaymentMethodType.Standard;
    public bool SkipPaymentInfo => false;

    public async Task<ProcessPaymentResult> ProcessPaymentAsync(ProcessPaymentRequest processPaymentRequest)
    {
        // processPaymentRequest.CustomerId

        var nop_customer = await _workContext.GetCurrentCustomerAsync();
        var nop_store = await _storeContext.GetCurrentStoreAsync();

        string orderGuid = processPaymentRequest.OrderGuid.ToString();

        var currency = await _workContext.GetWorkingCurrencyAsync();

        await _gateway.LogDebug(
            "ProcessPaymentRequest orderGuid: " + orderGuid,
            "Transaction Metadata: "
            );

        string bfTokenReference = await _genericAttributeService.GetAttributeAsync<string>(nop_customer, "bfTokenReference", nop_store.Id);
        string bfTransactionId = await _genericAttributeService.GetAttributeAsync<string>(nop_customer, "bfTransactionId", nop_store.Id);

        TransactionResponse transaction_res = null;

        var processPaymentResult = new ProcessPaymentResult();

        var transaction = new Transaction
        {
            TransactionId = bfTransactionId,
            Total = processPaymentRequest.OrderTotal.ToString(),
            Currency = currency.CurrencyCode,
            BfTokenReference = bfTokenReference
        };


        if (_bluefinPaymentSettings.UseAuthorizeOnly)
        {
            transaction_res = await _gateway.ProcessAuthorization(transaction);
            // processPaymentRequest.CustomValues.Add("Bluefin Transaction Type", "Authorization");
        }
        else
        {
            transaction_res = await _gateway.ProcessSale(transaction);
            // processPaymentRequest.CustomValues.Add("Bluefin Transaction Type", "Sale");
        }


        if (transaction_res.IsSuccess)
        {


            await _gateway.LogDebug(
                "Triggered ProcessPaymentAsync bfTokenReference: " + bfTokenReference,
                "Transaction Res Metadata: " + JsonConvert.SerializeObject(transaction_res.Metadata)
                );

            processPaymentRequest.CustomValues.Add("Bluefin Transaction Identifier", transaction_res.Metadata.transactionId);
            // processPaymentRequest.CustomValues.Add("Bluefin Transaction Status", transaction_res.metadata.status);

            // // TODO: Proper Delete. However, this suffices
            await _genericAttributeService.SaveAttributeAsync<string>(
                nop_customer,
                "bfTokenReference",
                null,
                nop_store.Id
            );

            await _genericAttributeService.SaveAttributeAsync<string>(
                nop_customer,
                "bfTransactionId",
                null,
                nop_store.Id
            );

            await _gateway.LogDebug(
                "Generic attribute cleanup",
                await _genericAttributeService.GetAttributeAsync<string>(nop_customer, "bfTokenReference", nop_store.Id)
            );

            // See: https://webiant.com/docs/nopcommerce/Libraries/Nop.Core/Domain/Payments/PaymentStatus
            if (_bluefinPaymentSettings.UseAuthorizeOnly)
            {
                processPaymentResult.NewPaymentStatus = PaymentStatus.Authorized;
            }
            else
            {
                processPaymentResult.NewPaymentStatus = PaymentStatus.Paid;
            }
        }
        else
        {
            processPaymentResult.NewPaymentStatus = PaymentStatus.Pending;

            string err_message = JsonConvert.SerializeObject(transaction_res.Metadata);
            processPaymentResult.AddError(err_message);
        }

        return processPaymentResult;
    }

    public Task PostProcessPaymentAsync(PostProcessPaymentRequest postProcessPaymentRequest)
    {
        return Task.CompletedTask;
    }

    public Task<bool> HidePaymentMethodAsync(IList<ShoppingCartItem> cart)
    {
        //you can put any logic here
        //for example, hide this payment method if all products in the cart are downloadable
        //or hide this payment method if current customer is from certain country
        return Task.FromResult(false);
    }

    public Task<decimal> GetAdditionalHandlingFeeAsync(IList<ShoppingCartItem> cart)
    {
        return Task.FromResult(decimal.Zero);
    }

    public async Task<CapturePaymentResult> CaptureAsync(CapturePaymentRequest capturePaymentRequest)
    {
        var capturePaymentResult = new CapturePaymentResult();

        string bfTransactionId = Utility.ParseBfTransactionId(capturePaymentRequest.Order.CustomValuesXml);

        await _gateway.LogDebug(
            "CaptureAsync CustomValuesXML",
            "bfTransactionId:" + bfTransactionId
        );

        TransactionResponse transaction_res = await _gateway.CaptureAuthorization(bfTransactionId);

        if (transaction_res.IsSuccess)
        {
            // See https://webiant.com/docs/nopcommerce/Libraries/Nop.Services/Messages/INotificationService
            _notificationService.SuccessNotification("Bluefin Transaction #" + bfTransactionId + " captured!");
            capturePaymentResult.NewPaymentStatus = PaymentStatus.Paid;

        }
        else
        {
            string err_message = "There has been an error while capturing the transaction: "
                            + new StringContent(JsonConvert.SerializeObject(transaction_res.Metadata));
            _notificationService.ErrorNotification(err_message);

            capturePaymentResult.AddError(err_message);

            // capturePaymentResult.NewPaymentStatus = PaymentStatus.Pending; // Authorized or Pending in this case
        }

        return capturePaymentResult;
    }

    public async Task<RefundPaymentResult> RefundAsync(RefundPaymentRequest refundPaymentRequest)
    {
        var refundResult = new RefundPaymentResult();
        var amount = refundPaymentRequest.AmountToRefund.ToString("0.00");

        await _gateway.LogDebug(
            "Triggered RefundAsync amount: " + amount,
            "Transaction Res Metadata: "
        );


        /*
        NOTE: max(AmountToRefund, OrderTotal) is the case before we even get to this point
        if (refundPaymentRequest.AmountToRefund > refundPaymentRequest.Order.OrderTotal)
        {
            string err_message = "Maximum Refund Amount is " + refundPaymentRequest.Order.OrderTotal.ToString("0.00");
            refundResult.AddError(err_message);
            return refundResult;
        }
        */

        string bfTransactionId = Utility.ParseBfTransactionId(refundPaymentRequest.Order.CustomValuesXml);

        var refund_transaction = new RefundTransaction
        {
            TransactionId = bfTransactionId,
            AmountToRefund = amount,
            Currency = refundPaymentRequest.Order.CustomerCurrencyCode
        };

        var refunded_res = await _gateway.ProcessRefund(refund_transaction);

        if (refunded_res.IsSuccess)
        {
            _notificationService.SuccessNotification("Refunded Transaction #"
                + bfTransactionId + " with the amount of: "
                + amount
                + " " + refund_transaction.Currency
                );
            refundResult.NewPaymentStatus = refundPaymentRequest.IsPartialRefund ? PaymentStatus.PartiallyRefunded : PaymentStatus.Refunded;
        }
        else
        {
            string err_message = "There has been an error while refunding the transaction: "
                + JsonConvert.SerializeObject(refunded_res.Metadata);

            _notificationService.ErrorNotification(err_message);

            refundResult.AddError(err_message);
        }

        return refundResult;
    }

    public Task<VoidPaymentResult> VoidAsync(VoidPaymentRequest voidPaymentRequest)
    {
        var result = new VoidPaymentResult();
        result.AddError("Void method not supported");

        return Task.FromResult(result);
    }

    public Task<ProcessPaymentResult> ProcessRecurringPaymentAsync(ProcessPaymentRequest processPaymentRequest)
    {
        var result = new ProcessPaymentResult();
        result.AddError("Recurring payment not supported");

        return Task.FromResult(result); 
    }

    public Task<CancelRecurringPaymentResult> CancelRecurringPaymentAsync(CancelRecurringPaymentRequest cancelPaymentRequest)
    {
        var result = new CancelRecurringPaymentResult();
        result.AddError("Recurring payment not supported");

        return Task.FromResult(result);
    }

    public Task<bool> CanRePostProcessPaymentAsync(Order order)
    {
        return Task.FromResult(false);
    }

    public Task<ProcessPaymentRequest> GetPaymentInfoAsync(IFormCollection form)
    {
        if (form == null)
            throw new ArgumentNullException(nameof(form));

        var paymentInfo = new ProcessPaymentRequest();

        return Task.FromResult(paymentInfo);
    }

    public Type GetPublicViewComponent()
    {
        return typeof(PaymentBluefinViewComponent);
    }

    public async Task<string> GetPaymentMethodDescriptionAsync()
    {
        return await _localizationService.GetResourceAsync("Plugins.Payments.Bluefin.PaymentMethodDescription");
    }

}
