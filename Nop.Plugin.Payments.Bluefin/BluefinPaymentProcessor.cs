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
using Nop.Services.Orders;

using Nop.Web.Factories;

using Nop.Core.Domain.Vendors;


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

    // private readonly IGenericAttributeService _genericAttributeService;
    private readonly IWorkContext _workContext;

    private readonly IProductService _productService;

    private readonly IShoppingCartService _shoppingCartService;

    private readonly IShoppingCartModelFactory _shoppingCartModelFactory;


    protected readonly IProductAttributeParser _productAttributeParser;
    protected readonly IProductAttributeService _productAttributeService;

    protected readonly IAttributeParser<VendorAttribute, VendorAttributeValue> _vendorAttributeParser;


    private readonly IStoreContext _storeContext;
    
    private readonly ILocalizationService _localizationService;

    private readonly ISettingService _settingService;
    private readonly INotificationService _notificationService;
    
    private readonly IWebHelper _webHelper;

    private readonly BluefinPaymentSettings _bluefinPaymentSettings;
    private readonly BluefinTokenRepositoryService _bluefinTokenRepositoryService;
    private readonly ReissueOrdersRepositoryService _reissueOrdersRepositoryService;

    private readonly BluefinGateway _gateway;

    #endregion

    #region Ctor

    public BluefinPaymentProcessor(ILogger logger,
        ILocalizationService localizationService,
        ISettingService settingService,
        INotificationService notificationService,
        IWebHelper webHelper,
        IWorkContext workContext,
        IStoreContext storeContext,
        BluefinPaymentSettings bluefinPaymentSettings,
        BluefinTokenRepositoryService bluefinTokenRepositoryService,
        ReissueOrdersRepositoryService reissueOrdersRepositoryService,
        TraceLogsRepositoryService traceLogsRepositoryService,
        IProductService productService,
        IShoppingCartService shoppingCartService,
        IShoppingCartModelFactory shoppingCartModelFactory,
        IProductAttributeParser productAttributeParser,
        IProductAttributeService productAttributeService,
        IAttributeParser<VendorAttribute, VendorAttributeValue> vendorAttributeParser
        )
    {
        _storeContext = storeContext;
        _workContext = workContext;
        _productService = productService;
        _shoppingCartService = shoppingCartService;
        _shoppingCartModelFactory = shoppingCartModelFactory;
        _vendorAttributeParser = vendorAttributeParser;
        _notificationService = notificationService;
        _localizationService = localizationService;
        _settingService = settingService;
        _webHelper = webHelper;
        _bluefinPaymentSettings = bluefinPaymentSettings;
        _bluefinTokenRepositoryService = bluefinTokenRepositoryService;
        _reissueOrdersRepositoryService = reissueOrdersRepositoryService;
        _gateway = new BluefinGateway(
            logger,
            _bluefinPaymentSettings,
            traceLogsRepositoryService
        );

        _productAttributeParser = productAttributeParser;
        _productAttributeService = productAttributeService;
        
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
            ["Plugins.Payments.Bluefin.Fields.UseSandbox.Hint"] = "By enabling this property, they are using Bluefin PayConex certification enviroment",
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

            ["Plugins.Payments.Bluefin.Fields.IframeResponsive"] = "Responsive iframe",
            ["Plugins.Payments.Bluefin.Fields.IframeResponsive.Hint"] = "Enable responsive iframe that automatically adjusts height to hide scrollbars",
            ["Plugins.Payments.Bluefin.Fields.IframeWidth"] = "Iframe Width",
            ["Plugins.Payments.Bluefin.Fields.IframeWidth.Hint"] = "Width of the payment iframe (e.g., 100%, 500px, 50vw)",
            ["Plugins.Payments.Bluefin.Fields.IframeWidth.Required"] = "Iframe Width is required",
            ["Plugins.Payments.Bluefin.Fields.IframeHeight"] = "Iframe Height",
            ["Plugins.Payments.Bluefin.Fields.IframeHeight.Hint"] = "Height of the payment iframe (e.g., 600px, 60vh, 400px)",
            ["Plugins.Payments.Bluefin.Fields.IframeHeight.Required"] = "Iframe Height is required",
            ["Plugins.Payments.Bluefin.Fields.IframeTimeout"] = "Iframe Timeout",
            ["Plugins.Payments.Bluefin.Fields.IframeTimeout.Hint"] = "The user interaction limit timeout in seconds.",
            ["Plugins.Payments.Bluefin.Fields.IframeTimeout.Required"] = "Iframe Timeout is required",

            ["Plugins.Payments.Bluefin.Fields.EnableCard"] = "Credit/Debit Card",
            ["Plugins.Payments.Bluefin.Fields.EnableACH"] = "ACH (Bank Transfer)",
            ["Plugins.Payments.Bluefin.Fields.EnableGooglePay"] = "Google Pay",
            ["Plugins.Payments.Bluefin.Fields.EnableClickToPay"] = "Click to Pay",
            ["Plugins.Payments.Bluefin.Fields.PaymentMethod.Required"] = "At least one payment method must be selected.",


            ["Plugins.Payments.Bluefin.Fields.ReissueOrder.OriginalPaymentId"] = "Original Order Id",
            ["Plugins.Payments.Bluefin.Fields.ReissueOrder.OriginalOrderStatus"] = "Original Order Status",
            ["Plugins.Payments.Bluefin.Fields.ReissueOrder.OriginalPaymentStatus"] = "Original Order Payment Status",
            ["Plugins.Payments.Bluefin.Fields.ReissueOrder.OriginalOrderTotal"] = "Original Order Total",
            ["Plugins.Payments.Bluefin.Fields.ReissueOrder.ReissueTotal"] = "Reissue Total (Decimal)",
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

    public bool IsTokenVaulated(TransactionResponse transaction_res)
    {
        return transaction_res.Metadata.bfTokenReference != null;
    }

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



        string productAttributes_string = "";

        /*
        var shopping_model_factory = await _shoppingCartModelFactory.PrepareMiniShoppingCartModelAsync();
        int item_c = 0;

        
        foreach (var item in shopping_model_factory.Items)
        {

            if (!string.IsNullOrEmpty(item.AttributeInfo))
            {

                // NOTE: Multi-line textbox will also include "<br/ >" so this is NOT optimal long-term.
                productAttributes_string += item.ProductName + ": "
                            + item.AttributeInfo.Replace("<br />", ", ");

                if (item_c < shopping_model_factory.Items.Count - 1)
                {

                    productAttributes_string += '\n'; // NOTE: separator
                }

            }

            item_c++;
        }
        await _gateway.LogDebug("productAttributes_string: ", productAttributes_string);
        */

        var CustomValues = processPaymentRequest.CustomValues;

        string bfTokenReference = CustomValues.ContainsKey("bfTokenReference") ? (string)CustomValues["bfTokenReference"] : "";
        string paymentType = CustomValues.ContainsKey("paymentType") ? (string)CustomValues["paymentType"] : "";

        string bfTransactionId = CustomValues.ContainsKey("bfTransactionId") ? (string)CustomValues["bfTransactionId"] : "";
        string savePaymentOption = CustomValues.ContainsKey("savePaymentOption") ? (string)CustomValues["savePaymentOption"] : "";

        TransactionResponse transaction_res = null;

        var processPaymentResult = new ProcessPaymentResult();

        processPaymentRequest.CustomValues.Add("Bluefin Payment Type", paymentType);

        if (paymentType == "ACH")
        {
            var transaction = new Transaction
            {
                TransactionId = bfTransactionId,
                Total = processPaymentRequest.OrderTotal.ToString(),
                Currency = currency.CurrencyCode,
                BfTokenReference = bfTokenReference,
                Description = productAttributes_string,
                CustomId = orderGuid,
            };

            transaction_res = await _gateway.ProcessACHSale(transaction);
            // processPaymentRequest.CustomValues.Add("Bluefin Payment Type", "ACH");


            if (transaction_res.IsSuccess)
            {

                await _gateway.LogDebug(
                    "Triggered ProcessPaymentAsync bfTokenReference: " + bfTokenReference,
                    "Transaction ACH Res Metadata: " + JsonConvert.SerializeObject(transaction_res.Metadata)
                    );

                processPaymentRequest.CustomValues.Add("Bluefin Transaction Identifier", transaction_res.Metadata.transactionId);

                // processPaymentRequest.CustomValues.Add("Bluefin Transaction Status", transaction_res.metadata.status);

                // See: https://webiant.com/docs/nopcommerce/Libraries/Nop.Core/Domain/Payments/PaymentStatus
                processPaymentResult.NewPaymentStatus = PaymentStatus.Paid;
            }
            else
            {
                processPaymentResult.NewPaymentStatus = PaymentStatus.Pending;

                dynamic metadata = transaction_res.Metadata;

                string err_message;

                if (metadata.status == "DECLINED")
                {
                    string resource_message = await GetPluginResourceAsync("Plugins.Payments.Bluefin.Error.PaymentDeclined");
                    string processorMessage = string.IsNullOrEmpty((string)metadata.auth.processorMessage) ? "" : metadata.auth.processorMessage;

                    err_message = string.IsNullOrEmpty(resource_message) ?
                        "Transaction #" + metadata.transactionId + " has been declined: " + processorMessage
                        : resource_message;
                }
                else if (metadata.status == "FAILED")
                {
                    string resource_message = await GetPluginResourceAsync("Plugins.Payments.Bluefin.Error.PaymentFailed");
                    string processorMessage = string.IsNullOrEmpty((string)metadata.auth.processorMessage) ? "" : metadata.auth.processorMessage;

                    err_message = string.IsNullOrEmpty(resource_message) ?
                        "Transaction #" + metadata.transactionId + " has failed: " + processorMessage
                        : resource_message;
                }
                else
                {
                    string resource_message = await GetPluginResourceAsync("Plugins.Payments.Bluefin.Error.Other");

                    err_message = string.IsNullOrEmpty(resource_message) ?
                        JsonConvert.SerializeObject(metadata)
                        : resource_message;
                }
                // TODO: Sort out if we proceed with the payment or block it on the spot with AddError
                processPaymentResult.AddError(err_message);
            }

            // Clean up the custom values that shouldn't be in the final order
            {
                CustomValues.Remove("bfTokenReference");
                CustomValues.Remove("savePaymentOption");
                CustomValues.Remove("bfTransactionId");
                CustomValues.Remove("paymentType");
            }
        }
        else
        {
            var transaction = new Transaction
            {
                TransactionId = bfTransactionId,
                Total = processPaymentRequest.OrderTotal.ToString(),
                Currency = currency.CurrencyCode,
                BfTokenReference = bfTokenReference,
                Description = productAttributes_string,
                CustomId = orderGuid,
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
                if (!string.IsNullOrEmpty(savePaymentOption)
                    && IsTokenVaulated(transaction_res))
                {
                    await _bluefinTokenRepositoryService.InsertAsync(
                        new BluefinTokenEntry
                        {
                            CustomerId = nop_customer.Id,
                            BfTokenReference = bfTokenReference
                        }
                    );
                }

                await _gateway.LogDebug(
                    "Triggered ProcessPaymentAsync bfTokenReference: " + bfTokenReference,
                    "Transaction Res Metadata: " + JsonConvert.SerializeObject(transaction_res.Metadata)
                    );

                processPaymentRequest.CustomValues.Add("Bluefin Transaction Identifier", transaction_res.Metadata.transactionId);

                // Reissuing Order. Consider only doing this with IsTokenVaulted() evaluating to true
                {
                    await _reissueOrdersRepositoryService.InsertAsync(
                        new ReissueOrderEntry
                        {
                            OrderGuid = orderGuid,
                            BfTokenReference = bfTokenReference
                        }
                    );
                }

                // processPaymentRequest.CustomValues.Add("Bluefin Transaction Status", transaction_res.metadata.status);

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

                dynamic metadata = transaction_res.Metadata;

                string err_message;

                // Note that the declined status is treated as a failed transaction (4xx response status) meaning we don't check whether the token was vauled for reuse for the Checkout Component (saved card)
                if (metadata.status == "DECLINED")
                {
                    string resource_message = await GetPluginResourceAsync("Plugins.Payments.Bluefin.Error.PaymentDeclined");
                    string processorMessage = string.IsNullOrEmpty((string)metadata.auth.processorMessage) ? "" : metadata.auth.processorMessage;

                    err_message = string.IsNullOrEmpty(resource_message) ?
                        "Transaction #" + metadata.transactionId + " has been declined: " + processorMessage
                        : resource_message;
                }
                else if (metadata.status == "FAILED")
                {
                    string resource_message = await GetPluginResourceAsync("Plugins.Payments.Bluefin.Error.PaymentFailed");
                    string processorMessage = string.IsNullOrEmpty((string)metadata.auth.processorMessage) ? "" : metadata.auth.processorMessage;

                    err_message = string.IsNullOrEmpty(resource_message) ?
                        "Transaction #" + metadata.transactionId + " has failed: " + processorMessage
                        : resource_message;
                }
                else
                {
                    string resource_message = await GetPluginResourceAsync("Plugins.Payments.Bluefin.Error.Other");

                    err_message = string.IsNullOrEmpty(resource_message) ?
                        JsonConvert.SerializeObject(metadata)
                        : resource_message;
                }
                // TODO: Sort out if we proceed with the payment or block it on the spot with AddError
                processPaymentResult.AddError(err_message);
            }

            // Clean up the custom values that shouldn't be in the final order
            {
                CustomValues.Remove("bfTokenReference");
                CustomValues.Remove("savePaymentOption");
                CustomValues.Remove("bfTransactionId");
                CustomValues.Remove("paymentType");
            }
        }

        return processPaymentResult;
    }
    
    public async Task<string> GetPluginResourceAsync(string resource_key)
    {
        var working_language = await _workContext.GetWorkingLanguageAsync();
        string resource_message = await _localizationService.GetResourceAsync(
                    resource_key,
                    working_language.Id,
                    false,
                    "",
                    true);

        return resource_message;

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

        await _gateway.LogDebug(
            "CaptureAsync CustomValuesXML",
            "CustomValuesXml:" + capturePaymentRequest.Order.CustomValuesXml
        );

        string bfTransactionId = Utility.ParseBfTransactionId(_gateway, capturePaymentRequest.Order.CustomValuesXml);

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
            // TODO: (metadata.status == "DECLINED") { "Transaction has been declined" }

            string err_message = "There has been an error while capturing the transaction: "
                            + new StringContent(JsonConvert.SerializeObject(transaction_res.Metadata));
            _notificationService.ErrorNotification(err_message);

            capturePaymentResult.AddError(err_message);

            // capturePaymentResult.NewPaymentStatus = PaymentStatus.Pending; // Note still Authorized or Pending in this case
        }

        return capturePaymentResult;
    }

    public async Task<RefundPaymentResult> RefundAsync(RefundPaymentRequest refundPaymentRequest)
    {
        var refundResult = new RefundPaymentResult();
        var amount = refundPaymentRequest.AmountToRefund.ToString("0.00");

        var orderGuid = refundPaymentRequest.Order.OrderGuid.ToString();

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

        string bfPaymentType = Utility.ParseBfPaymentType(_gateway, refundPaymentRequest.Order.CustomValuesXml);

        string bfTransactionId = Utility.ParseBfTransactionId(_gateway, refundPaymentRequest.Order.CustomValuesXml);

        if (bfPaymentType == "ACH")
        {
            var refund_transaction = new RefundTransaction
            {
                TransactionId = bfTransactionId,
                AmountToRefund = amount,
                Currency = refundPaymentRequest.Order.CustomerCurrencyCode,
                CustomId = orderGuid,
            };

            var refunded_res = await _gateway.ProcessACHRefund(refund_transaction);

            if (refunded_res.IsSuccess)
            {
                _notificationService.SuccessNotification("Refunded ACH Transaction #"
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
        }
        else
        {
            var refund_transaction = new RefundTransaction
            {
                TransactionId = bfTransactionId,
                AmountToRefund = amount,
                Currency = refundPaymentRequest.Order.CustomerCurrencyCode,
                CustomId = orderGuid,
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
        /*
        string bfTokenReference = CustomValues.ContainsKey("bfTokenReference") ? (string)CustomValues["bfTokenReference"] : "";
        string paymentType = CustomValues.ContainsKey("paymentType") ? (string)CustomValues["paymentType"] : "";

        string bfTransactionId = CustomValues.ContainsKey("bfTransactionId") ? (string)CustomValues["bfTransactionId"] : "";
        string savePaymentOption = CustomValues.ContainsKey("savePaymentOption") ? (string)CustomValues["savePaymentOption"] : "";
        */

        if (form == null)
            throw new ArgumentNullException(nameof(form));

        var paymentInfo = new ProcessPaymentRequest();

        if (form.TryGetValue("BfTokenReference", out StringValues BfTokenReference))
        {
            paymentInfo.CustomValues.Add("bfTokenReference", BfTokenReference[0]);
        }

        if (form.TryGetValue("BluefinPaymentType", out StringValues BluefinPaymentType))
        {
            paymentInfo.CustomValues.Add("paymentType", BluefinPaymentType[0]);
        }

        if (form.TryGetValue("BfTransactionId", out StringValues BfTransactionId))
        {
            paymentInfo.CustomValues.Add("bfTransactionId", BfTransactionId[0]);
        }

        if (form.TryGetValue("BluefinSavePaymentOption", out StringValues BluefinSavePaymentOption))
        {
            paymentInfo.CustomValues.Add("savePaymentOption", BluefinSavePaymentOption[0]);
        }

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
