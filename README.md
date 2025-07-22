## nopCommerce Plugin Information

Being a payment method plugin, here is its basic information with CNP transactions it supports.

| Friendly name | System name      | Supports capture | Refund | Partial refund | Void | Recurring support |
| :------------ | :--------------- | :--------------- | :----- | :------------- | ---- | :---------------- |
| Bluefin       | Payments.Bluefin | ✔                | ✔      | ✔              | ✗    | Not supported     |

## Overview

This nopCommerce plugin combines the Bluefin Checkout Component and REST API, constituting the complete ready-to-use Bluefin payment method for nopCommerce platform.

The checkout component supports Card Payment, Google Pay, Mastercard Click to Pay, proving an all-in comprehensive eCommerce payment solution. 

The plugin requires the merchant integration with the Bluefin Gateway where the integration team sets up your configuration according to your needs. The merchant is free to customize their iframe configuration and configure their payment method options on their own as they have gained enough experience while certifying with their Bluefin integration.

The plugin is built upon the Bluefin® PayConex™ REST API that connects to various PayConex™ services, thus serving as an HTTPS communication bridge to the PayConex™ Gateway.

> 📘 Note
>
> The merchant using this plugin is _not_ required to understand much of what's happening behind the scenes and how the Bluefin APIs are used.
>
> If you are interested in all the ins and outs, check out our [Comprehensive Documentation and Reference Materials](https://developers.bluefin.com/payconex/v4/reference/payconex-introduction).

Here are some of the key components that the Bluefin payment plugin offers to the merchant.

### **Bluefin Hosted Checkout Components**

- **Easy Integration:** Use our secure, pre-built Checkout Component UI via our SDK, designed for seamless integration into your existing systems.
- **Security:** These components are hosted on Bluefin's servers and handle all payment data input through an HTML iframe, ensuring that no sensitive credit card data reaches your servers.
- **Flexible Management and Configuration**: With a set of API endpoints, you can easily configure and create iframe payment instances, and effectively overwrite the configuration for a specific instance per customer. For more, see [Creating an Instance](https://developers.bluefin.com/payconex/v4/reference/creating-an-instance).
- **Tokenization:** Once the form is completed, it securely tokenizes the information for CNP transactions by communicating with the ShieldConex® tokenization service and utilizes a payment authentication service based on the type of payment method, e.g. 3DS(Credit or Debit Card), Google Account(Google Pay), ACH(Bank Information), Mastercard Click to Pay. After tokenization, a transaction is supposed to be processed during the PayConex™ token life-span (within 10 minutes).

### **Versatile Transaction Processing**

- **Security:** Bluefin ShieldConex® ensures that no sensitive card information is ever stored on your servers, significantly reducing the PCI scope.
- **Card Not Present Transactions:** Before processing, CNP transactions primarily rely on ShieldConex® for security. **ShieldConex®** does not store any sensitive cardholder data. Instead, it uses tokenization/detokenization on its vaultless tokens for online PII, PHI, payments and ACH account data. These tokens can be securely utilized or stored on the merchant's server, significantly reducing the vendor's or merchant's PCI footprint. However, if the merchant loses these tokens, they _cannot_ be recovered.
- **Transaction Types**: Our gateway supports a variety of the most common transaction types used on a day-to-day basis such as sale, authorization, store, capture, refund and credit.

### **3DS Support**

- **Security Backbone:** Besides the vaultless tokenization solution by ShieldConex®, Bluefin provides one of the security backbones for processing online CNP transactions, with iframe configurations that can fully integrate 3DS as a feature of PayConex™.
- **Fraud Prevention:** Implement 3DS to enhance fraud prevention and secure customer authentication.
- **Fraud Scoring:** Iframe configurations allow for anti-fraud service for extra authorization based on the score during transaction processing. Fraud Scoring is available through all our processors and help merchants score transactions based on rules. However, this is a feature on its own and it does _not_ need to be used with 3D Secure.
- **User Experience:** Ensure a smooth user experience while maintaining high security standards.
- **3DS MPI Simulation**: Bluefin 3DS Solution can be simulated in the certification environment for testing purposes.



## Installing the Plugin

To build and install our Bluefin Payment Plugin, please follow the [Plugins in nopCommerce](https://docs.nopcommerce.com/en/getting-started/advanced-configuration/plugins-in-nopcommerce.html) Guide for building and installing third-party plugins.



## Comprehensive Reference

For the comprehensive documentation on this nopCommerce plugin, please go to [our readme.io page](https://developers.bluefin.com/payconex/v4/reference/nopcommerce-plugin-for-bluefin).
