using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Payments.Bluefin.Models;

/// <summary>
/// Represents a payment info model
/// </summary>
public record TraceLogModel : BaseNopModel
{
    public int Id { get; set; }
    public string TraceId { get; set; }

    public string ErrorMessage { get; set; }

    public string Json { get; set; }
}