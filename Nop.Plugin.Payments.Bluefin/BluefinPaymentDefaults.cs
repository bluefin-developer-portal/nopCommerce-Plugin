namespace Nop.Plugin.Payments.Bluefin;


public class BluefinPaymentDefaults
{
    public static string ScriptPath => "https://secure.payconex.net/iframe-v2/iframe-v2.0.0.min.js";

    public static string CheckoutComponentCERT => "https://checkout-cert.payconex.net";
    public static string CheckoutComponentPROD => "https://checkout.payconex.net";

    public static string certEnv => "https://api-cert.payconex.net";
    public static string prodEnv => "https://api.payconex.net";
}