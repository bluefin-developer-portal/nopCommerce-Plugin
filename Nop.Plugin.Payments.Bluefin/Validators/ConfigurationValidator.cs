using FluentValidation;
using Nop.Plugin.Payments.Bluefin.Models;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace Nop.Plugin.Payments.Bluefin.Validators;


public class ConfigurationValidator : BaseNopValidator<ConfigurationModel>
{
    #region Ctor

    public ConfigurationValidator(ILocalizationService localizationService)
    {
        
        RuleFor(model => model.ApiKeyId)
            .NotEmpty()
            .WithMessageAwait(localizationService.GetResourceAsync("Plugins.Payments.Bluefin.Fields.ApiKeyId.Required"));

        RuleFor(model => model.ApiKeySecret)
            .NotEmpty()
            .WithMessageAwait(localizationService.GetResourceAsync("Plugins.Payments.Bluefin.Fields.ApiKeySecret.Required"));

        RuleFor(model => model.IFrameConfigId)
            .NotEmpty()
            .WithMessageAwait(localizationService.GetResourceAsync("Plugins.Payments.Bluefin.Fields.IFrameConfigId.Required"));

        RuleFor(model => model.AccountId)
            .NotEmpty()
            .WithMessageAwait(localizationService.GetResourceAsync("Plugins.Payments.Bluefin.Fields.AccountId.Required"));


        RuleFor(model => model.IframeWidth)
            .NotEmpty()
            .When(m => !m.IframeResponsive)
            .WithMessageAwait(localizationService.GetResourceAsync("Plugins.Payments.Bluefin.Fields.IframeWidth.Required"));


        RuleFor(model => model.IframeHeight)
            .NotEmpty()
            .When(m => !m.IframeResponsive)
            .WithMessageAwait(localizationService.GetResourceAsync("Plugins.Payments.Bluefin.Fields.IframeHeight.Required"));

        RuleFor(model => model.IframeTimeout)
            .NotEmpty()
            .WithMessageAwait(localizationService.GetResourceAsync("Plugins.Payments.Bluefin.Fields.IframeTimeout.Required"));

        RuleFor(m => m.EnableCard)
            .NotEmpty()
            .When(m => !(m.EnableCard || m.EnableACH || m.EnableGooglePay || m.EnableClickToPay))
            // .Must(v => v.Equals(true))
            .WithMessageAwait(localizationService.GetResourceAsync("Plugins.Payments.Bluefin.Fields.PaymentMethod.Required"));
    }

    #endregion
}