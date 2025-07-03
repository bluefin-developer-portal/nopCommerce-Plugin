using Nop.Core.Configuration;


namespace Nop.Plugin.Payments.Bluefin;

public class BluefinPaymentSettings : ISettings
{

    #region Credentials
    public string ApiKeyId { get; set; }

    public string AccountId { get; set; }

    public string ApiKeySecret { get; set; }
    #endregion

    #region Other
    public bool UseSandbox { get; set; }

    public bool Use3DS { get; set; }

    public string IFrameConfigId { get; set; }

    public bool UseAuthorizeOnly { get; set; }

    public bool EnableLogging { get; set; }
    #endregion
}