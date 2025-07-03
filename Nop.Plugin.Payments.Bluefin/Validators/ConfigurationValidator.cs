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

    }

    #endregion
}