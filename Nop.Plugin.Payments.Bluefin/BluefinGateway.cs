using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Dynamic;

using Newtonsoft.Json;

using Nop.Core.Domain.Logging;
using Nop.Services.Logging;


namespace Nop.Plugin.Payments.Bluefin;

public class BluefinCustomer
{
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

    static public string ParseBfTransactionId(string CustomValuesXml) {

        XmlDocument doc = new XmlDocument();
        doc.LoadXml(CustomValuesXml);
        string jsonText = JsonConvert.SerializeXmlNode(doc);

        var definition = new { DictionarySerializer = new { item = new { key = "", value = "" } } };
        var jsonParsed = JsonConvert.DeserializeAnonymousType(jsonText, definition);

        string bfTransactionId = jsonParsed.DictionarySerializer.item.value;

        return bfTransactionId;
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

    public BluefinGateway(ILogger logger,
        BluefinPaymentSettings bluefinPaymentSettings) : base(logger, bluefinPaymentSettings.EnableLogging)
    {
        _bluefinPaymentSettings = bluefinPaymentSettings;
        _client = new HttpClient();

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

            if (!response.IsSuccessStatusCode)
            {

                await LogError(
                    source,
                    "Request: " + JsonConvert.SerializeObject(request) + ", " + "Response " + jsonString
                );

            }

            return JsonConvert.DeserializeObject<object>(jsonString);
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
                await LogError(
                    source,
                    "Request: " + JsonConvert.SerializeObject(request) + ", " + "Response " + jsonString
                );
                transaction_res.Metadata = JsonConvert.DeserializeObject<object>(jsonString);
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
        request.currency = customer.Currency;
        request.bfTokenReferences = bfTokenReferences;
        request.initializeTransaction = true;

        // Build allowedPaymentMethods from individual booleans
        var allowedPaymentMethods = bfTokenReferences, // new List<string>(), // NOTE: DISABLE FOR NOW bfTokenReferences,
        if (_bluefinPaymentSettings.EnableCard) allowedPaymentMethods.Add("CARD");
        if (_bluefinPaymentSettings.EnableACH) allowedPaymentMethods.Add("ACH");
        if (_bluefinPaymentSettings.EnableGooglePay) allowedPaymentMethods.Add("GOOGLE_PAY");
        if (_bluefinPaymentSettings.EnableClickToPay) allowedPaymentMethods.Add("CLICK_TO_PAY");
        request.allowedPaymentMethods = allowedPaymentMethods;

        request.customer = new
        {
            name = customer.BillingAddress.FirstName + " " + customer.BillingAddress.LastName,
            email = customer.Email,
            phone = customer.BillingAddress.PhoneNumber,
            billingAddress = new
            {
                address1 = customer.BillingAddress.Address1,
                address2 = customer.BillingAddress.Address2,
                city = customer.BillingAddress.City,
                state = customer.BillingAddress.State,
                zip = customer.BillingAddress.Zip,
                country = customer.BillingAddress.Country,
                company = customer.BillingAddress.Company
            }
        };
        request.shippingAddress = new
        {
            address1 = customer.ShippingAddress.Address1,
            address2 = customer.ShippingAddress.Address2,
            city = customer.ShippingAddress.City,
            state = customer.ShippingAddress.State,
            zip = customer.ShippingAddress.Zip,
            country = customer.ShippingAddress.Country,
            company = customer.ShippingAddress.Company,
            recipient = customer.ShippingAddress.FirstName + " " + customer.ShippingAddress.LastName,
            recipientPhone = customer.ShippingAddress.PhoneNumber
        };

        request.cardSettings = new
        {
            threeDSecure = "omit",
        };

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
            posProfile = "ECOMMERCE",
            amounts = new
            {
                total = transaction.Total,
                currency = transaction.Currency

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
            posProfile = "ECOMMERCE",
            amounts = new
            {
                total = transaction.Total,
                currency = transaction.Currency
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
}