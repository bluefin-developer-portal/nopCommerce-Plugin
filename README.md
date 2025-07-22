# nopCommerce Plugin for Bluefin



## Overview

This nopCommerce plugin combines the Bluefin Checkout Component and REST API, constituting the complete ready-to-use Bluefin payment method for nopCommerce platform.

The checkout component supports Card Payment, Google Pay, Mastercard Click to Pay, proving an all-in comprehensive eCommerce payment solution. 

It requires the merchant integration with the Bluefin Gateway where the integration team sets up your configuration according to your needs. The merchant is free to customize their iframe configuration and configure their payment method options on their own as they have gained enough experience while certifying with their Bluefin integration.

The plugin is built upon the Bluefin® PayConex™ REST API that connects to various PayConex™ services, thus serving as an HTTPS communication bridge to the PayConex™ Gateway.



Here are some of the key components that the Bluefin payment plugin offers to the merchant.

> 📘 Note
>
> The merchant using this plugin is _not_ required to understand much of what's happening behind the scenes and how the Bluefin APIs are used.
>
> If you are interested in all the ins and outs, check out our [Comprehensive Documentation and Reference Materials](https://developers.bluefin.com/payconex/v4/reference/payconex-introduction).



### **Bluefin Hosted Checkout Components**

- **Easy Integration:** Use our secure, pre-built Checkout Component UI via our SDK, designed for seamless integration into your existing systems.
- **Security:** These components are hosted on Bluefin's servers and handle all payment data input through an HTML iframe, ensuring that no sensitive credit card data reaches your servers.
- **Flexible Management and Configuration**: With a set of API endpoints, you can easily configure and create iframe payment instances, and effectively overwrite the configuration for a specific instance per customer. For more, see [Creating an Instance](https://developers.bluefin.com/payconex/v4/reference/creating-an-instance).
- **Tokenization:** Once the form is completed, it securely tokenizes the information for CNP transactions by communicating with the ShieldConex® tokenization service and utilizes a payment authentication service based on the type of payment method, e.g. 3DS(Credit or Debit Card), Google Account(Google Pay), ACH(Bank Information), Mastercard Click to Pay. After tokenization, a transaction is supposed to be processed during the PayConex™ token life-span (within 10 minutes).

### **Versatile Transaction Processing**

- **Security:** Bluefin ShieldConex® ensures that no sensitive card information is ever stored on your servers, significantly reducing the PCI scope.
- **Card Not Present Transactions:** Before processing, CNP transactions primarily rely on ShieldConex® for security. **ShieldConex®** does not store any sensitive cardholder data. Instead, it uses tokenization/detokenization on its vaultless tokens for online PII, PHI, payments and ACH account data. These tokens can be securely utilized or stored on the merchant's server, significantly reducing the vendor's or merchant's PCI footprint. However, if the merchant loses these tokens, they *cannot* be recovered.
- **Transaction Types**: Our gateway supports a variety of the most common transaction types used on a day-to-day basis such as sale, authorization, store, capture, refund and credit.

### **3DS Support**

- **Security Backbone:** Besides the vaultless tokenization solution by ShieldConex®, Bluefin provides one of the security backbones for processing online CNP transactions, with iframe configurations that can fully integrate 3DS as a feature of PayConex™.
- **Fraud Prevention:** Implement 3DS to enhance fraud prevention and secure customer authentication.
- **Fraud Scoring:** Iframe configurations allow for anti-fraud service for extra authorization based on the score during transaction processing. Fraud Scoring is available through all our processors and help merchants score transactions based on rules. However, this is a feature on its own and it does *not* need to be used with 3D Secure.
- **User Experience:** Ensure a smooth user experience while maintaining high security standards.
- **3DS MPI Simulation**: Bluefin 3DS Solution can be simulated in the certification environment for testing purposes.



## nopCommerce Plugin Information

Being a payment method plugin, here are the CNP transaction types that it supports.

| Friendly name | System name      | Supports capture | Refund | Partial refund | Void | Recurring support |
| :------------ | :--------------- | :--------------- | :----- | :------------- | ---- | :---------------- |
| Bluefin       | Payments.Bluefin | ✔                | ✔      | ✔              | ✗    | Not supported     |





## Installing the Plugin







## Setting up and Configuring the Plugin

The plugin supports being on multiple stores of nopCommerce, for more see: https://docs.nopcommerce.com/en/getting-started/advanced-configuration/multi-store.html.

 

Once the plugin is installed and is active, the merchant is required to go to the [Admin Area](https://docs.nopcommerce.com/en/getting-started/admin-area-overview.html) -> Configuration -> Payment methods and configure the plugin.



>📘 Note
>
>Once the merchant is integrated with Bluefin Payment Gateway, the Bluefin Integrations Team provides them with the recommended nopCommerce configuration according to their needs.
>
>For more details on configuring the Bluefin plugin, make sure to check out below and be up-to-date with the [Bluefin Checkout Component Documentation](https://developers.bluefin.com/payconex/v4/reference/creating-a-configuration).



There, we can see the Configure Page that consists of the following options.

- **API Key Identifier (Required)**

  - The secret API key ID, used in conjunction with the secret for API authentication. For more information, see [API Key Management](https://developers.bluefin.com/payconex/v4/reference/api-key-management-1).

- **API Key Secret (Required)**

  - The API key secret, used in conjunction with the API key ID for API authentication. For more information, see [API Key Management](https://developers.bluefin.com/payconex/v4/reference/api-key-management-1).

    > **Note**
    > Keep this secret secure and never expose it in your client-side code.
    > API Key Identifier and Secret are both used to generate either [Basic Authentication](https://developers.bluefin.com/payconex/v4/reference/api-authentication#basic-authentication) or [HMAC Authentication](https://developers.bluefin.com/payconex/v4/reference/api-authentication#hmac-authentication) depending on the environment the merchant integrates with.

- **Account Identifier (Required)**

  - The primary account ID associated with the API key.

- **iFrame Configuration Identifier (Required)**

  - iFrame Configuration used by the Checkout Component. Preconfigure payment methods and their settings

- **Use sandbox environment**

  - This check box must be selected if the merchant is in the process of conducting thorough testing in the Bluefin **certification** environment.
  - This also includes
    - Simulating various transaction scenarios to ensure reliability and security.
    - Performing end-to-end testing, from [Checkout Component](https://developers.bluefin.com/payconex/v4/reference/integration-steps#5-checkout-component-integration) rendering to transaction completion.
  - For going to **Production**, this option must be checked out.
  - Note that in the certification environment, the plugin uses the [Base64 Basic Authentication](https://developers.bluefin.com/payconex/v4/reference/api-authentication#basic-authentication) whereas for production, it automatically generates the [HMAC Authentication](https://developers.bluefin.com/payconex/v4/reference/api-authentication#hmac-authentication).

- **Use 3D Secure**

  - Use 3D Secure for the Checkout Component, Card Payment Method.
  - With this setting enabled, the [`threeDSecureInitSettings`](#threedsecureinitsettings) must be configured according to your needs. Note that this setting is required if `cardSettings.threeDSecure` is defined as `"required"`. For more information, check out [PayConex™ API | Creating an Instance](https://developers.bluefin.com/payconex/v4/reference/creating-an-instance#api-request).
  - While in the certification environment, make sure to use the following [3DS Test Card Numbers](https://developers.bluefin.com/payconex/v4/reference/bluefin-3ds-support#test-card-numbers) where you can simulate real-life scenarios to gain a better understanding of how 3D Secure operates with our Checkout Component and transaction authorization.
  - For the comprehensive information on Bluefin 3D Secure Protocol, check out [3D Secure](https://developers.bluefin.com/payconex/docs/3d-secure) and [Bluefin 3DS Support](https://developers.bluefin.com/payconex/v4/reference/bluefin-3ds-support).



#### Iframe Settings



#### Payment Methods



#### threeDSecureInitSettings

With `Use 3D Secure` enabled, the merchant is required to configure the following 3DS Settings accordingly.

- **3DS Transaction Type**

  - Each option provides context about the nature of the transaction, helping to ensure accurate processing and risk assessment.

  - | Option                     | Description                                                  |
    | -------------------------- | ------------------------------------------------------------ |
    | `"GOODS_SERVICE_PURCHASE"` | Indicates a purchase of goods or services. This is the most common type of transaction for retail or eCommerce. |
    | `"CHECK_ACCEPTANCE"`       | Refers to the acceptance of a check as a form of payment.    |
    | `"ACCOUNT_FUNDING"`        | Represents a transaction that involves funding an account, such as adding funds to a digital wallet or prepaid card. |
    | `"QUSAI_CASH_TRANSACTION"` | Refers to transactions similar to cash withdrawals, such as purchasing traveler's checks, foreign currency, or gambling tokens. |
    | `"PREPAID_ACTIVATION"`     | Refers to activating a prepaid account or card, often involving an initial funding transaction. |



## Iframe Configuration

Throughout this documentation, we are using the following iframe configuration.

> 📘 Note
>
> For the `cardSettings`, we pretty much omit all of the customer-related information since we are passing it all before the checkout from the nopCommerce (current) customer.
>
> Typically, the Bluefin Integrations team creates this iframe configuration for the merchant if they don't want go into details of our APIs.

**POST** `/api/v4/accounts/{accountId}/payment-iframe`

```json
{
  "label": "Multi-Payment iframe",
  "language": "ENGLISH",
  "timeout": 600,
  "allowedPaymentMethods": [
    "CARD",
    "ACH",
    "GOOGLE_PAY",
    "CLICK_TO_PAY"
  ],
  "allowedParentDomains": [
    "demo.nopcommerce.com"
  ],
  "cardSettings": {
    "cvv": "required",
    "billingAddress": {
      "address1": "omit",
      "address2": "omit",
      "city": "omit",
      "state": "omit",
      "zip": "omit"
    },
    "capturePhone": "omit",
    "threeDSecure": "omit",
    "captureEmail": "omit",
    "captureShippingAddress": false
  },
  "achSettings": {
    "billingAddress": {
      "address1": "required",
      "address2": "optional",
      "city": "required",
      "state": "required",
      "zip": "required"
    },
    "capturePhone": "omit",
    "captureEmail": "omit",
    "captureShippingAddress": false
  },
  "clickToPaySettings": {
    "srcDpaId": "3fa85f65-6728-4562-b3fd-2c963f66afa6",
    "dpaName": "PayConex",
    "dpaPresentationName": "PayConex App",
    "allowedCardBrands": [
      "VISA",
      "MASTERCARD",
      "AMERICAN_EXPRESS",
      "DISCOVER",
      "CHINA_UNION_PAY"
    ]
  },
  "googlePaySettings": {
    "merchantId": "12345678901234567890",
    "merchantName": "Demo Merchant",
    "billingAddressRequired": false,
    "shippingAddressRequired": false,
    "emailRequired": true,
    "billingAddressParameters": {
      "format": "MIN",
      "phoneNumberRequired": false
    },
    "shippingAddressParameters": {
      "allowedCountryCodes": ["US"],
      "phoneNumberRequired": false
    },
    "allowedAuthMethods": [
      "PAN_ONLY",
      "CRYPTOGRAM_3DS"
    ],
    "allowedCardBrands": [
      "VISA",
      "MASTERCARD",
      "AMERICAN_EXPRESS",
      "DISCOVER",
      "JCB",
      "INTERAC"
    ],
    "threeDSecure": "omit"
  },
  "currency": "USD",
  "savePaymentOption": "required"
}
```

For the full breakdown of these settings, dive into [Creating a Configuration](https://developers.bluefin.com/payconex/v4/reference/creating-a-configuration).

> In response, we receive the iFrame Configuration Identifier that is used to configure the plugin.



## Customer Checkout

After configuring the plugin, we need some Products we can check out with and test the Bluefin payment method.

If you are in the nopCommerce sandbox environment, it is necessary to create some Products so that we have something to check out.

This can be accomplished by going to [Admin Area](https://docs.nopcommerce.com/en/getting-started/admin-area-overview.html) -> Catalog -> Products -> Add new.



Next, we add the Product to the cart and go to the checkout.



### Checkout

The Checkout Component securely transmits payment details directly to Bluefin's system, mitigating risk while maintaining a smooth and user-friendly checkout experience. This also reduces [PCI compliance scope](https://developers.bluefin.com/payconex/v4/reference/payconex-introduction#pci-scope) and enhances security.

For all the examples of the payment methods, various checkout scenarios, and technical details, check out the [Customer Checkout](https://developers.bluefin.com/payconex/v4/reference/customer-checkout).

Even though the Bluefin Checkout Component supports inputting billing and shipping address, the billing and shipping address is filled out and managed via nopCommerce. These are then passed onto the Checkout Component so that the customer is not required to reinput the same fields.





#### Selecting Payment Method

Now, we select the Bluefin Payment method.

> 🚧 Note
>
> This requires the plugin to be enabled/active from nopCommerce.

screenshot







#### Pickup

The plugin also implements the logic where the nopCommerce pickup option is selected as the shipping address.





#### Omitting Shipping Address

If the shipping address is omitted, the plugin is capable of handling that scenario by completely excluding the shipping address from the Bluefin transaction itself as well.

In nopCommerce, the shipping is enabled _per product_. Thus, in order to omit it, the merchant needs to go to [Admin Area](https://docs.nopcommerce.com/en/getting-started/admin-area-overview.html) -> Catalog -> Products, select their desired product, and scroll down to `Shipping`.



screenshot



> 🚧 Note
>
> If the merchant configures their plugin not to use the shipping address while using the 3D Secure, then the `shippingIndicator` is required to be `BILLING_ADDRESS`.
>
> If, at least, one product is the cart requires shipping, the entire checkout will require the shipping address.





### Confirming Order

