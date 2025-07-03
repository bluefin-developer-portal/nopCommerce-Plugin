using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Payments.Bluefin.Models;

public record ConfigurationModel : BaseNopModel
{
    [NopResourceDisplayName("Plugins.Payments.Bluefin.Fields.UseSandbox")]
    public bool UseSandbox { get; set; }
    public bool UseSandbox_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.Payments.Bluefin.Fields.ApiKeyId")]
    public string ApiKeyId { get; set; }
    public string ApiKeyId_OverrideForStore { get; set; } // TODO: Override other settings per store

    [NopResourceDisplayName("Plugins.Payments.Bluefin.Fields.ApiKeySecret")]
    [DataType(DataType.Password)]
    public string ApiKeySecret { get; set; }

    public string AccountId { get; set; }

    [NopResourceDisplayName("Plugins.Payments.Bluefin.Fields.iFrameConfigId")]
    public string IFrameConfigId { get; set; }

    [NopResourceDisplayName("Plugins.Payments.Bluefin.Fields.Use3DS")]
    public bool Use3DS { get; set; }

    [NopResourceDisplayName("Plugins.Payments.Bluefin.Fields.UseAuthorizeOnly")]
    public bool UseAuthorizeOnly { get; set; }

    [NopResourceDisplayName("Plugins.Payments.Bluefin.Fields.EnableLogging")]
    public bool EnableLogging { get; set; }
}