using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;


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

using Nop.Web.Areas.Admin.Models.Logging;

using Nop.Web.Framework.Models.Extensions;

using Nop.Web.Framework.Models;

using Nop.Web.Framework.Models.DataTables;


using Nop.Web.Areas.Admin.Models.Orders;
using Nop.Web.Areas.Admin.Factories;


using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Orders;

using Nop.Core.Domain.Catalog;

using Microsoft.AspNetCore.Http;
using System.Dynamic;


using Nop.Services.Directory;

public partial record TraceLogsListModel : BasePagedListModel<TraceLogModel>
{
}

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
    protected readonly IOrderService _orderService;
    protected readonly IOrderModelFactory _orderModelFactory;
    private readonly IOrderProcessingService _orderProcessingService;
    private readonly IGenericAttributeService _genericAttributeService;

    private readonly BluefinPaymentSettings _bluefinPaymentSettings;
    private readonly IWorkContext _workContext;
    private readonly IStoreContext _storeContext;

    protected readonly IWebHelper _webHelper;
    protected readonly ICustomNumberFormatter _customNumberFormatter;

    private readonly BluefinGateway _gateway;

    private readonly BluefinTokenRepositoryService _bluefinTokenRepositoryService;
    private readonly TraceLogsRepositoryService _traceLogsRepositoryService;
    private readonly ReissueOrdersRepositoryService _reissueOrdersRepositoryService;

    private readonly IAddressService _addressService;


    private readonly IStateProvinceService _stateProvinceService;
    private readonly ICountryService _countryService;

    public PaymentBluefinController(ILogger logger,
        ISettingService settingService,
        ILocalizationService localizationService,
        INotificationService notificationService,
        IOrderService orderService,
        IOrderModelFactory orderModelFactory,
        IOrderProcessingService orderProcessingService,
        IGenericAttributeService genericAttributeService,
        BluefinPaymentSettings bluefinPaymentSettings,
        BluefinTokenRepositoryService bluefinTokenRepositoryService,
        ReissueOrdersRepositoryService reissueOrdersRepositoryService,
        TraceLogsRepositoryService traceLogsRepositoryService,
        IWorkContext workContext,
        IStoreContext storeContext,
        ICustomNumberFormatter customNumberFormatter,
        IAddressService addressService,
        IStateProvinceService stateProvinceService,
        ICountryService countryService,
        IWebHelper webHelper)
    {
        // _logger = logger;
        _localizationService = localizationService;
        _notificationService = notificationService;
        _orderService = orderService;
        _orderModelFactory = orderModelFactory;
        _settingService = settingService;
        _orderProcessingService = orderProcessingService;
        _storeContext = storeContext;
        _customNumberFormatter = customNumberFormatter;
        _bluefinPaymentSettings = bluefinPaymentSettings;
        _genericAttributeService = genericAttributeService;
        _bluefinTokenRepositoryService = bluefinTokenRepositoryService;
        _reissueOrdersRepositoryService = reissueOrdersRepositoryService;
        _traceLogsRepositoryService = traceLogsRepositoryService;
        _workContext = workContext;
        _addressService = addressService;
        _stateProvinceService = stateProvinceService;
        _countryService = countryService;
        _webHelper = webHelper;
        _gateway = new BluefinGateway(
            logger,
            _bluefinPaymentSettings,
            traceLogsRepositoryService
        );
    }

    [Area(AreaNames.ADMIN)]
    [CheckPermission(StandardPermission.Configuration.MANAGE_PAYMENT_METHODS)]
    public IActionResult TraceLogs() // async Task<IActionResult>
    {
        // var data = await _dbContext.BluefinEntries.ToListAsync();
        return View("~/Plugins/Payments.Bluefin/Views/TraceLogs.cshtml"); // , data);
    }

    [Area(AreaNames.ADMIN)]
    [CheckPermission(StandardPermission.System.MANAGE_SYSTEM_LOG)]
    public async Task<IActionResult> ViewTraceLog(int id)
    {
        var entry = await _traceLogsRepositoryService.GetById(id);

        var model = new TraceLogModel
        {
            Id = entry.Id,
            TraceId = entry.TraceId,
            ErrorMessage = entry.ErrorMessage,
            Json = entry.Json,
            Created = entry.Created
        };
        
        /*
        var log = await _logger.GetLogByIdAsync(id);
        if (log == null)
            return RedirectToAction("List");

        //prepare model
        var model = await _logModelFactory.PrepareLogModelAsync(null, log);
        */

        return View("~/Plugins/Payments.Bluefin/Views/ViewTraceLog.cshtml", model);
    }



    // [HttpPost]
    [Area(AreaNames.ADMIN)]
    [CheckPermission(StandardPermission.System.MANAGE_SYSTEM_LOG)]
    public async Task<IActionResult> TraceLogList(TraceSearchModel _searchModel)
    {

        // TODO: Refactor and optimize via query parameters for paged list with TraceSearchModel
        var _list = await _traceLogsRepositoryService.GetAllLogs();

        var __list = new List<TraceLogModel>();

        // TODO: Refactor to sort via query. This is inefficient
        var _sorted = new List<TraceIdEntry>(_list);

        // Descending
        _sorted.Sort((x,y) => y.Created.CompareTo(x.Created));

        foreach(var item in _sorted) {
            __list.Add(new TraceLogModel{
                Id = item.Id,
                TraceId = item.TraceId,
                ErrorMessage = item.ErrorMessage,
                Json = item.Json,
                Created = item.Created
            });
        }

        /*
        var _list = new List<TraceLogModel>
        {
             new TraceLogModel
            {
                Id = 0,
                TraceId = "c3abdb61-ca46-4cfc-8341-61ed69eee49a",
                ErrorMessage = "bfTokenReference is expired",
                Json = "{}"
            },
            new TraceLogModel
            {
                Id = 1,
                TraceId = "373fe421-bc7c-49c7-9a46-6d2d109fc3d8",
                ErrorMessage = "Request validation error: Does not conform to API schema.",
                Json = "{}"
            },
        };
        */


        // NOTE: Keep this commment if the functionality like this is needed down the line
        /*
        var searchModel = new TraceSearchModel
        {
            // TraceId = "A",
            // ErrorMessage = "B"
            Draw = "1"
        };
        */

        // Note: Page => (Start / Length) + 1; and PageSize => Length;
        var logItems = new PagedList<TraceLogModel>(__list, _searchModel.Page - 1, _searchModel.PageSize);

        // logItems.ToPagedList();

        var model = await new TraceLogsListModel().PrepareToGridAsync(_searchModel, logItems, () =>
        {
            //fill in model values from the entity
            return logItems.SelectAwait(async logItem =>
            {
                //fill in model values from the entity
                // var logModel = logItem.ToModel<TraceLogModel>();
                return logItem;
            });
        });

        /*
        return Json(new
        {
            Data = _sorted,
            draw = "1",
            recordsFiltered = _list.Count,
            recordsTotal = _list.Count,
            CustomProperties = new { }
        });
        */

        return Json(model);

        // return Json(new
        // {
        //    Data = _list
        // });

        // return Ok(new DataTablesModel { Data = _list });


    }


    [Area(AreaNames.ADMIN)]
    [CheckPermission(StandardPermission.Configuration.MANAGE_PAYMENT_METHODS)]
    public IActionResult ReissueOrders() // async Task<IActionResult>
    {
        // var data = await _dbContext.BluefinEntries.ToListAsync();
        return View("~/Plugins/Payments.Bluefin/Views/ReissueOrders.cshtml"); // , data);
    }

    [HttpPost]
    [Area(AreaNames.ADMIN)]
    [CheckPermission(StandardPermission.Orders.ORDERS_VIEW)]
    public async Task<IActionResult> OrderList(OrderSearchModel _searchModel)
    {
        //prepare model

        /*
        var searchModel = new OrderSearchModel{
            Draw = "1",
            // Page = _searchModel.Page,
        };
        */

        var model = await _orderModelFactory.PrepareOrderListModelAsync(_searchModel);

        return Json(model);
    }

    [Area(AreaNames.ADMIN)]
    [CheckPermission(StandardPermission.System.MANAGE_SYSTEM_LOG)]
    // [Route("ViewOrder/{order_temp_id}/{id}")]
    // [HttpGet("ViewOrder/{order_temp_id:long}/{id:int}")]
    public async Task<IActionResult> ViewOrder(int id) // long order_temp_id, int id
    {

        var order = await _orderService.GetOrderByIdAsync(id);

        // var model = await _orderModelFactory.PrepareOrderModelAsync(null, order);

        var model = new ReissueOrderModel
        {
            Id = order.Id,
            OrderStatus = order.OrderStatus,
            OrderGuid = order.OrderGuid,
            PaymentStatus = order.PaymentStatus,
            OrderTotal = order.OrderTotal,
        };

        // TODO: Use a different model to pass on the order_temp_id to be reused within a cshtml page via model.order_temp_id, for instance.

        /*
        var model = new TraceLogModel
        {
            Id = entry.Id,
            TraceId = entry.TraceId,
            ErrorMessage = entry.ErrorMessage,
            Json = entry.Json,
            Created = entry.Created
        };
        */

        /*
        var log = await _logger.GetLogByIdAsync(id);
        if (log == null)
            return RedirectToAction("List");

        //prepare model
        var model = await _logModelFactory.PrepareLogModelAsync(null, log);
        */

        return View("~/Plugins/Payments.Bluefin/Views/ViewOrder.cshtml", model);
    }

    public string parsePhoneNumber(string phone_number)
    {

        string output = "";
        foreach(char c in phone_number)
        {
            if(Char.IsDigit(c))
            {
                output += c;
            }

        }

        return output;

    }


    public async Task<Boolean> ProcessAndPlaceOrder(string BfTokenReference, ReissueOrderModel reissue_order_model) {

        var order = await _orderService.GetOrderByIdAsync(reissue_order_model.Id);

        var store = await _storeContext.GetCurrentStoreAsync();
        var customer_language = await _workContext.GetWorkingLanguageAsync();

        var processPaymentRequest = new ProcessPaymentRequest();
        var processPaymentResult = new ProcessPaymentResult();
        var currency = await _workContext.GetWorkingCurrencyAsync();


        decimal reissueOrderTotal = reissue_order_model.ReissueTotal;

        var new_order_guid = Guid.NewGuid();
        
        TransactionResponse transaction_res;

        // First try to process payment via Bluefin Payment Gateway
        {
            // TODO: Reuse from model.CustomValues once we can reissue ACH Payment
            processPaymentRequest.CustomValues.Add("Bluefin Payment Type", "CARD");

            var transaction = new TransactionMIT
            {
                TransactionId = "",
                Total = reissueOrderTotal.ToString("F2"),
                Currency = currency.CurrencyCode,
                BfTokenReference = BfTokenReference,
                Description = "",
                CustomId = new_order_guid.ToString(),
            };

            dynamic bluefin_customer = new ExpandoObject();


            {
                var billing_address = await _addressService.GetAddressByIdAsync(order.BillingAddressId);
                var stateProvince = await _stateProvinceService.GetStateProvinceByAddressAsync(billing_address);
                var country = await _countryService.GetCountryByAddressAsync(billing_address);

                bluefin_customer.customer = new ExpandoObject();

                if (!string.IsNullOrEmpty(billing_address.Email))
                {
                    bluefin_customer.customer.email = billing_address.Email;
                }

                if (!string.IsNullOrEmpty(billing_address.PhoneNumber))
                {
                    bluefin_customer.customer.phone = "+" + parsePhoneNumber(billing_address.PhoneNumber);
                }

                if (!string.IsNullOrEmpty(billing_address.FirstName) && !string.IsNullOrEmpty(billing_address.LastName))
                {
                    bluefin_customer.customer.name = billing_address.FirstName + " " + billing_address.LastName;
                }

                bluefin_customer.customer.billingAddress = new ExpandoObject();

                if (!string.IsNullOrEmpty(billing_address.Address1))
                {
                    bluefin_customer.customer.billingAddress.address1 = billing_address.Address1;
                }

                if (!string.IsNullOrEmpty(billing_address.Address2))
                {
                    bluefin_customer.customer.billingAddress.address2 = billing_address.Address2;

                }

                if(!string.IsNullOrEmpty(billing_address.City))
                {
                    bluefin_customer.customer.billingAddress.city = billing_address.City;
                }


                if (!string.IsNullOrEmpty(billing_address.ZipPostalCode))
                {
                    bluefin_customer.customer.billingAddress.zip = billing_address.ZipPostalCode;
                }

                if (!string.IsNullOrEmpty(billing_address.Company))
                {
                    bluefin_customer.customer.billingAddress.company = billing_address.Company;
                }

                if (!string.IsNullOrEmpty(stateProvince.Abbreviation))
                {
                    bluefin_customer.customer.billingAddress.state = stateProvince.Abbreviation;
                }

                if(!string.IsNullOrEmpty(country.ThreeLetterIsoCode))
                {
                    bluefin_customer.customer.billingAddress.country = country.ThreeLetterIsoCode;
                }


            }

            if (order.ShippingAddressId != null)
            {
                bluefin_customer.shippingAddress = new ExpandoObject();

                var shipping_address = await _addressService.GetAddressByIdAsync(order.ShippingAddressId ?? 0);
                var shipping_stateProvince = await _stateProvinceService.GetStateProvinceByAddressAsync(shipping_address);
                var shipping_country = await _countryService.GetCountryByAddressAsync(shipping_address);

                if(!string.IsNullOrEmpty(shipping_address.FirstName) && !string.IsNullOrEmpty(shipping_address.LastName))
                {
                    bluefin_customer.shippingAddress.recipient = shipping_address.FirstName + " " + shipping_address.LastName;
                }

                if (!string.IsNullOrEmpty(shipping_address.PhoneNumber))
                {
                    bluefin_customer.shippingAddress.recipientPhone = parsePhoneNumber(shipping_address.PhoneNumber);
                }

                if (!string.IsNullOrEmpty(shipping_address.Company))
                {
                    bluefin_customer.shippingAddress.company = shipping_address.Company;
                }

                if(!string.IsNullOrEmpty(shipping_country.ThreeLetterIsoCode))
                {
                    bluefin_customer.shippingAddress.country = shipping_country.ThreeLetterIsoCode;
                }

                if (!string.IsNullOrEmpty(shipping_address.ZipPostalCode))
                {
                    bluefin_customer.shippingAddress.zip = shipping_address.ZipPostalCode;
                }

                if(!string.IsNullOrEmpty(shipping_address.City))
                {
                    bluefin_customer.shippingAddress.city = shipping_address.City;
                }

                if(!string.IsNullOrEmpty(shipping_address.Address1))
                {
                    bluefin_customer.shippingAddress.address1 = shipping_address.Address1;
                }

                if(!string.IsNullOrEmpty(shipping_address.Address2))
                {
                    bluefin_customer.shippingAddress.address2 = shipping_address.Address2;
                }

                if (!string.IsNullOrEmpty(shipping_stateProvince.Abbreviation))
                {
                    bluefin_customer.shippingAddress.state = shipping_stateProvince.Abbreviation;
                }


            } else
            {
                bluefin_customer.shippingAddress = null;
            }

            /*
            new_order.BillingAddressId = order.BillingAddressId;

            if (order.ShippingAddressId != null)
            {
                new_order.ShippingAddressId = order.ShippingAddressId;
            }
            */


            transaction_res = await _gateway.ProcessMITSale(transaction, bluefin_customer);

            if (!transaction_res.IsSuccess)
            {
                dynamic metadata = transaction_res.Metadata;
                string err_message = JsonConvert.SerializeObject(metadata);
                _notificationService.ErrorNotification(err_message);
                return false;
            }

            processPaymentRequest.CustomValues.Add("Bluefin Transaction Identifier", transaction_res.Metadata.transactionId);

            processPaymentRequest.CustomValues.Add("Bluefin Transaction Initiator", "Merchant Initiated Transaction");
            
            processPaymentResult.NewPaymentStatus = PaymentStatus.Paid;

            
        }

        // processPaymentResult.NewPaymentStatus = PaymentStatus.Paid;

        // processPaymentResult.NewPaymentStatus = PaymentStatus.Pending;

        // processPaymentRequest.CustomValues.Add("Bluefin Transaction Identifier", "123456789010");




        // Place the order in nopCommerce db
        {

            // See: https://github.com/nopSolutions/nopCommerce/blob/release-4.80.9/src/Libraries/Nop.Services/Payments/PaymentService.cs#L372
            var ds = new DictionarySerializer(processPaymentRequest.CustomValues);
            var xs = new XmlSerializer(typeof(DictionarySerializer));
            using var textWriter = new StringWriter();
            using (var xmlWriter = XmlWriter.Create(textWriter))
            {
                xs.Serialize(xmlWriter, ds);
            }

            var CustomValuesXml = textWriter.ToString();

            // See: https://webiant.com/docs/nopcommerce/Libraries/Nop.Core/Domain/Orders/Order for the complete Order Entry Schema
            var new_order = new Order
            {
                StoreId = store.Id,
                OrderGuid = new_order_guid,
                CustomerId = order.CustomerId,
                CustomerLanguageId = customer_language.Id,
                // CustomerTaxDisplayType = order.CustomerTaxDisplayType,
                CustomerIp = _webHelper.GetCurrentIpAddress(),

                /*
                OrderSubtotalInclTax = order.OrderSubtotalInclTax,
                OrderSubtotalExclTax = order.OrderSubtotalExclTax,
                OrderSubTotalDiscountInclTax = order.OrderSubTotalDiscountInclTax,
                OrderSubTotalDiscountExclTax = order.OrderSubTotalDiscountExclTax,
                PaymentMethodAdditionalFeeInclTax = order.PaymentMethodAdditionalFeeInclTax,
                PaymentMethodAdditionalFeeExclTax = order.PaymentMethodAdditionalFeeExclTax,
                */

                // TaxRates = order.TaxRates,
                // OrderTax = order.OrderTax,

                // OrderShippingInclTax = order.OrderShippingInclTax,
                // OrderShippingExclTax = order.OrderShippingExclTax,
                OrderTotal = reissueOrderTotal,
                RefundedAmount = order.RefundedAmount,

                /*
                OrderDiscount = order.OrderDiscount,
                CheckoutAttributeDescription = order.CheckoutAttributeDescription,
                CheckoutAttributesXml = order.CheckoutAttributesXml,
                */

                CustomerCurrencyCode = order.CustomerCurrencyCode,
                CurrencyRate = order.CurrencyRate,
                // OrderStatus = order.OrderStatus,

                /*
                AffiliateId = order.AffiliateId,
                AllowStoringCreditCardNumber = order.AllowStoringCreditCardNumber,
                CardType = order.CardType,
                CardName = order.CardName,
                CardNumber = order.CardNumber,
                MaskedCreditCardNumber = order.MaskedCreditCardNumber,
                CardCvv2 = order.CardCvv2,
                CardExpirationMonth = order.CardExpirationMonth,
                CardExpirationYear = order.CardExpirationYear,
                */

                PaymentMethodSystemName = order.PaymentMethodSystemName,

                /*
                AuthorizationTransactionId = order.AuthorizationTransactionId,
                AuthorizationTransactionCode = order.AuthorizationTransactionCode,
                AuthorizationTransactionResult = order.AuthorizationTransactionResult,
                CaptureTransactionId = order.CaptureTransactionId,
                CaptureTransactionResult = order.CaptureTransactionResult,
                SubscriptionTransactionId = order.SubscriptionTransactionId,
                */

                PaymentStatus = processPaymentResult.NewPaymentStatus,
                PaidDateUtc = null,
                PickupInStore = order.PickupInStore,
                ShippingStatus = order.ShippingStatus,
                ShippingMethod = order.ShippingMethod,
                ShippingRateComputationMethodSystemName = order.ShippingRateComputationMethodSystemName,
                CustomValuesXml = CustomValuesXml,
                VatNumber = order.VatNumber,
                CreatedOnUtc = DateTime.UtcNow,
                CustomOrderNumber = string.Empty

            };

            new_order.OrderStatus = OrderStatus.Complete;

            // await _gateway.LogDebug("order.billingAddressId " + order.BillingAddressId.ToString() + (order.BillingAddressId == null).ToString(), "");
            // await _gateway.LogDebug("order.shippingAddressId " + order.ShippingAddressId.ToString() + (order.ShippingAddressId == null).ToString(), "");

            new_order.BillingAddressId = order.BillingAddressId;

            if (order.ShippingAddressId != null)
            {
                new_order.ShippingAddressId = order.ShippingAddressId;
            }

            if (order.PickupAddressId != null)
            {
                new_order.PickupAddressId = order.PickupAddressId;
            }

            new_order.PaidDateUtc = DateTime.UtcNow;

            await _orderService.InsertOrderAsync(new_order);


            // Order.Id can be used now (after the actual insert)
            new_order.CustomOrderNumber = _customNumberFormatter.GenerateOrderCustomNumber(new_order);
            await _orderService.UpdateOrderAsync(new_order);

            /*
            var orderItems = await _orderService.GetOrderItemsAsync(order.Id);

            foreach (var orderItem in orderItems)
            {
                var newOrderItem = new OrderItem
                {
                    OrderItemGuid = Guid.NewGuid(),
                    OrderId = new_order.Id,
                    ProductId = orderItem.ProductId,
                    UnitPriceInclTax = orderItem.UnitPriceInclTax,
                    UnitPriceExclTax = orderItem.UnitPriceExclTax,
                    PriceInclTax = orderItem.PriceInclTax,
                    PriceExclTax = orderItem.PriceExclTax,
                    OriginalProductCost = orderItem.OriginalProductCost,
                    AttributeDescription = orderItem.AttributeDescription,
                    AttributesXml = orderItem.AttributesXml,
                    Quantity = orderItem.Quantity,
                    DiscountAmountInclTax = orderItem.DiscountAmountInclTax,
                    DiscountAmountExclTax = orderItem.DiscountAmountExclTax,
                    DownloadCount = orderItem.DownloadCount,
                    IsDownloadActivated = orderItem.IsDownloadActivated,
                    LicenseDownloadId = orderItem.LicenseDownloadId,
                    ItemWeight = orderItem.ItemWeight,
                    RentalStartDateUtc = orderItem.RentalStartDateUtc,
                    RentalEndDateUtc = orderItem.RentalEndDateUtc
                };

                await _orderService.InsertOrderItemAsync(newOrderItem);
            }
            */

            // Add a note
            await _orderService.InsertOrderNoteAsync(new OrderNote
            {
                OrderId = new_order.Id,
                Note = "The order " + order.OrderGuid.ToString() + " has been reissued as order " + new_order.OrderGuid.ToString(),
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow
            });

            _notificationService.SuccessNotification(
                "Order successfully reissued as " + "orderId of " + new_order.Id.ToString() + " with " + "orderGuid of " + new_order_guid.ToString());

        }

        // Enable chain reissuing
        await _reissueOrdersRepositoryService.InsertAsync(
            new ReissueOrderEntry
            {
                OrderGuid = new_order_guid.ToString(),
                BfTokenReference = BfTokenReference,
            }
        );

        /*
        if (model.PickupAddress != null)
        {
            await _addressService.InsertAddressAsync(model.PickupAddress);
            order.PickupAddressId = model.PickupAddress.Id;
        }

        if (model.ShippingAddress != null)
        {
            await _addressService.InsertAddressAsync(model.ShippingAddress);
            order.ShippingAddressId = model.ShippingAddress.Id;
        }
        */


        return true;

    }

    #region Edit, delete

    // [HttpGet]
    [Area(AreaNames.ADMIN)]
    [CheckPermission(StandardPermission.Orders.ORDERS_VIEW)]
    public virtual async Task<IActionResult> EditOrder(int id)
    {
        var order = await _orderService.GetOrderByIdAsync(id);

        // var model = await _orderModelFactory.PrepareOrderModelAsync(null, order);
        var model = new ReissueOrderModel
        {
            Id = order.Id,
            OrderStatus = order.OrderStatus,
            OrderGuid = order.OrderGuid,
            PaymentStatus = order.PaymentStatus,
            OrderTotal = order.OrderTotal,
        };


        return View("~/Plugins/Payments.Bluefin/Views/ViewOrder.cshtml", model);
    }
    

    [Area(AreaNames.ADMIN)]
    [HttpPost, ActionName("EditOrder")]
    [FormValueRequired("reissueorder")]
    [CheckPermission(StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE)]
    public async Task<IActionResult> ReissueOrder(ReissueOrderModel order_model) { // IFormCollection form) { // (int id)

        // _notificationService.ErrorNotification("At least one payment method must be selected." + id.ToString());
        // _notificationService.SuccessNotification(); // await _localizationService.GetResourceAsync("Admin.Plugins.Saved")
        
        /*
        foreach (string key in form.Keys)
        {
            string value = form[key];
            await _gateway.LogDebug("form: key: " + key.ToString() + " value: " + value.ToString(), "");
        }
        */

        int id = order_model.Id;

        var order = await _orderService.GetOrderByIdAsync(id);

        // var model = await _orderModelFactory.PrepareOrderModelAsync(null, order);

        ReissueOrderEntry reissue_entry = await _reissueOrdersRepositoryService.GetBfTokenByOrderGuid(order.OrderGuid.ToString());

        var reissue_order_model = new ReissueOrderModel
        {
            Id = order.Id,
            OrderStatus = order.OrderStatus,
            OrderGuid = order.OrderGuid,
            PaymentStatus = order.PaymentStatus,
            OrderTotal = order.OrderTotal,
            ReissueTotal = order_model.ReissueTotal,
        };

        // await _gateway.LogDebug("reissue_order_model ReissueTotal " + order_model.ReissueTotal.ToString(), "");

        if (reissue_entry == null)
        {
            _notificationService.ErrorNotification("No Bluefin token assigned to the order. Order cannot be reissued.");
            return View("~/Plugins/Payments.Bluefin/Views/ViewOrder.cshtml", reissue_order_model);
        }

        string BfTokenReference = reissue_entry.BfTokenReference;

        var success = await ProcessAndPlaceOrder(BfTokenReference, order_model);


        // await _gateway.LogDebug("(order_model == null) " + (order_model == null).ToString(), "");

        // await _gateway.LogDebug("order_model Id " + order_model.Id, "");
        // await _gateway.LogDebug("order_model OrderStatus " + order_model.OrderStatus, "");
        // await _gateway.LogDebug("order_model PaymentStatus " + order_model.PaymentStatus, "");


        if (success)
        {
            // _notificationService.SuccessNotification(
            //    "Reissued order created with Bluefin token: " + BfTokenReference + " for guid: " + reissue_entry.OrderGuid);
        }
        else
        {
            _notificationService.ErrorNotification("Order reissue failed.");
        }

        return View("~/Plugins/Payments.Bluefin/Views/ViewOrder.cshtml", reissue_order_model);
    }
    #endregion


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
            IframeTimeout = bluefinPaymentSettings.IframeTimeout,

            EnableCard = bluefinPaymentSettings.EnableCard,
            EnableACH = bluefinPaymentSettings.EnableACH,
            EnableGooglePay = bluefinPaymentSettings.EnableGooglePay,
            EnableClickToPay = bluefinPaymentSettings.EnableClickToPay
        };

        // await _gateway.LogDebug("storeScope " + storeScope.ToString(), "");

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
            model.IframeTimeout_OverrideForStore = await _settingService.SettingExistsAsync(bluefinPaymentSettings, settings => settings.IframeTimeout, storeScope);

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

        if (!(model.EnableCard || model.EnableACH || model.EnableGooglePay || model.EnableClickToPay))
        {
            _notificationService.ErrorNotification("At least one payment method must be selected.");
            return await Configure();
        }

        if (!model.IframeResponsive)
        {
            if (string.IsNullOrEmpty(model.IframeWidth) || string.IsNullOrEmpty(model.IframeHeight))
            {
                _notificationService.ErrorNotification("Iframe Width and Height are required if not responsive");
                return await Configure();
            }
        }


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
        bluefinPaymentSettings.IframeTimeout = model.IframeTimeout;

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
        await _settingService.SaveSettingOverridablePerStoreAsync(bluefinPaymentSettings, settings => settings.IframeTimeout, model.IframeTimeout_OverrideForStore, storeScope, false);

        await _settingService.SaveSettingOverridablePerStoreAsync(bluefinPaymentSettings, settings => settings.EnableCard, model.EnableCard_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(bluefinPaymentSettings, settings => settings.EnableACH, model.EnableACH_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(bluefinPaymentSettings, settings => settings.EnableGooglePay, model.EnableGooglePay_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(bluefinPaymentSettings, settings => settings.EnableClickToPay, model.EnableClickToPay_OverrideForStore, storeScope, false);

        await _settingService.SaveSettingAsync(bluefinPaymentSettings, storeScope);

        // Note: Clear settings cache
        await _settingService.ClearCacheAsync();

        
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

        return Json(res);
    }

}