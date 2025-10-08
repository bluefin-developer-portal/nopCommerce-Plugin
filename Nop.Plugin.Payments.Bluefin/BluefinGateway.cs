using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Dynamic;

using Newtonsoft.Json;

using Nop.Core.Domain.Logging;
using Nop.Services.Logging;

using Nop.Plugin.Payments.Bluefin.Domain;
using Nop.Plugin.Payments.Bluefin.Services;
using DocumentFormat.OpenXml.Drawing.Diagrams;


namespace Nop.Plugin.Payments.Bluefin;

public class BluefinCustomer
{
    public int? Timeout { get; set; }
    public int CustomerId { get; set; }
    public string Email { get; set; }
    public string Amount { get; set; } // TODO: Refactor to an Order class or similar
    public string Currency { get; set; }
    public BillingAddress BillingAddress { get; set; }
    public ShippingAddress ShippingAddress { get; set; }
}

public class BillingAddress
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string City { get; set; }
    public string Company { get; set; }
    public string Country { get; set; }
    public string PhoneNumber { get; set; }
    public string Zip { get; set; }
    public string State { get; set; }
    public string Address1 { get; set; }
    public string Address2 { get; set; }
}

public class ShippingAddress
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string City { get; set; }
    public string Company { get; set; }
    public string Country { get; set; }
    public string PhoneNumber { get; set; }
    public string Zip { get; set; }
    public string State { get; set; }
    public string Address1 { get; set; }
    public string Address2 { get; set; }
}

public class Transaction
{
    public string TransactionId { get; set; }
    public string Total { get; set; }
    public string Currency { get; set; }
    public string BfTokenReference { get; set; }

    public string Description { get; set; }
}

public class TransactionResponse
{
    public dynamic Metadata { get; set; }
    public bool IsSuccess { get; set; }
}

public class RefundTransaction
{
    public string TransactionId { get; set; }
    public string AmountToRefund { get; set; }

    public string Currency { get; set; }
}

public class Utility
{

    static public string ToHex(byte[] bytes)
    {
        StringBuilder builder = new StringBuilder();
        for (int i = 0; i < bytes.Length; i++)
        {
            builder.Append(bytes[i].ToString("x2")); // "x2" for lowercase hexadecimal
        }
        return builder.ToString();
    }

    static public string GenerateHMACHeader(string path, string body, string api_key_id, string secret)
    {
        string method = "POST"; // NOTE: POST by default for all now until we add other HTTP methods

        if (method == "GET")
        {
            body = "";
        }

        string nonce = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");

        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();

        using var sha256 = SHA256.Create();

        byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(body));

        string payloadHash = ToHex(hashedBytes);

        string canonical_request = method + ' ' + path + '\n'
            + nonce + '\n'
            + timestamp + "\n\n"
            + payloadHash;


        using var hmacsha256 = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var bytes = hmacsha256.ComputeHash(Encoding.UTF8.GetBytes(canonical_request));

        var digest = ToHex(bytes);

        return ("Hmac "
            + "id=\"" + api_key_id + "\""
            + ", nonce=\"" + nonce + "\""
            + ", timestamp=\"" + timestamp + "\""
            + ", response=\"" + digest + "\"");
    }

    static public string ParseBfTransactionId(BluefinGateway gateway, string CustomValuesXml)
    {

        XmlDocument doc = new XmlDocument();
        doc.LoadXml(CustomValuesXml);
        string jsonText = JsonConvert.SerializeXmlNode(doc);

        /*
        gateway.LogDebug(
            "ParseBfTransactionId",
            "jsonText:" + jsonText
        ).Wait();
        */

        string bfTransactionId = "";

        try
        {
            var definition = new
            {
                DictionarySerializer = new
                {
                    item = new List<Dictionary<string, string>>()
                }
            };

            var jsonParsed = JsonConvert.DeserializeAnonymousType(jsonText, definition);

            int bftrans_inx = jsonParsed.DictionarySerializer.item.FindIndex(pair =>
                    pair["key"] == "Bluefin Transaction Identifier");

            if (bftrans_inx != -1)
            {
                bfTransactionId = jsonParsed.DictionarySerializer.item[bftrans_inx]["value"];
            }

        }
        catch (JsonSerializationException) // NOTE: Ensure Backwards compatibility with the old version that was used to make payments.
        {
            var definition = new { DictionarySerializer = new { item = new { key = "", value = "" } } };
            var jsonParsed = JsonConvert.DeserializeAnonymousType(jsonText, definition);

            bfTransactionId = jsonParsed.DictionarySerializer.item.value;
        }

        return bfTransactionId;
    }

    static public string ParseBfPaymentType(BluefinGateway gateway, string CustomValuesXml)
    {

        XmlDocument doc = new XmlDocument();
        doc.LoadXml(CustomValuesXml);
        string jsonText = JsonConvert.SerializeXmlNode(doc);

        /*
        gateway.LogDebug(
            "ParseBfTransactionId",
            "jsonText:" + jsonText
        ).Wait();
        */

        string bfPaymentType= "";

        try
        {
            var definition = new
            {
                DictionarySerializer = new
                {
                    item = new List<Dictionary<string, string>>()
                }
            };

            var jsonParsed = JsonConvert.DeserializeAnonymousType(jsonText, definition);

            int bfpaymenttype_inx = jsonParsed.DictionarySerializer.item.FindIndex(pair =>
                    pair["key"] == "Bluefin Payment Type");

            if (bfpaymenttype_inx != -1)
            {
                bfPaymentType = jsonParsed.DictionarySerializer.item[bfpaymenttype_inx]["value"];
            }

        }
        catch (JsonSerializationException) // NOTE: Ensure Backwards compatibility with the old version that was used to make payments.
        {
            var definition = new { DictionarySerializer = new { item = new { key = "", value = "" } } };
            var jsonParsed = JsonConvert.DeserializeAnonymousType(jsonText, definition);

            bfPaymentType = jsonParsed.DictionarySerializer.item.value;
        }

        return bfPaymentType;
    }

}

public class BluefinLogger(ILogger logger, bool enabled)
{
    private readonly ILogger _logger = logger;
    private readonly bool _enabled = enabled;

    public async Task LogError(string source, string message)
    {
        if (_enabled)
        {
            await _logger.InsertLogAsync(
                LogLevel.Error,
                source,
                message
            );
        }

    }

    public async Task LogInfo(string source, string message)
    {
        if (_enabled)
        {
            await _logger.InsertLogAsync(
                LogLevel.Information,
                source,
                message
            );
        }
    }

    public async Task LogDebug(string source, string message)
    {
        if (_enabled)
        {
            await _logger.InsertLogAsync(
                LogLevel.Debug,
                source,
                message
            );
        }
    }

}

public class BluefinGateway : BluefinLogger
{
    private readonly BluefinPaymentSettings _bluefinPaymentSettings;
    private readonly HttpClient _client;
    private readonly string _baseEnvURL;

    private readonly TraceLogsRepositoryService _traceLogsRepositoryService;

    public BluefinGateway(ILogger logger,
        BluefinPaymentSettings bluefinPaymentSettings,
        TraceLogsRepositoryService traceLogsRepositoryService
        ) : base(logger, bluefinPaymentSettings.EnableLogging)
    {
        _bluefinPaymentSettings = bluefinPaymentSettings;
        _client = new HttpClient();
        _traceLogsRepositoryService = traceLogsRepositoryService;

        if (_bluefinPaymentSettings.UseSandbox)
        {
            string AuthHeader = System.Convert.ToBase64String(
                System.Text.Encoding.UTF8.GetBytes(_bluefinPaymentSettings.ApiKeyId + ':' + _bluefinPaymentSettings.ApiKeySecret)
                );
            _client.DefaultRequestHeaders.Authorization
                 = new AuthenticationHeaderValue("Basic", AuthHeader);

            _baseEnvURL = BluefinPaymentDefaults.certEnv;
        }
        else
        {
            _baseEnvURL = BluefinPaymentDefaults.prodEnv;
        }
    }

    public void InjectHeaders(HttpRequestMessage requestMessage, string URI, string serializedBody)
    {
        if (!_bluefinPaymentSettings.UseSandbox) // USE HMAC IN PROD
        {
            string hmac_auth = Utility.GenerateHMACHeader(URI, serializedBody, _bluefinPaymentSettings.ApiKeyId, _bluefinPaymentSettings.ApiKeySecret);

            // await _logger.InsertLogAsync(
            //    LogLevel.Error,
            //    "HMAC",
            //    hmac_auth
            // );

            requestMessage.Headers.Add("Authorization",
                hmac_auth
            );
        }

        return;
    }

    public async Task<object> HandleRequest(HttpRequestMessage requestMessage, object request, string source)
    {

        var response = await _client.SendAsync(requestMessage);

        if (response != null)
        {
            var jsonString = await response.Content.ReadAsStringAsync();

            dynamic output = JsonConvert.DeserializeObject<object>(jsonString);

            if (!response.IsSuccessStatusCode)
            {

                await LogError(
                    source,
                    "Request: " + JsonConvert.SerializeObject(request) + ", " + "Response " + jsonString
                );

                await _traceLogsRepositoryService.InsertAsync(
                    new TraceIdEntry
                    {
                        TraceId = output.traceId,
                        ErrorMessage = output.message,
                        Json = jsonString,
                        Created = DateTime.Now
                    }
                );

            }

            return output;
        }

        return null;

    }

    public async Task<TransactionResponse> HandleTransactionRequest(HttpRequestMessage requestMessage, object request, string source)
    {

        var response = await _client.SendAsync(requestMessage);
        var transaction_res = new TransactionResponse();

        if (response != null)
        {
            var jsonString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                dynamic output = JsonConvert.DeserializeObject<object>(jsonString);

                await LogError(
                    source,
                    "Request: " + JsonConvert.SerializeObject(request) + ", " + "Response " + jsonString
                );

                await _traceLogsRepositoryService.InsertAsync(
                    new TraceIdEntry
                    {
                        TraceId = output.traceId,
                        ErrorMessage = output.message,
                        Json = jsonString,
                        Created = DateTime.Now
                    }
                );

                transaction_res.Metadata = output;
                transaction_res.IsSuccess = false;
                return transaction_res;
            }

            transaction_res.Metadata = JsonConvert.DeserializeObject<object>(jsonString);
            transaction_res.IsSuccess = true;

            return transaction_res;
        }

        return transaction_res;

    }

    public async Task<object> IframeInit(BluefinCustomer customer, List<string> bfTokenReferences)
    {
        string accountId = _bluefinPaymentSettings.AccountId;

        string URI = "/api/v4/accounts/" +
                    accountId + "/payment-iframe/" + _bluefinPaymentSettings.IFrameConfigId + "/instance/init";

        // NOTE: dynamic object to build flexible JSON request with conditional properties,
        // With a strongly-typed object, I couldn't add properties conditionally at runtime using Visual Studio.
        dynamic request = new ExpandoObject();

        request.label = "my-instance-1"; // TODO: Compose based on the Nop Customer Info?
        request.amount = customer.Amount;
        request.timeout = customer.Timeout;
        request.currency = customer.Currency;
        request.bfTokenReferences = bfTokenReferences;
        request.initializeTransaction = true;

        // Build allowedPaymentMethods from individual booleans
        var allowedPaymentMethods = new List<string>(); // NOTE: DISABLE FOR NOW bfTokenReferences,
        if (_bluefinPaymentSettings.EnableCard)
            allowedPaymentMethods.Add("CARD");
        if (_bluefinPaymentSettings.EnableACH)
            allowedPaymentMethods.Add("ACH");
        if (_bluefinPaymentSettings.EnableGooglePay)
            allowedPaymentMethods.Add("GOOGLE_PAY");
        if (_bluefinPaymentSettings.EnableClickToPay)
            allowedPaymentMethods.Add("CLICK_TO_PAY");
        request.allowedPaymentMethods = allowedPaymentMethods;

        request.customer = new ExpandoObject();

        // NOTE: Billing Address
        request.customer.billingAddress = new ExpandoObject();


        if (customer.Email != null)
        {
            request.customer.email = customer.Email;
        }

        if (customer.BillingAddress != null)
        {
            if (customer.BillingAddress.FirstName != null && customer.BillingAddress.LastName != null)
            {
                request.customer.name = customer.BillingAddress.FirstName + " " + customer.BillingAddress.LastName;
            }
            else if (customer.BillingAddress.FirstName != null)
            {
                request.customer.name = customer.BillingAddress.FirstName;
            }

            if (customer.BillingAddress.PhoneNumber != null)
            {
                request.customer.phone = customer.BillingAddress.PhoneNumber;
            }

            if (customer.BillingAddress.Address1 != null)
            {
                request.customer.billingAddress.address1 = customer.BillingAddress.Address1;
            }

            if (customer.BillingAddress.Address2 != null)
            {
                request.customer.billingAddress.address2 = customer.BillingAddress.Address2;
            }

            if (customer.BillingAddress.City != null)
            {
                request.customer.billingAddress.city = customer.BillingAddress.City;
            }

            if (customer.BillingAddress.State != null)
            {
                request.customer.billingAddress.state = customer.BillingAddress.State;
            }

            if (customer.BillingAddress.Zip != null)
            {
                request.customer.billingAddress.zip = customer.BillingAddress.Zip;
            }
            if (customer.BillingAddress.Country != null)
            {
                request.customer.billingAddress.country = customer.BillingAddress.Country;
            }

            if (customer.BillingAddress.Company != null)
            {
                request.customer.billingAddress.company = customer.BillingAddress.Company;
            }


        }


        if (customer.ShippingAddress != null)
        {
            // NOTE: Shipping Address including Pickup
            request.shippingAddress = new ExpandoObject();

            if (customer.ShippingAddress.FirstName != null && customer.ShippingAddress.LastName != null)
            {
                request.shippingAddress.recipient = customer.ShippingAddress.FirstName + " " + customer.ShippingAddress.LastName;
            }
            else if (customer.ShippingAddress.FirstName != null)
            {
                request.shippingAddress.recipient = customer.ShippingAddress.FirstName;
            }

            if (customer.ShippingAddress.Address1 != null)
            {
                request.shippingAddress.address1 = customer.ShippingAddress.Address1;
            }

            if (customer.ShippingAddress.Address2 != null)
            {
                request.shippingAddress.address2 = customer.ShippingAddress.Address2;
            }

            if (customer.ShippingAddress.City != null)
            {
                request.shippingAddress.city = customer.ShippingAddress.City;
            }

            if (customer.ShippingAddress.State != null)
            {
                request.shippingAddress.state = customer.ShippingAddress.State;
            }

            if (customer.ShippingAddress.Zip != null)
            {
                request.shippingAddress.zip = customer.ShippingAddress.Zip;
            }

            if (customer.ShippingAddress.Country != null)
            {
                request.shippingAddress.country = customer.ShippingAddress.Country;
            }

            if (customer.ShippingAddress.Company != null)
            {
                request.shippingAddress.company = customer.ShippingAddress.Company;
            }

            if (customer.ShippingAddress.PhoneNumber != null)
            {
                request.shippingAddress.recipientPhone = customer.ShippingAddress.PhoneNumber;
            }

        }

        if (_bluefinPaymentSettings.Use3DS)
        {
            request.threeDSecureInitSettings = new
            {
                transactionType = _bluefinPaymentSettings.ThreeDTransType,
                deliveryTimeFrame = _bluefinPaymentSettings.DeliveryTimeFrame,
                threeDSecureChallengeIndicator = _bluefinPaymentSettings.ThreeDSecureChallengeIndicator,
                reorderIndicator = _bluefinPaymentSettings.ReorderIndicator,
                shippingIndicator = _bluefinPaymentSettings.ShippingIndicator
            };
        }


        HttpRequestMessage requestMessage = null;

        var serializedBody = JsonConvert.SerializeObject(request);

        requestMessage = new HttpRequestMessage(HttpMethod.Post, _baseEnvURL + URI)
        {
            Content = new StringContent(serializedBody, Encoding.UTF8, "application/json")
        };

        InjectHeaders(requestMessage, URI, serializedBody);

        return await HandleRequest(requestMessage, request, "BluefinGateway.iframeInit ERROR");
    }

    // See: https://developers.bluefin.com/payconex/v4/reference/customer-and-merchant-initiated-transactions-1
    public dynamic MakeCITParameters()
    {
        return new
        {
            credentialOnFile = new
            {
                transactionInitiator = "CUSTOMER",
                storedCredentialIndicator = "INITIAL",
                scheduleIndicator = "UNSCHEDULED"
            }
        };
    }


    public async Task<TransactionResponse> CaptureAuthorization(string transactionId)
    {
        string accountId = _bluefinPaymentSettings.AccountId;

        string URI = "/api/v4/accounts/" +
                    accountId + "/payments/" + transactionId + "/capture";

        var request = new
        {
            posProfile = "ECOMMERCE"
        };

        HttpRequestMessage requestMessage = null;

        var serializedBody = JsonConvert.SerializeObject(request);

        requestMessage = new HttpRequestMessage(HttpMethod.Post, _baseEnvURL + URI)
        {
            Content = new StringContent(serializedBody, Encoding.UTF8, "application/json")
        };

        InjectHeaders(requestMessage, URI, serializedBody);

        return await HandleTransactionRequest(requestMessage, request, "BluefinGateway.captureAuthorization ERROR");

    }


    public async Task<TransactionResponse> ProcessRefund(RefundTransaction transaction)
    {
        string accountId = _bluefinPaymentSettings.AccountId;

        string URI = "/api/v4/accounts/" +
                    accountId + "/payments/" + transaction.TransactionId + "/refund";

        var request = new
        {
            posProfile = "ECOMMERCE",
            amounts = new
            {
                total = transaction.AmountToRefund,
                currency = transaction.Currency
            },
            trace = new
            {
                source = "nopCommerce Plugin"
            }
        };

        HttpRequestMessage requestMessage = null;

        var serializedBody = JsonConvert.SerializeObject(request);

        requestMessage = new HttpRequestMessage(HttpMethod.Post, _baseEnvURL + URI)
        {
            Content = new StringContent(serializedBody, Encoding.UTF8, "application/json")
        };

        InjectHeaders(requestMessage, URI, serializedBody);

        return await HandleTransactionRequest(requestMessage, request, "BluefinGateway.processRefund ERROR");

    }

    public async Task<TransactionResponse> ProcessAuthorization(Transaction transaction)
    {
        string accountId = _bluefinPaymentSettings.AccountId;

        string URI = "/api/v4/accounts/" +
                    accountId + "/payments/auth";
        var request = new
        {
            transactionId = transaction.TransactionId,
            description = string.IsNullOrEmpty(transaction.Description) ? "" : transaction.Description,
            posProfile = "ECOMMERCE",
            amounts = new
            {
                total = transaction.Total,
                currency = transaction.Currency

            },
            trace = new
            {
                source = "nopCommerce Plugin"
            },
            bfTokenReference = transaction.BfTokenReference,
            credentialOnFile = MakeCITParameters().credentialOnFile
        };

        HttpRequestMessage requestMessage = null;

        var serializedBody = JsonConvert.SerializeObject(request);

        requestMessage = new HttpRequestMessage(HttpMethod.Post, _baseEnvURL + URI)
        {
            Content = new StringContent(serializedBody, Encoding.UTF8, "application/json")
        };

        InjectHeaders(requestMessage, URI, serializedBody);

        return await HandleTransactionRequest(requestMessage, request, "BluefinGateway.processAuthorization ERROR");

    }

    public async Task<TransactionResponse> ProcessSale(Transaction transaction)
    {
        string accountId = _bluefinPaymentSettings.AccountId;

        string URI = "/api/v4/accounts/" +
                    accountId + "/payments/sale";
        var request = new
        {
            transactionId = transaction.TransactionId,
            description = string.IsNullOrEmpty(transaction.Description) ? "" : transaction.Description,
            posProfile = "ECOMMERCE",
            amounts = new
            {
                total = transaction.Total,
                currency = transaction.Currency
            },
            trace = new
            {
                source = "nopCommerce Plugin"
            },
            bfTokenReference = transaction.BfTokenReference,
            credentialOnFile = MakeCITParameters().credentialOnFile
        };

        HttpRequestMessage requestMessage = null;

        var serializedBody = JsonConvert.SerializeObject(request);

        requestMessage = new HttpRequestMessage(HttpMethod.Post, _baseEnvURL + URI)
        {
            Content = new StringContent(serializedBody, Encoding.UTF8, "application/json")
        };

        InjectHeaders(requestMessage, URI, serializedBody);

        return await HandleTransactionRequest(requestMessage, request, "BluefinGateway.processSale ERROR");

    }

    public async Task<TransactionResponse> ProcessACHSale(Transaction transaction)
    {
        string accountId = _bluefinPaymentSettings.AccountId;

        string URI = "/api/v4/accounts/" +
                    accountId + "/ach/sale";
        var request = new
        {
            transactionId = transaction.TransactionId,
            description = string.IsNullOrEmpty(transaction.Description) ? "" : transaction.Description,
            amounts = new
            {
                total = transaction.Total,
                currency = transaction.Currency
            },
            trace = new
            {
                source = "nopCommerce Plugin"
            },
            bfTokenReference = transaction.BfTokenReference
        };

        HttpRequestMessage requestMessage = null;

        var serializedBody = JsonConvert.SerializeObject(request);

        requestMessage = new HttpRequestMessage(HttpMethod.Post, _baseEnvURL + URI)
        {
            Content = new StringContent(serializedBody, Encoding.UTF8, "application/json")
        };

        InjectHeaders(requestMessage, URI, serializedBody);

        return await HandleTransactionRequest(requestMessage, request, "BluefinGateway.processSale ERROR");

    }

    public async Task<TransactionResponse> ProcessACHRefund(RefundTransaction transaction)
    {
        string accountId = _bluefinPaymentSettings.AccountId;

        string URI = "/api/v4/accounts/" +
                    accountId + "/ach/" + transaction.TransactionId + "/refund";

        var request = new
        {
            amounts = new
            {
                total = transaction.AmountToRefund,
                currency = transaction.Currency
            },
            trace = new
            {
                source = "nopCommerce Plugin"
            }
        };

        HttpRequestMessage requestMessage = null;

        var serializedBody = JsonConvert.SerializeObject(request);

        requestMessage = new HttpRequestMessage(HttpMethod.Post, _baseEnvURL + URI)
        {
            Content = new StringContent(serializedBody, Encoding.UTF8, "application/json")
        };

        InjectHeaders(requestMessage, URI, serializedBody);

        return await HandleTransactionRequest(requestMessage, request, "BluefinGateway.processRefund ERROR");

    }
    

}