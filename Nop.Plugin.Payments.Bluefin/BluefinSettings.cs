using Nop.Core.Configuration;


namespace Nop.Plugin.Payments.Bluefin;

public class BluefinPaymentSettings : ISettings
{

    #region Credentials
    public string ApiKeyId { get; set; }

    public string AccountId { get; set; }

    public string ApiKeySecret { get; set; }
    #endregion

    #region Essential
    public bool UseSandbox { get; set; }
    public string IFrameConfigId { get; set; }

    public bool UseAuthorizeOnly { get; set; }

    public bool EnableLogging { get; set; }
    #endregion

    #region Iframe Settings
    public bool IframeResponsive { get; set; } = false;
    public string IframeWidth { get; set; } = "100%";
    public string IframeHeight { get; set; } = "600px";

    public bool EnableCard { get; set; } = true;
    public bool EnableACH { get; set; } = false;
    public bool EnableGooglePay { get; set; } = false;
    public bool EnableClickToPay { get; set; } = false;
    #endregion

    #region threeDSecure
    public bool Use3DS { get; set; }
    public string ThreeDTransType { get; set; }
    public string DeliveryTimeFrame { get; set; }
    public string ThreeDSecureChallengeIndicator { get; set; }
    public string ReorderIndicator { get; set; }
    public string ShippingIndicator { get; set; }
    #endregion
}